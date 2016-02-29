using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library;
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
        private List<album> _albums;
        private ListSearchHelper<album> _listSearchHelper;
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
        public List<album> Albums
        {
            get
            {
                return _listSearchHelper.FilteredItems;
            }
        }

        public AlbumListingViewModel() : base()
        {
            ClearSearch = new RelayCommand(() => SearchText = string.Empty);

            _albums = new List<album>()
            {
                new album(){ name = "Times" },
                new album(){ name = "Rave Madness" },
                new album(){ name = "One" },
                new album(){ name = "Jolata True" },
                new album(){ name = "Rave Madness" },
                new album(){ name = "One" },
                new album(){ name = "Jolata True" },
                new album(){ name = "Rave Madness" },
                new album(){ name = "One" },
                new album(){ name = "Jolata True" },
                new album(){ name = "Rave Madness" },
                new album(){ name = "One" },
                new album(){ name = "Jolata True" },
                new album(){ name = "Rave Madness" },
                new album(){ name = "One" }
            };

            _albums = _albums.OrderBy(a => a.name).ToList();

            _listSearchHelper = new ListSearchHelper<album>
            {
                Items = _albums,
                FilterLambda = (a, s) => a.name.ToLower().Contains(s.ToLower())
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
