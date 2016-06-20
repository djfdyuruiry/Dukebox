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

namespace Dukebox.Desktop.ViewModel
{
    public class TrackListingPreviewViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IAudioPlaylist _audioPlaylist;

        private List<ITrack> _tracks;
        private readonly IMusicLibrary _musicLibrary;

        public ICommand ClearSearch
        {
            get
            {
                return null;
            }
        }

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
                return false;
            }
        }

        public string SearchText { get; set; }

        public Visibility ShowSearchControl
        {
            get
            {
                return Visibility.Hidden;
            }
        }

        public List<TrackWrapper> Tracks
        {
            get
            {
                return _tracks.Select(t => new TrackWrapper(_musicLibrary, t)).ToList();
            }
        }

        private void UpdateTracks(List<ITrack> tracks)
        {
            _tracks = tracks;
            OnPropertyChanged("Tracks");
        }

        public TrackListingPreviewViewModel(IAudioPlaylist audioPlaylist, IMusicLibrary musicLibrary)
        {
            _audioPlaylist = audioPlaylist;
            _musicLibrary = musicLibrary;

            _tracks = new List<ITrack>();

            LoadTrack = new RelayCommand<ITrack>(DoLoadTrack);

            RegisterMessageHandlers();
        }

        private void RegisterMessageHandlers()
        {
            Messenger.Default.Register<PreviewArtistOrAlbumMessage>(this, (nm) =>
            {
                if (nm.IsArtist)
                {
                    UpdateTracks(_musicLibrary.GetTracksForArtist(nm.Name));
                }
                else
                {
                    UpdateTracks(_musicLibrary.GetTracksForAlbum(nm.Name));
                }
            });
        }

        private void DoLoadTrack(ITrack track)
        {
            _audioPlaylist.LoadPlaylistFromList(_tracks, false);
            _audioPlaylist.SkipToTrack(track);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }
    }
}
