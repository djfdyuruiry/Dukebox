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
    public class ArtistListingViewModel : ViewModelBase, IArtistListingViewModel
    {
        private List<Artist> _artists;
        private ListSearchHelper<Artist> _listSearchHelper;
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
        public List<Artist> Artists
        {
            get
            {
                return _listSearchHelper.FilteredItems;
            }
        }

        public ArtistListingViewModel() : base()
        {
            ClearSearch = new RelayCommand(() => SearchText = string.Empty);

            _artists = new List<Artist>()
            {
                new Artist(){ Title = "Times" },
                new Artist(){ Title = "Rave Madness" },
                new Artist(){ Title = "One" },
                new Artist(){ Title = "Jolata True" },
                new Artist(){ Title = "Rave Madness" },
                new Artist(){ Title = "One" },
                new Artist(){ Title = "Jolata True" },
                new Artist(){ Title = "Rave Madness" },
                new Artist(){ Title = "One" },
                new Artist(){ Title = "Jolata True" },
                new Artist(){ Title = "Rave Madness" },
                new Artist(){ Title = "One" },
                new Artist(){ Title = "Jolata True" },
                new Artist(){ Title = "Rave Madness" },
                new Artist(){ Title = "One" }
            };

            _artists = _artists.OrderBy(a => a.Title).ToList();

            _listSearchHelper = new ListSearchHelper<Artist>
            {
                Items = _artists,
                FilterLambda = Artist.Filter
            };
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via songs property
            OnPropertyChanged("Artists");
        }
    }
}
