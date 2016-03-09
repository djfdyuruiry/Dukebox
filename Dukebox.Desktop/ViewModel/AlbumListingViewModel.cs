using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Services;
using Dukebox.Library;
using Dukebox.Library.Interfaces;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dukebox.Desktop.ViewModel
{
    public class AlbumListingViewModel : ViewModelBase, IAlbumListingViewModel, ISearchControlViewModel
    {
        private readonly IMusicLibrary _mediaLibrary;

        private List<Album> _albums;
        private ListSearchHelper<Album> _listSearchHelper;
        private string _searchText;

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
        public bool SearchEnabled
        {
            get
            {
                return true;
            }
        }
        public List<Album> Albums
        {
            get
            {
                return _listSearchHelper.FilteredItems;
            }
            private set
            {
                _albums = value;
                _listSearchHelper.Items = _albums;
                SearchText = string.Empty;

                OnPropertyChanged("Albums");
            }
        }

        public AlbumListingViewModel(IMusicLibrary mediaLibrary) : base()
        {
            _mediaLibrary = mediaLibrary;            
            _listSearchHelper = new ListSearchHelper<Album>
            {
                FilterLambda = Album.ContainsString
            };

            _mediaLibrary.AlbumAdded += (o, e) => LoadAlbumsFromLibrary();

            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadAlbumsFromLibrary();
        }

        private void LoadAlbumsFromLibrary()
        {
            Albums = _mediaLibrary.OrderedAlbums
                .Select(a => Album.BuildAlbumInstance(a))
                .ToList();
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via songs property
            OnPropertyChanged("Albums");
        }
    }
}
