using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Services;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Desktop.ViewModel
{
    public class PlaylistListingViewModel : ViewModelBase, IPlaylistListingViewModel, ISearchControlViewModel
    {
        private readonly IMusicLibraryEventService _eventService;
        private readonly IMusicLibraryRepository _musicRepo;
        private readonly IMusicLibraryCacheService _cacheService;
        private readonly IMusicLibraryUpdateService _updateService;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly ListSearchHelper<Playlist> _listSearchHelper;
        
        public ICommand LoadPlaylist { get; private set; }
        public ICommand RemovePlaylist { get; private set; }

        public List<PlaylistWrapper> Playlists
        {
            get
            {
                return _listSearchHelper
                    .FilteredItems
                    .Select(p => new PlaylistWrapper(_eventService, _musicRepo, _updateService, p))
                    .ToList();
            }
        }

        public ICommand ClearSearch { get; private set; }

        public string SearchText
        {
            get
            {
                return _listSearchHelper.SearchFilter;
            }
            set
            {
                _listSearchHelper.SearchFilter = value;
                OnPropertyChanged(nameof(Playlists));
            }
        }

        public bool SearchEnabled
        {
            get
            {
                return true;
            }
        }

        public PlaylistListingViewModel(IMusicLibraryEventService eventService, IMusicLibraryRepository musicRepo,
            IMusicLibraryCacheService cacheService, IMusicLibraryUpdateService updateService, IAudioPlaylist audioPlaylist)
        {
            _eventService = eventService;
            _musicRepo = musicRepo;
            _cacheService = cacheService;
            _updateService = updateService;
            _audioPlaylist = audioPlaylist;
            
            LoadPlaylist = new RelayCommand<PlaylistWrapper>(DoLoadPlaylist);
            RemovePlaylist = new RelayCommand<PlaylistWrapper>(DoRemovePlaylist);
            ClearSearch = new RelayCommand(DoClearSearch);

            _eventService.PlaylistsAdded += (o, e) => UpdatePlaylists();
            _eventService.PlaylistUpdated += (o, e) => UpdatePlaylists();

            _listSearchHelper = new ListSearchHelper<Playlist>
            {
                FilterLambda = (p, s) => p.Name.ToLower(CultureInfo.InvariantCulture)
                    .Contains(s.ToLower(CultureInfo.InvariantCulture)),
                SortResults = true,
                SortLambda = (p) => p.Name.ToLower(CultureInfo.InvariantCulture)
            };

            UpdatePlaylists();
        }

        private void DoLoadPlaylist(PlaylistWrapper playlistWrapper)
        {
            var tracks = _musicRepo.GetTracksForPlaylist(playlistWrapper.Name);
            _audioPlaylist.LoadPlaylistFromList(tracks);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void DoRemovePlaylist(PlaylistWrapper playlistWrapper)
        {
            _updateService.RemovePlaylist(playlistWrapper.Data).Wait();
            UpdatePlaylists();
        }

        private void UpdatePlaylists()
        {
            _listSearchHelper.Items = _cacheService.OrderedPlaylists.Select(p => p).ToList();
            OnPropertyChanged(nameof(Playlists));
        }

        private void DoClearSearch()
        {
            SearchText = string.Empty;
            OnPropertyChanged(nameof(Playlists));
        }
    }
}
