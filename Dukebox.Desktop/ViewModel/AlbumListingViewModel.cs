using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dukebox.Desktop.ViewModel
{
    public class AlbumListingViewModel : ViewModelBase, IAlbumListingViewModel
    {
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
        }

        public AlbumListingViewModel() : base()
        {
            ClearSearch = new RelayCommand(() => SearchText = string.Empty);

            _albums = new List<Album>()
            {
                new Album(){ Title = "Times" },
                new Album(){ Title = "Rave Madness" },
                new Album(){ Title = "One" },
                new Album(){ Title = "Jolata True" },
                new Album(){ Title = "Rave Madness" },
                new Album(){ Title = "One" },
                new Album(){ Title = "Jolata True" },
                new Album(){ Title = "Rave Madness" },
                new Album(){ Title = "One" },
                new Album(){ Title = "Jolata True" },
                new Album(){ Title = "Rave Madness" },
                new Album(){ Title = "One" },
                new Album(){ Title = "Jolata True" },
                new Album(){ Title = "Rave Madness" },
                new Album(){ Title = "One" }
            };

            _albums = _albums.OrderBy(a => a.Title).ToList();

            _listSearchHelper = new ListSearchHelper<Album>
            {
                Items = _albums,
                FilterLambda = Album.Filter
            };
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via songs property
            OnPropertyChanged("Albums");
        }
    }
}
