using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
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
    public class ArtistListingViewModel : ViewModelBase, IArtistListingViewModel, ISearchControlViewModel
    {
        private readonly IMusicLibrary _musicLibrary;

        private List<artist> _artists;
        private ListSearchHelper<artist> _listSearchHelper;
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
        public List<artist> Artists
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

        public ArtistListingViewModel(IMusicLibrary musicLibrary) : base()
        {
            _musicLibrary = musicLibrary;

            _listSearchHelper = new ListSearchHelper<artist>
            {
                FilterLambda = (a, s) => a.name.ToLower().Contains(s.ToLower())
            };

            _musicLibrary.ArtistAdded += (o, e) => RefreshArtistsFromLibrary();

            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            RefreshArtistsFromLibrary();
        }

        public void RefreshArtistsFromLibrary()
        {
            Artists = _musicLibrary.OrderedArtists;
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via songs property
            OnPropertyChanged("Artists");
        }
    }
}
