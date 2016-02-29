﻿using Dukebox.Desktop.Helper;
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
    public class ArtistListingViewModel : ViewModelBase, IArtistListingViewModel
    {
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
        }

        public ArtistListingViewModel() : base()
        {
            ClearSearch = new RelayCommand(() => SearchText = string.Empty);

            _artists = new List<artist>()
            {
                new artist(){ name = "Times" },
                new artist(){ name = "Rave Madness" },
                new artist(){ name = "One" },
                new artist(){ name = "Jolata True" },
                new artist(){ name = "Rave Madness" },
                new artist(){ name = "One" },
                new artist(){ name = "Jolata True" },
                new artist(){ name = "Rave Madness" },
                new artist(){ name = "One" },
                new artist(){ name = "Jolata True" },
                new artist(){ name = "Rave Madness" },
                new artist(){ name = "One" },
                new artist(){ name = "Jolata True" },
                new artist(){ name = "Rave Madness" },
                new artist(){ name = "One" }
            };

            _artists = _artists.OrderBy(a => a.name).ToList();

            _listSearchHelper = new ListSearchHelper<artist>
            {
                Items = _artists,
                FilterLambda = (a, s) => a.name.ToLower().Contains(s.ToLower())
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
