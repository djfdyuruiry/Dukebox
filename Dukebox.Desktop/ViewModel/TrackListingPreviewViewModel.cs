using Dukebox.Desktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dukebox.Library.Interfaces;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Model;
using GalaSoft.MvvmLight.Messaging;

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

        public List<ITrack> Tracks
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
                    Tracks = _musicLibrary.GetTracksForArtist(nm.Name);
                }
                else
                {
                    Tracks = _musicLibrary.GetTracksForAlbum(nm.Name);
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
