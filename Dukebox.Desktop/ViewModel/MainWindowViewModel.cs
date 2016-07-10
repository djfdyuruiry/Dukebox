using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using log4net;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IAudioPlaylist _audioPlaylist;
        
        private Visibility _showLibraryListing;
        private Visibility _showAlbumListing;
        private Visibility _showArtistListing;
        private Visibility _showRecentlyPlayedListing;
        private Visibility _showPlaylistListing;
        private Visibility _showAudioCdListing;
        private string _importReportText;

        public Visibility ShowCurrentlyPlayingListing
        {
            get
            {
                return _showLibraryListing;
            }
            private set
            {
                _showLibraryListing = value;
                OnPropertyChanged("ShowCurrentlyPlayingListing");
            }
        }
        public Visibility ShowLibraryListing 
        { 
            get 
            { 
                return _showLibraryListing; 
            } 
            private set 
            { 
                _showLibraryListing = value; 
                OnPropertyChanged("ShowLibraryListing"); 
            } 
        }
        public Visibility ShowAlbumListing
        {
            get
            {
                return _showAlbumListing;
            }
            private set
            {
                _showAlbumListing = value;
                OnPropertyChanged("ShowAlbumListing");
                OnPropertyChanged("ShowTrackListingPreview");
            }
        }
        public Visibility ShowArtistListing
        {
            get
            {
                return _showArtistListing;
            }
            private set
            {
                _showArtistListing = value;
                OnPropertyChanged("ShowArtistListing");
                OnPropertyChanged("ShowTrackListingPreview"); 
            }
        }
        public Visibility ShowRecentlyPlayedListing
        {
            get
            {
                return _showRecentlyPlayedListing;
            }
            private set
            {
                _showRecentlyPlayedListing = value;
                OnPropertyChanged("ShowRecentlyPlayedListing");
            }
        }
        public Visibility ShowPlaylistListing
        {
            get
            {
                return _showPlaylistListing;
            }
            private set
            {
                _showPlaylistListing = value;
                OnPropertyChanged(nameof(ShowPlaylistListing));
                OnPropertyChanged(nameof(ShowTrackListingPreview));
            }
        }
        public Visibility ShowAudioCdListing
        {
            get
            {
                return _showAudioCdListing;
            }
            private set
            {
                _showAudioCdListing = value;
                OnPropertyChanged("ShowAudioCdListing");
            }
        }        
        public Visibility ShowTrackListingPreview
        {
            get
            {
                return (ShowAlbumListing == Visibility.Visible || 
                        ShowArtistListing == Visibility.Visible || 
                        ShowPlaylistListing == Visibility.Visible) ? 
                    Visibility.Visible : 
                    Visibility.Hidden;
            }
        }

        public string ImportReportText
        {
            get
            {
                return _importReportText;
            }
            set
            {
                _importReportText = value;
                OnPropertyChanged("ImportReportText");
            }
        }

        public ICommand ShowLoadingScreen { get; private set; }
        public ICommand NavBarItemClickCommand { get; private set; }
        public ICommand StopAudio { get; private set; }

        public MainWindowViewModel(IAudioPlaylist audioPlaylist, IWatchFolderManagerService watchFolderManager) : base()
        {
            _audioPlaylist = audioPlaylist;

            ShowCurrentlyPlayingListing = Visibility.Hidden;
            ShowLibraryListing = Visibility.Visible;
            ShowAlbumListing = Visibility.Hidden;
            ShowArtistListing = Visibility.Hidden;
            ShowRecentlyPlayedListing = Visibility.Hidden;
            ShowPlaylistListing = Visibility.Hidden;
            ShowAudioCdListing = Visibility.Hidden;

            ShowLoadingScreen = new RelayCommand(DoShowSplashScreen);
            NavBarItemClickCommand = new RelayCommand<string>(NavBarItemClicked);
            StopAudio = new RelayCommand(DoStopAudio);
            
            // Enable passing message to show currently playing view
            Messenger.Default.Register<NotificationMessage>(this, (nm) =>
            {
                if (nm.Notification == NotificationMessages.AudioPlaylistLoadedNewTracks)
                {
                    NavBarItemClicked(NavIconNames.CurrentlyPlaying);
                }
            });

            watchFolderManager.WatchFolderServiceProcessedEvent += (o, e) => ImportReportText = e.GetReportString();
            ImportReportText = string.Empty;
        }

        private void DoShowSplashScreen()
        {
            var splashScreen = new LoadingScreen();
            splashScreen.DataContext = DesktopContainer.GetInstance<ILoadingScreenViewModel>();

            splashScreen.Show();
        }

        private void DoStopAudio()
        {
            try
            {
                _audioPlaylist.StopPlaylistPlayback();
            }
            catch (Exception ex)
            {
                logger.Error("Error when stopping audio on window close", ex);
            }
        }

        private void NavBarItemClicked(string navIconName)
        {
            HideAllViews();

            if (navIconName == NavIconNames.CurrentlyPlaying)
            {
                ShowCurrentlyPlayingListing = Visibility.Visible;
            }
            else if (navIconName == NavIconNames.Library)
            {
                ShowLibraryListing = Visibility.Visible;
            }
            else if (navIconName == NavIconNames.Albums)
            {
                ShowAlbumListing = Visibility.Visible;
            }
            else if (navIconName == NavIconNames.Artists)
            {
                ShowArtistListing = Visibility.Visible;
            }            
            else if (navIconName == NavIconNames.RecentlyPlayed)
            {
                ShowRecentlyPlayedListing = Visibility.Visible;
            }
            else if (navIconName == NavIconNames.PlaylistListing)
            {
                ShowPlaylistListing = Visibility.Visible;
            }
            else if (navIconName == NavIconNames.AudioCd)
            {
                ShowAudioCdListing = Visibility.Visible;
            }
        }

        private void HideAllViews()
        {
            ShowLibraryListing = Visibility.Hidden;
            ShowAlbumListing = Visibility.Hidden;
            ShowArtistListing = Visibility.Hidden;
            ShowRecentlyPlayedListing = Visibility.Hidden;
            ShowPlaylistListing = Visibility.Hidden;
            ShowAudioCdListing = Visibility.Hidden;
        }
    }
}
