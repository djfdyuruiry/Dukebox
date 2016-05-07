﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.ViewModel
{
    public class CurrentlyPlayingListingViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IAudioPlaylist _audioPlaylist;
        
        private List<ITrack> _tracks;
        private string _searchText;
        private ListSearchHelper<ITrack> _listSearchHelper;

        public List<ITrack> Tracks
        {
            get
            {
                return _listSearchHelper.FilteredItems;
            }
            private set
            {
                _tracks = value;

                _listSearchHelper.Items = _tracks;
                OnPropertyChanged("Tracks");
            }
        }

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

        public bool EditingListingsDisabled
        {
            get
            {
                return false;
            }
        }

        public ICommand ClearSearch { get; private set; }

        public ICommand LoadTrack { get; private set; }

        public CurrentlyPlayingListingViewModel(IAudioPlaylist audioPlaylist) : base()
        {
            _audioPlaylist = audioPlaylist;
            _listSearchHelper = new ListSearchHelper<ITrack>
            {
                FilterLambda = (t, s) => t.ToString().ToLower().Contains(s.ToLower())
            };

            _audioPlaylist.TrackListModified += (o, e) => RefreshTracks();
            
            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadTrack = new RelayCommand<ITrack>(DoLoadTrack);

            RefreshTracks();
        }

        private void RefreshTracks()
        {
            Tracks = _audioPlaylist.Tracks.ToList();
        }

        private void DoLoadTrack(ITrack track)
        {
            _audioPlaylist.SkipToTrack(track);
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via Tracks property
            OnPropertyChanged("Tracks");
        }
    }
}
