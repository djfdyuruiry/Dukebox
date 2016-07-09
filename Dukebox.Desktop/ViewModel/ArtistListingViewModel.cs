using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using System.Threading.Tasks;
using System.Threading;

namespace Dukebox.Desktop.ViewModel
{
    public class ArtistListingViewModel : ViewModelBase, IArtistListingViewModel, ISearchControlViewModel
    {
        private readonly ListSearchHelper<Artist> _listSearchHelper;
        private readonly IMusicLibraryRepository _musicLibraryRepo;
        private readonly IMusicLibraryCacheService _cacheService;
        private readonly IAudioPlaylist _audioPlaylist;

        private List<Artist> _artists;
        private string _searchText;
        private CancellationTokenSource _cancellationTokenSource;

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
        public List<Artist> Artists
        {
            get
            {
                return _listSearchHelper.FilteredItems;
            }
            private set
            {
                _artists = value;
                _listSearchHelper.Items = _artists;
                SearchText = string.Empty;

                OnPropertyChanged("Artists");
            }
        }
        public ICommand ClearSearch { get; private set; }
        public ICommand LoadArtist { get; private set; }

        public ArtistListingViewModel(IMusicLibraryRepository musicLibraryRepo, IMusicLibraryCacheService cacheService,
            IMusicLibraryEventService eventService, IAudioPlaylist audioPlaylist) : base()
        {
            _musicLibraryRepo = musicLibraryRepo;
            _cacheService = cacheService;
            _audioPlaylist = audioPlaylist;

            _listSearchHelper = new ListSearchHelper<Artist>
            {
                FilterLambda = (a, s) => a.Name.ToLower(CultureInfo.InvariantCulture).Contains(s.ToLower(CultureInfo.InvariantCulture)),
                SortResults = true,
                SortLambda = (a) => a.Name.ToLower(CultureInfo.InvariantCulture)
            };

            eventService.ArtistsAdded += (o, e) => RefreshArtistsFromLibrary();
            eventService.SongDeleted += (o, e) => RefreshArtistsFromLibrary();

            LoadArtist = new RelayCommand<Artist>(DoLoadArtist);
            ClearSearch = new RelayCommand(() => SearchText = string.Empty);

            RefreshArtistsFromLibrary();
        }

        public void RefreshArtistsFromLibrary()
        {
            Artists = _cacheService.OrderedArtists;
        }

        private void DoLoadArtist(Artist artist)
        {
            var tracks = _musicLibraryRepo.GetTracksForArtist(artist.Name);
            _audioPlaylist.LoadPlaylistFromList(tracks);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
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

                _listSearchHelper.SearchFilter = SearchText;
                
                OnPropertyChanged("Artists");
            }, taskCancelToken);
        }
    }
}
