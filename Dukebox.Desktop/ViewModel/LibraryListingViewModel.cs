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
                Track.BuildTrackInstance(new album {name = "Times"}, new artist {name= "Bob Dylan"}, new song { title = "Are a changin'"}),
                Track.BuildTrackInstance(new album {name = "Times"}, new artist {name= "Bob Dylan"}, new song { title = "Are a changin'"}),
                Track.BuildTrackInstance(new album {name = "Times"}, new artist {name= "Bob Dylan"}, new song { title = "Are a changin'"}),
                Track.BuildTrackInstance(new album {name = "Times"}, new artist {name= "Bob Dylan"}, new song { title = "Are a changin'"}),
                Track.BuildTrackInstance(new album {name = "Times"}, new artist {name= "Bob Dylan"}, new song { title = "Are a changin'"})
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
