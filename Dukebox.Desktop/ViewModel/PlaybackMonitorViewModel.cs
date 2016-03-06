using MediaPlayer = Dukebox.Audio.MediaPlayer;
using Dukebox.Audio.Interfaces;
using Dukebox.Audio.Model;
using Dukebox.Desktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Desktop.Helper;
using System.Windows;

namespace Dukebox.Desktop.ViewModel
{
    public class PlaybackMonitorViewModel : ViewModelBase, IPlaybackMonitorViewModel
    {     
        private readonly IMediaPlayer _mediaPlayer;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly ImageToImageSourceConverter _imageToImageSourceConverter;

        private string _artist;
        private string _track;
        private string _album;
        private string _trackMinutesPassed;
        private string _trackMinutesTotal;
        private ImageSource _albumArt;
        private ImageSource _playPauseImage;

        public string Artist
        {
            get
            {
                return _artist;
            }
            private set
            {
                _artist = value;

                OnPropertyChanged("Artist");
            }
        }
        public string Track
        {
            get
            {
                return _track;
            }
            private set
            {
                _track = value;

                OnPropertyChanged("Track");
            }
        }
        public string Album
        {
            get
            {
                return _album;
            }
            private set
            {
                _album = value;

                OnPropertyChanged("Album");
            }
        }
        public string TrackMinutesPassed
        {
            get
            {
                return _trackMinutesPassed;
            }
            private set
            {
                _trackMinutesPassed = value;

                OnPropertyChanged("TrackMinutesPassed");
            }
        }
        public string TrackMinutesTotal
        {
            get
            {
                return _trackMinutesTotal;
            }
            private set
            {
                _trackMinutesTotal = value;

                OnPropertyChanged("TrackMinutesTotal");
            }
        }
        public ImageSource AlbumArt
        {
            get
            {
                return _albumArt;
            }
            private set
            {
                _albumArt = value;

                OnPropertyChanged("AlbumArt");
            }
        }
        public ImageSource PlayPauseImage
        {
            get
            {
                return _playPauseImage;
            }
            private set
            {
                _playPauseImage = value;

                OnPropertyChanged("ImageSource");
            }
        }
        public ICommand PlayPauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand ForwardCommand { get; private set; }

        public PlaybackMonitorViewModel(IMediaPlayer mediaPlayer, IAudioPlaylist audioPlaylist, 
            ImageToImageSourceConverter imageToImageSourceConverter) : base()
        {
            _mediaPlayer = mediaPlayer;
            _audioPlaylist = audioPlaylist;
            _imageToImageSourceConverter = imageToImageSourceConverter;

            AlbumArt = ImageResources.DefaultAlbumArtImage;
            PlayPauseImage = ImageResources.PlayImage;

            SetupAudioEventListeners();
            SetupPlaybackControlCommands();
        }
        
        private void SetupAudioEventListeners()
        {
            _mediaPlayer.LoadedTrackFromFile += (o, e) => LoadedTrackFromFile(e);
            _audioPlaylist.NewTrackLoaded += (o, e) => LoadNewTrackAlbumArtIfPresent(e);

            _mediaPlayer.FinishedPlayingTrack += (o, e) => TrackFinishedPlaying();
            _mediaPlayer.AudioPositionChanged += (o, e) => TrackPositionChanged();

            // pause\play image toggles
            _mediaPlayer.StartPlayingTrack += (o, e) => PlayPauseImage = ImageResources.PauseImage;
            _mediaPlayer.TrackPaused += (o, e) => PlayPauseImage = ImageResources.PlayImage;
            _mediaPlayer.TrackResumed += (o, e) => PlayPauseImage = ImageResources.PauseImage;
        }

        private void SetupPlaybackControlCommands()
        {
            PlayPauseCommand = new RelayCommand(_audioPlaylist.PausePlay);
            StopCommand = new RelayCommand(_audioPlaylist.Stop);
            BackCommand = new RelayCommand(_audioPlaylist.Back);
            ForwardCommand = new RelayCommand(_audioPlaylist.Forward);
        }

        private void LoadedTrackFromFile(TrackLoadedFromFileEventArgs trackLoadedArgs)
        {
            TrackMinutesTotal = _mediaPlayer.AudioLengthInMins;
            AlbumArt = ImageResources.DefaultAlbumArtImage;

            if (trackLoadedArgs.Metadata == null)
            {
                Album = Artist = Track = string.Empty;
            }

            Artist = trackLoadedArgs.Metadata.ArtistName;
            Track = trackLoadedArgs.Metadata.TrackName;
            Album = trackLoadedArgs.Metadata.AlbumName;
        }

        private void LoadNewTrackAlbumArtIfPresent(NewTrackLoadedEventArgs newTrackArgs)
        {
            if (!newTrackArgs.Track.Metadata.HasAlbumArt)
            {
                AlbumArt = ImageResources.DefaultAlbumArtImage;
                return;
            }

            AlbumArt = _imageToImageSourceConverter.Convert(newTrackArgs.Track.Metadata.AlbumArt);
        }

        private void TrackFinishedPlaying()
        {
            PlayPauseImage = ImageResources.PlayImage;
            TrackMinutesPassed = string.Format(MediaPlayer.MinuteFormat, "00", "00");
        }

        private void TrackPositionChanged()
        {
            TrackMinutesPassed = _mediaPlayer.MinutesPlayed;
        }

        private void AlbumArtChanged(ImageSource albumArt)
        {
            AlbumArt = albumArt;
        }
    }
}
