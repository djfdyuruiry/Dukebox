﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Services;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.ViewModel
{
    public class CurrentlyPlayingListingViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly IMusicLibrary _musicLibrary;
        
        private string _searchText;
        private readonly ListSearchHelper<ITrack> _listSearchHelper;

        public List<TrackWrapper> Tracks
        {
            get
            {
                return _listSearchHelper.FilteredItems.Select(t => new TrackWrapper(_musicLibrary, t)).ToList();
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
                return true;
            }
        }

        public Visibility ShowSearchControl
        {
            get
            {
                return Visibility.Visible;
            }
        }

        public ICommand ClearSearch { get; private set; }

        public ICommand LoadTrack { get; private set; }

        public CurrentlyPlayingListingViewModel(IAudioPlaylist audioPlaylist, IMusicLibrary musicLibrary) : base()
        {
            _audioPlaylist = audioPlaylist;
            _musicLibrary = musicLibrary;
            _listSearchHelper = new ListSearchHelper<ITrack>
            {
                FilterLambda = (t, s) => t.ToString().ToLower(CultureInfo.InvariantCulture)
                    .Contains(s.ToLower(CultureInfo.InvariantCulture))
            };

            _audioPlaylist.TrackListModified += (o, e) => RefreshTracks();
            
            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadTrack = new RelayCommand<ITrack>(DoLoadTrack);

            RefreshTracks();
        }

        private void RefreshTracks()
        {
            _listSearchHelper.Items = _audioPlaylist.Tracks.ToList();
            OnPropertyChanged("Tracks");
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
