using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Services;
using Dukebox.Library.Interfaces;
using System;
using Dukebox.Library.Model;
using Dukebox.Desktop.Helper;
using System.Globalization;

namespace Dukebox.Desktop.ViewModel
{
    public class TrackListingPreviewViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IAudioPlaylist _audioPlaylist;

        private List<ITrack> _tracks;
        private readonly IMusicLibraryRepository _musicLibraryRepo;
        private readonly IMusicLibraryUpdateService _musicLibraryUpdateService;
        private readonly IMusicLibraryEventService _eventService;
        private string _trackFilter;
        private string _trackFilterName;
        private readonly ListSearchHelper<ITrack> _listSearchHelper;
        private string _searchTerm;

        public ICommand ClearSearch { get; private set; }

        public bool EditingListingsDisabled
        {
            get
            {
                return true;
            }
        }

        public ICommand LoadTrack { get; private set; }

        public bool SearchEnabled
        {
            get
            {
                return true;
            }
        }

        public string SearchText
        {
            get
            {
                return _searchTerm;
            }
            set
            {
                _searchTerm = value;
                _listSearchHelper.SearchFilter = _searchTerm;

                OnPropertyChanged(nameof(Tracks));
            }
        }

        public Visibility ShowSearchControl
        {
            get
            {
                return Visibility.Visible;
            }
        }

        public List<TrackWrapper> Tracks
        {
            get
            {
                return _listSearchHelper.FilteredItems.Select(t => new TrackWrapper(_musicLibraryUpdateService, _eventService, t)).ToList();
            }
        }

        public TrackListingPreviewViewModel(IAudioPlaylist audioPlaylist, IMusicLibraryRepository libraryRepo, 
            IMusicLibraryUpdateService updateService, IMusicLibraryEventService eventService)
        {
            _audioPlaylist = audioPlaylist;
            _musicLibraryRepo = libraryRepo;
            _musicLibraryUpdateService = updateService;
            _eventService = eventService;

            _tracks = new List<ITrack>();

            _listSearchHelper = new ListSearchHelper<ITrack>
            {
                FilterLambda = (t, s) => t.ToString().ToLower(CultureInfo.InvariantCulture)
                    .Contains(s.ToLower(CultureInfo.InvariantCulture)),
                SortResults = true,
                SortLambda = (t) => t.Title.ToLower(CultureInfo.InvariantCulture),
                Items = _tracks
            };

            LoadTrack = new RelayCommand<ITrack>(DoLoadTrack);
            ClearSearch = new RelayCommand(DoClearSearch);

            _trackFilter = string.Empty;
            _trackFilterName = string.Empty;

            RegisterMessageHandlers();

            _eventService.SongAdded += (o, e) => ReloadTracksIfNeccessary(e);
            _eventService.SongDeleted += (o, e) => RemoveTrackIfNeccessary(e);
        }

        private void DoLoadTrack(ITrack track)
        {
            _audioPlaylist.LoadPlaylistFromList(_tracks, false);
            _audioPlaylist.SkipToTrack(track);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void DoClearSearch()
        {
            _listSearchHelper.SearchFilter = string.Empty;
            OnPropertyChanged(nameof(Tracks));
        }
        
        private void RegisterMessageHandlers()
        {
            Messenger.Default.Register<PreviewArtistOrAlbumMessage>(this, (nm) =>
            {
                if (nm.IsArtist)
                {
                    _trackFilter = "Artist";
                    UpdateTracks(_musicLibraryRepo.GetTracksForArtist(nm.Name));
                }
                else
                {
                    _trackFilter = "Album";
                    UpdateTracks(_musicLibraryRepo.GetTracksForAlbum(nm.Name));
                }

                _trackFilterName = nm.Name;
            });
        }

        private void UpdateTracks(List<ITrack> tracks)
        {
            _tracks = tracks;
            _listSearchHelper.Items = _tracks;

            OnPropertyChanged(nameof(Tracks));
        }

        private void ReloadTracksIfNeccessary(Song song)
        {
            var matchingTrack = Tracks.FirstOrDefault(t => t.Data.Song == song);
            var songHasMatchingFilter = _trackFilter.Equals("Artist") ? song.Artist.Name.Equals(_trackFilterName) : song.Album.Name.Equals(_trackFilterName);

            if (matchingTrack == null && !songHasMatchingFilter)
            {
                return;
            }

            if (songHasMatchingFilter)
            {
                if (_trackFilter == "Artist")
                {
                    UpdateTracks(_musicLibraryRepo.GetTracksForArtist(_trackFilterName));
                }
                else
                {
                    UpdateTracks(_musicLibraryRepo.GetTracksForAlbum(_trackFilterName));
                }
            }

            OnPropertyChanged("Tracks");
        }

        private void RemoveTrackIfNeccessary(Song song)
        {
            var matchingTrack = Tracks.FirstOrDefault(t => t.Data.Song == song);

            if (matchingTrack == null)
            {
                return;
            }

            Tracks.Remove(matchingTrack);
            OnPropertyChanged(nameof(Tracks));
        }
    }
}
