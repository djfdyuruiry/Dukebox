using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Model.Services;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dukebox.Desktop.ViewModel
{
    public class LibraryListingViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IMusicLibrary _musicLibrary;
        private readonly IAudioPlaylist _audioPlaylist;

        private List<Track> _tracks;
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
                return _tracks;
            }
            private set
            {
                _tracks = value;
                OnPropertyChanged("Tracks");
            }
        }
        public bool EditingListingsDisabled
        {
            get
            { 
                return false; 
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


        public LibraryListingViewModel(IMusicLibrary musicLibrary, IAudioPlaylist audioPlaylist) : base()
        {
            _musicLibrary = musicLibrary;
            _musicLibrary.SongAdded += (o, e) => RefreshTrackListing();

            _audioPlaylist = audioPlaylist;

            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadTrack = new RelayCommand<Track>(DoLoadTrack);

            RefreshTrackListing();
        }

        private void DoLoadTrack(Track track)
        {
            _audioPlaylist.LoadPlaylistFromList(_tracks);
            _audioPlaylist.SkipToTrack(track);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void RefreshTrackListing()
        {
            SearchText = string.Empty;
            DoSearch();
        }

        private void DoSearch()
        {
            Tracks = _musicLibrary.SearchForTracksInArea(SearchAreas.All, SearchText);
        }
    }
}
