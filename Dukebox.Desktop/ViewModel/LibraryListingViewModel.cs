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
    public class LibraryListingViewModel : ViewModelBase, ISongListingViewModel
    {
        private List<Song> _songs;
        private ListSearchHelper<Song> _listSearchHelper;
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
        public List<Song> Songs 
        { 
            get 
            {
                return _listSearchHelper.FilteredItems;
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

        public LibraryListingViewModel() : base()
        {
            ClearSearch = new RelayCommand(() => SearchText = string.Empty);

            _songs = new List<Song>()
            {
                new Song(){ Artist = "Bob Dylan", Album = "Times", Title = "Are a changin'" },
                new Song(){ Artist = "Marky Mark", Album = "Rave Madness", Title = "Good Vibrations" },
                new Song(){ Artist = "Metallica", Album = "One", Title = "Enter Sandman" },
                new Song(){ Artist = "Tracy Chapman", Album = "Jolata True", Title = "Fast Car" }
            };

            _listSearchHelper = new ListSearchHelper<Song>
            {
                Items = _songs,
                FilterLambda = Song.Filter
            };
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via songs property
            OnPropertyChanged("Songs");
        }
    }
}
