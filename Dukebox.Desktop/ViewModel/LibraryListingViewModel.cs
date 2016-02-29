using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library;
using Dukebox.Model.Services;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dukebox.Desktop.ViewModel
{
    public class LibraryListingViewModel : ViewModelBase, ITrackListingViewModel
    {
        private List<Track> _tracks;
        private ListSearchHelper<Track> _listSearchHelper;
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
        public List<Track> Tracks 
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

            _tracks = new List<Track>()
            {
                new Track(){ Artist = new artist {name= "Bob Dylan"}, Album = new album {name = "Times"}, Song = new song { title = "Are a changin'"} },
                new Track(){ Artist = new artist {name= "Marky Mark"}, Album = new album {name = "Rave Madness"}, Song = new song { title = "Good Vibrations"} },
                new Track(){ Artist = new artist {name= "VNV Nation"}, Album = new album {name = "Matter+Form"}, Song = new song { title = "Lightwave"} },
                new Track(){ Artist = new artist {name= "Tracy Chapman"}, Album = new album {name = "Jolata True"}, Song = new song { title = "Fast Car"} }
            };

            _listSearchHelper = new ListSearchHelper<Track>
            {
                Items = _tracks,
                FilterLambda = (t, s) => t.Song.title.ToLower().Contains(s.ToLower())
            };
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via songs property
            OnPropertyChanged("Tracks");
        }
    }
}
