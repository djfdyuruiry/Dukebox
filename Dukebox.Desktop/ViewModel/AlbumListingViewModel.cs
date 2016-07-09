using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Services;
using LibraryAlbum = Dukebox.Library.Model.Album;
using Dukebox.Library.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Dukebox.Desktop.ViewModel
{
    public class AlbumListingViewModel : ViewModelBase, IAlbumListingViewModel, ISearchControlViewModel
    {
        private readonly IMusicLibraryRepository _musicLibraryRepo;
        private readonly IMusicLibraryCacheService _cacheService;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly ListSearchHelper<Services.Album> _listSearchHelper;
        
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
        public List<Services.Album> Albums
        {
            get
            {
                return _listSearchHelper.FilteredItems;
            }
            private set
            {
                _listSearchHelper.Items = value;
                SearchText = string.Empty;

                OnPropertyChanged("Albums");
            }
        }
        public ICommand ClearSearch { get; private set; }
        public ICommand LoadAlbum { get; private set; }

        public AlbumListingViewModel(IMusicLibraryRepository musicLibraryRepo, IMusicLibraryCacheService cacheService, 
            IMusicLibraryEventService eventService, IAudioPlaylist audioPlaylist) : base()
        {
            _musicLibraryRepo = musicLibraryRepo;
            _cacheService = cacheService;
            _audioPlaylist = audioPlaylist;
            _listSearchHelper = new ListSearchHelper<Services.Album>
            {
                FilterLambda = Services.Album.ContainsString,
                SortResults = true,
                SortLambda = (a) => a.Data.Name.ToLower(CultureInfo.InvariantCulture)
            };

            eventService.AlbumsAdded += (o, e) => LoadAlbumsFromLibrary();
            eventService.SongDeleted += (o, e) => LoadAlbumsFromLibrary();

            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadAlbum = new RelayCommand<Services.Album>(this.DoLoadAlbum);

            LoadAlbumsFromLibrary();
        }

        private void LoadAlbumsFromLibrary()
        {
            var albums = _cacheService.OrderedAlbums
                .Select(a => Album.BuildAlbumInstance(a))
                .ToList();

            Albums = albums;
        }

        private void DoLoadAlbum(Album album)
        {
            var tracks = _musicLibraryRepo.GetTracksForAlbum((album.Data as LibraryAlbum).Name);
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

                // trigger filtered items call via songs property
                OnPropertyChanged("Albums");
            }, taskCancelToken);
        }
    }
    }
}
