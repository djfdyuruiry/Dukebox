using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Services;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Desktop.Helper;

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
                return _listSearchHelper.SearchFilter;
            }
            set
            {
                _listSearchHelper.SearchFilter = value;

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
            SearchText = string.Empty;
        }

        private void RegisterMessageHandlers()
        {
            Messenger.Default.Register<PreviewTracksMessage>(this, (nm) =>
            {
                if (nm.IsArtist)
                {
                    _trackFilter = "Artist";
                    UpdateTracks(() => _musicLibraryRepo.GetTracksForArtist(nm.Name));
                }
                else if (nm.IsAlbum)
                {
                    _trackFilter = "Album";
                    UpdateTracks(() => _musicLibraryRepo.GetTracksForAlbum(nm.Name));
                }
                else
                {
                    UpdateTracks(() => _musicLibraryRepo.GetTracksForPlaylist(nm.Name));
                }

                _trackFilterName = nm.Name;
            });
        }

        private void UpdateTracks(Func<List<ITrack>> tracksGenerator)
        {
            try
            {
                _tracks = tracksGenerator();
            }
            catch (Exception)
            {
                _tracks = new List<ITrack>();
            }
            finally
            {
                _listSearchHelper.Items = _tracks;
                OnPropertyChanged(nameof(Tracks));
            }
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
                    UpdateTracks(() => _musicLibraryRepo.GetTracksForArtist(_trackFilterName));
                }
                else
                {
                    UpdateTracks(() => _musicLibraryRepo.GetTracksForAlbum(_trackFilterName));
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
