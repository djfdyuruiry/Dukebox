using Dukebox.Desktop.Interfaces;

namespace Dukebox.Desktop.ViewModel
{
    public class ViewModelLocator
    {
        public ILoadingScreenViewModel LoadingScreenViewModel
        {
            get
            {
                return DesktopContainer.GetInstance<ILoadingScreenViewModel>();
            }
        }

        // window view model
        public IMainWindowViewModel MainWindow
        {
            get
            {
                return DesktopContainer.GetInstance<IMainWindowViewModel>();
            }
        }
        
        // menu view models
        public IFileMenuViewModel FileMenuViewModel
        {
            get
            {
                return DesktopContainer.GetInstance<IFileMenuViewModel>();
            }
        }

        public IAudioCdMenuViewModel AudioCdMenuViewModel
        {
            get
            {
                return DesktopContainer.GetInstance<IAudioCdMenuViewModel>();
            }
        }

        public IPlaybackMenuViewModel PlaybackMenuViewModel
        {
            get
            {
                return DesktopContainer.GetInstance<IPlaybackMenuViewModel>();
            }
        }

        public IPlaylistMenuViewModel PlaylistMenuViewModel
        {
            get
            {
                return DesktopContainer.GetInstance<IPlaylistMenuViewModel>();
            }
        }

        public IHelpMenuViewModel HelpMenuViewModel
        {
            get
            {
                return DesktopContainer.GetInstance<IHelpMenuViewModel>();
            }
        }

        public ISettingsMenuViewModel SettingsMenuViewModel
        {
            get
            {
                return DesktopContainer.GetInstance<ISettingsMenuViewModel>();
            }
        }       

        // screen view models
        public IPlaybackMonitorViewModel IPlaybackMonitor
        {
            get
            {
                return DesktopContainer.GetInstance<IPlaybackMonitorViewModel>();
            }
        }
        
        public IAlbumListingViewModel IAlbumListing
        {
            get
            {
                return DesktopContainer.GetInstance<IAlbumListingViewModel>();
            }
        }

        public IArtistListingViewModel IArtistListing
        {
            get
            {
                return DesktopContainer.GetInstance<IArtistListingViewModel>();
            }
        }

        public ITrackListingViewModel TrackListingPreview
        {
            get
            {
                return DesktopContainer.GetInstance<TrackListingPreviewViewModel>();
            }
        }

        public ITrackListingViewModel CurrentlyPlayingListing
        {
            get
            {
                return DesktopContainer.GetInstance<CurrentlyPlayingListingViewModel>();
            }
        }

        public ITrackListingViewModel LibraryListing
        {
            get
            {
                return DesktopContainer.GetInstance<LibraryListingViewModel>();
            }
        }

        public ITrackListingViewModel RecentlyPlayedListing
        {
            get
            {
                return DesktopContainer.GetInstance<RecentlyPlayedListingViewModel>();
            }
        }

        public ITrackListingViewModel AudioCdListing
        {
            get
            {
                return DesktopContainer.GetInstance<AudioCdViewModel>();
            }
        } 
        
        public IMetadataColumnsSettingsViewModel IMetadataColumnsSettingsViewModel
        {
            get
            {
                return DesktopContainer.GetInstance<IMetadataColumnsSettingsViewModel>();
            }
        }
    }
}
