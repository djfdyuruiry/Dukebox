using System.Linq;
using System.Windows;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Factories;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Services;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.ViewModel
{
    public class RecentlyPlayedListingViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IRecentlyPlayedRepository _recentlyPlayedRepo;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly TrackSourceFactory _trackSourceFactory;

        private string _searchText;
        private VirtualizingObservableCollection<ITrack> _tracksCollection;

        public ICommand ClearSearch { get; private set; }
        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;

                // run search on search text change
                DoSearch();
                OnPropertyChanged("SearchText");
            }
        }
        public VirtualizingObservableCollection<ITrack> Tracks
        {
            get
            {
                return _tracksCollection;
            }
            set
            {
                _tracksCollection = value;
                OnPropertyChanged(nameof(Tracks));
            }
        }

        public bool EditingListingsDisabled
        {
            get
            {
                return true;
            }
        }
        public bool SearchEnabled
        {
            get
            {
                return false;
            }
        }

        public Visibility ShowSearchControl
        {
            get
            {
                return Visibility.Hidden;
            }
        }

        public ICommand LoadTrack { get; private set; }
        public ICommand EditTrack => null;

        public RecentlyPlayedListingViewModel(IRecentlyPlayedRepository recentlyPlayedRepo, IAudioPlaylist audioPlaylist, 
            TrackSourceFactory trackSourceFactory) : base()
        {
            _recentlyPlayedRepo = recentlyPlayedRepo;
            _audioPlaylist = audioPlaylist;
            _trackSourceFactory = trackSourceFactory;

            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadTrack = new RelayCommand<ITrack>(DoLoadTrack);

            _recentlyPlayedRepo.RecentlyPlayedListModified += (o, e) => RefreshRecentlyPlayedFromLibrary();
            RefreshRecentlyPlayedFromLibrary();
        }

        private void DoLoadTrack(ITrack track)
        {
            _audioPlaylist.LoadPlaylistFromList(Tracks.Select(t => t.Song.FileName).ToList());
            _audioPlaylist.SkipToTrack(track.Song.FileName);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        public void RefreshRecentlyPlayedFromLibrary()
        {
            var recentlyPlayedTracks = _recentlyPlayedRepo.RecentlyPlayedAsList;
            var trackSource = _trackSourceFactory.BuildLibraryOrFileTracksSource(recentlyPlayedTracks);

            var paginationManager = new PaginationManager<ITrack>(trackSource)
            {
                Provider = trackSource
            };

            paginationManager.PageSize = 50;
            paginationManager.MaxPages = 2;

            Tracks = new VirtualizingObservableCollection<ITrack>(paginationManager);
        }

        private void DoSearch()
        {
            // TODO: Search logic
        }
    }
}
