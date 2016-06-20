using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library.Interfaces;
using Dukebox.Desktop.Services;

namespace Dukebox.Desktop.ViewModel
{
    public class RecentlyPlayedListingViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IMusicLibrary _musicLibrary;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly ListSearchHelper<ITrack> _listSearchHelper;
        
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
        public List<TrackWrapper> Tracks
        {
            get
            {
                return _listSearchHelper.FilteredItems.Select(t => new TrackWrapper(_musicLibrary, t)).ToList();
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

        public Visibility ShowSearchControl
        {
            get
            {
                return Visibility.Visible;
            }
        }

        public ICommand LoadTrack { get; private set; }

        public RecentlyPlayedListingViewModel(IMusicLibrary musicLibrary, IAudioPlaylist audioPlaylist) : base()
        {
            _musicLibrary = musicLibrary;
            _audioPlaylist = audioPlaylist;

            _listSearchHelper = new ListSearchHelper<ITrack>
            {
                FilterLambda = (t, s) => t.ToString().ToLower(CultureInfo.InvariantCulture)
                .Contains(s.ToLower(CultureInfo.InvariantCulture))
            };

            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadTrack = new RelayCommand<ITrack>(DoLoadTrack);

            _musicLibrary.RecentlyPlayedListModified += (o, e) => RefreshRecentlyPlayedFromLibrary();
            RefreshRecentlyPlayedFromLibrary();
        }

        private void DoLoadTrack(ITrack track)
        {
            _audioPlaylist.LoadPlaylistFromList(_listSearchHelper.FilteredItems);
            _audioPlaylist.SkipToTrack(track);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        public void RefreshRecentlyPlayedFromLibrary()
        {
            _listSearchHelper.Items = _musicLibrary.RecentlyPlayedAsList;
            OnPropertyChanged("Tracks");
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via Tracks property
            OnPropertyChanged("Tracks");
        }
    }
}
