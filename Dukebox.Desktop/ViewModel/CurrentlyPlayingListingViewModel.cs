using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using AlphaChiTech.Virtualization;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Services;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.ViewModel
{
    public class CurrentlyPlayingListingViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly IMusicLibraryUpdateService _musicLibraryUpdateService;
        private readonly IMusicLibraryEventService _eventService;
        private readonly IMusicLibrarySearchService _searchService;

        private string _searchText;
        private readonly ListSearchHelper<string> _listSearchHelper;

        private VirtualizingObservableCollection<ITrack> _tracksCollection;

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

        public bool SearchEnabled
        {
            get
            {
                return true;
            }
        }

        public bool EditingListingsDisabled
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

        public ICommand ClearSearch { get; private set; }

        public ICommand LoadTrack { get; private set; }

        public CurrentlyPlayingListingViewModel(IAudioPlaylist audioPlaylist, IMusicLibraryUpdateService musicLibraryUpdateService, 
            IMusicLibraryEventService eventService, IMusicLibrarySearchService searchService) : base()
        {
            _audioPlaylist = audioPlaylist;
            _musicLibraryUpdateService = musicLibraryUpdateService;
            _eventService = eventService;
            _searchService = searchService;

            _listSearchHelper = new ListSearchHelper<string>
            {
                FilterLambda = (t, s) => t.ToLower(CultureInfo.InvariantCulture)
                    .Contains(s.ToLower(CultureInfo.InvariantCulture))
            };

            _audioPlaylist.TrackListModified += (o, e) => RefreshTracks();
            
            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadTrack = new RelayCommand<ITrack>(DoLoadTrack);

            RefreshTracks();
        }

        private void RefreshTracks()
        {
            //_listSearchHelper.Items = _audioPlaylist.Tracks.ToList();

            var audioPlaylistTracks = _audioPlaylist.Tracks.ToList();
            var trackSource = new LibraryOrFileTracksSource(audioPlaylistTracks, _searchService);

            var paginationManager = new PaginationManager<ITrack>(trackSource)
            {
                Provider = trackSource
            };

            paginationManager.PageSize = 50;
            paginationManager.MaxPages = 2;

            Tracks = new VirtualizingObservableCollection<ITrack>(paginationManager);
        }

        private void DoLoadTrack(ITrack track)
        {
            _audioPlaylist.SkipToTrack(track.Song.FileName);
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via Tracks property
            OnPropertyChanged("Tracks");
        }
    }
}
