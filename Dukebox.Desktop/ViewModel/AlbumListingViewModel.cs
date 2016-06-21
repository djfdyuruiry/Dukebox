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

namespace Dukebox.Desktop.ViewModel
{
    public class AlbumListingViewModel : ViewModelBase, IAlbumListingViewModel, ISearchControlViewModel
    {
        private readonly IMusicLibrary _musicLibrary;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly ListSearchHelper<Services.Album> _listSearchHelper;
        
        private string _searchText;

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

        public AlbumListingViewModel(IMusicLibrary musicLibrary, IAudioPlaylist audioPlaylist) : base()
        {
            _musicLibrary = musicLibrary;
            _audioPlaylist = audioPlaylist;
            _listSearchHelper = new ListSearchHelper<Services.Album>
            {
                FilterLambda = Services.Album.ContainsString,
                SortResults = true,
                SortLambda = (a) => a.Data.Name.ToLower(CultureInfo.InvariantCulture)
            };

            _musicLibrary.AlbumAdded += (o, e) => LoadAlbumsFromLibrary();

            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadAlbum = new RelayCommand<Services.Album>(this.DoLoadAlbum);

            LoadAlbumsFromLibrary();
        }

        private void LoadAlbumsFromLibrary()
        {
            var albums = _musicLibrary.OrderedAlbums
                .Select(a => Album.BuildAlbumInstance(a))
                .ToList();

            Albums = albums;
        }

        private void DoLoadAlbum(Album album)
        {
            var tracks = _musicLibrary.GetTracksForAlbum((album.Data as LibraryAlbum).Name);
            _audioPlaylist.LoadPlaylistFromList(tracks);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via songs property
            OnPropertyChanged("Albums");
        }
    }
}
