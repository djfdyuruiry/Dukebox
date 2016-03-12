using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library;
using Dukebox.Library.Interfaces;
using Dukebox.Model.Services;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dukebox.Desktop.ViewModel
{
    public class RecentlyPlayedListingViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IMusicLibrary _musicLibrary;
        private readonly IAudioPlaylist _audioPlaylist;

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

                // run search on search text change
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
            private set
            {
                _tracks = value;

                _listSearchHelper.Items = _tracks;
                OnPropertyChanged("Tracks");
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

        public ICommand LoadTrack { get; private set; }

        public RecentlyPlayedListingViewModel(IMusicLibrary musicLibrary, IAudioPlaylist audioPlaylist) : base()
        {
            _musicLibrary = musicLibrary;
            _audioPlaylist = audioPlaylist;
            _tracks = new List<Track>();

            _listSearchHelper = new ListSearchHelper<Track>
            {
                Items = _tracks,
                FilterLambda = (t, s) => t.Song.title.ToLower().Contains(s.ToLower())
            };

            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadTrack = new RelayCommand<Track>(DoLoadTrack);

            _musicLibrary.RecentlyPlayedListModified += (o, e) => RefreshRecentlyPlayedFromLibrary();
            RefreshRecentlyPlayedFromLibrary();
        }

        private void DoLoadTrack(Track track)
        {
            _audioPlaylist.LoadPlaylistFromList(_tracks);
            _audioPlaylist.SkipToTrack(track);
        }

        public void RefreshRecentlyPlayedFromLibrary()
        {
            Tracks = _musicLibrary.RecentlyPlayedAsList;
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via Tracks property
            OnPropertyChanged("Tracks");
        }
    }
}
