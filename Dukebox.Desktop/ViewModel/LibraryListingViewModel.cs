using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Services;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Desktop.ViewModel
{
    public class LibraryListingViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IMusicLibraryUpdateService _musicLibraryUpdateService;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly IMusicLibrarySearchService _musicLibrarySearcher;
        private readonly IMusicLibraryEventService _eventService;

        private List<ITrack> _tracks;
        private string _searchText;
        private CancellationTokenSource _cancellationTokenSource;

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

                // run search on key press
                DoSearch();
                OnPropertyChanged("SearchText");
            }
        }
        public List<TrackWrapper> Tracks 
        { 
            get 
            {
                return _tracks.Select(t => new TrackWrapper(_musicLibraryUpdateService, _eventService, t)).ToList();
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
                return true;
            }
        }

        public Visibility ShowSearchControl
        {
            get
            {
                return Visibility.Visible;
            }
        }

        public ICommand LoadTrack { get; private set; }


        public LibraryListingViewModel(IMusicLibraryUpdateService musicLibraryUpdateService, IMusicLibraryEventService eventService, 
            IMusicLibrarySearchService musicLibrarySearcher, IAudioPlaylist audioPlaylist) : base()
        {
            _musicLibraryUpdateService = musicLibraryUpdateService;
            _audioPlaylist = audioPlaylist;
            _musicLibrarySearcher = musicLibrarySearcher;
            _eventService = eventService;

            _eventService.SongsAdded += (o, e) => RefreshTrackListing();
            _eventService.SongAdded += (o, e) => RefreshTrackListing();
            _eventService.SongDeleted += (o, e) => RefreshTrackListing();

            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadTrack = new RelayCommand<ITrack>(DoLoadTrack);

            _tracks = new List<ITrack>();

            RefreshTrackListing();
        }

        private void DoLoadTrack(ITrack track)
        {
            _audioPlaylist.LoadPlaylistFromList(_tracks, false);
            _audioPlaylist.SkipToTrack(track);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void RefreshTrackListing()
        {
            SearchText = string.Empty;
            DoSearch();
        }

        private void DoSearch()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var taskCancelToken = _cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                await Task.Delay(250);
                taskCancelToken.ThrowIfCancellationRequested();

                _tracks = _musicLibrarySearcher.SearchForTracksInArea(SearchAreas.All, SearchText);
                OnPropertyChanged("Tracks");
            }, taskCancelToken);
        }
    }
}
