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
using AlphaChiTech.Virtualization;

namespace Dukebox.Desktop.ViewModel
{
    public class RecentlyPlayedListingViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IMusicLibraryUpdateService _musicLibraryUpdateService;
        private readonly IRecentlyPlayedRepository _recentlyPlayedRepo;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly IMusicLibraryEventService _eventService;
        private readonly ListSearchHelper<string> _listSearchHelper;
        
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
        public VirtualizingObservableCollection<ITrack> Tracks
        {
            get
            {
                return null; // _listSearchHelper.FilteredItems;
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

        public RecentlyPlayedListingViewModel(IMusicLibraryUpdateService musicLibraryUpdateService, IRecentlyPlayedRepository recentlyPlayedRepo, 
            IAudioPlaylist audioPlaylist, IMusicLibraryEventService eventService) : base()
        {
            _musicLibraryUpdateService = musicLibraryUpdateService;
            _recentlyPlayedRepo = recentlyPlayedRepo;
            _audioPlaylist = audioPlaylist;
            _eventService = eventService;

            _listSearchHelper = new ListSearchHelper<string>
            {
                FilterLambda = (t, s) => t.ToLower(CultureInfo.InvariantCulture)
                .Contains(s.ToLower(CultureInfo.InvariantCulture))
            };

            ClearSearch = new RelayCommand(() => SearchText = string.Empty);
            LoadTrack = new RelayCommand<ITrack>(DoLoadTrack);

            _recentlyPlayedRepo.RecentlyPlayedListModified += (o, e) => RefreshRecentlyPlayedFromLibrary();
            RefreshRecentlyPlayedFromLibrary();
        }

        private void DoLoadTrack(ITrack track)
        {
            _audioPlaylist.LoadPlaylistFromList(_listSearchHelper.FilteredItems);
            _audioPlaylist.SkipToTrack(track.Song.FileName);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        public void RefreshRecentlyPlayedFromLibrary()
        {
            _listSearchHelper.Items = _recentlyPlayedRepo.RecentlyPlayedAsList;
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
