using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using AlphaChiTech.Virtualization;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
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
        private readonly ILibraryTracksSource _libraryTrackSource;

        private List<ITrack> _tracks;
        private VirtualizingObservableCollection<ITrack> _virtualTrackCollection = null;
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
                OnPropertyChanged(nameof(SearchText));
            }
        }
        public VirtualizingObservableCollection<ITrack> Tracks 
        { 
            get
            {
                return _virtualTrackCollection;
                //return _tracks.Select(t => new TrackWrapper(_musicLibraryUpdateService, _eventService, t)).ToList();
            }
            private set
            {
                _virtualTrackCollection = value;
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
            IMusicLibrarySearchService musicLibrarySearcher, IAudioPlaylist audioPlaylist, ILibraryTracksSource libraryTrackSource) : base()
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

            _libraryTrackSource = libraryTrackSource;

            RefreshTrackListing();
        }

        private void DoLoadTrack(ITrack track)
        {
            var tracks = _libraryTrackSource
                .GetItemsAt(0, _libraryTrackSource.Count, true)
                .Items
                .Select(t => t.Song.FileName)
                .ToList();

            _audioPlaylist.LoadPlaylistFromList(tracks, false);

            _audioPlaylist.SkipToTrack(track.Song.FileName);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void RefreshTrackListing()
        {
            SearchText = string.Empty;

            var paginationManager = new PaginationManager<ITrack>(_libraryTrackSource)
            {
                Provider = _libraryTrackSource
            };

            paginationManager.PageSize = 50;
            paginationManager.MaxPages = 2;

            Tracks = new VirtualizingObservableCollection<ITrack>(paginationManager);
            
            /*
            DoSearch();
            */
        }

        private void DoSearch()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var taskCancelToken = _cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                await Task.Delay(250);

                if (taskCancelToken.IsCancellationRequested)
                {
                    return;
                }

                //_tracks = _musicLibrarySearcher.SearchForTracksInArea(SearchAreas.All, SearchText);
                OnPropertyChanged(nameof(Tracks));
            }, taskCancelToken);
        }
    }
}
