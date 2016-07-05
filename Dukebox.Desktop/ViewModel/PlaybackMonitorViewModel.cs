using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;
using MediaPlayer = Dukebox.Audio.MediaPlayer;
using Dukebox.Audio.Interfaces;
using Dukebox.Audio.Model;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Desktop.Services;

namespace Dukebox.Desktop.ViewModel
{
    public class PlaybackMonitorViewModel : ViewModelBase, IPlaybackMonitorViewModel
    {
        private const string retryRegisterHotKeysDialogText = "Error registering global multimedia hot keys! Do you want to retry (This usually happens when another media player or web browser is open)";
        private const string retryRegisterHotKeysDialogTitle = "Dukebox - Error Registering Hot Keys";
        private const string emptyTrackTimeText = "00";

        private readonly IMediaPlayer _mediaPlayer;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly IGlobalMultimediaHotKeyService _globalHotKeyService;
        private readonly IMusicLibraryEventService _eventService;
        private readonly IAlbumArtCacheService _albumArtCache;

        private string _artist;
        private string _track;
        private string _album;
        private string _trackMinutesPassed;
        private string _trackMinutesTotal;
        private ImageSource _playPauseImage;
        private string _albumArtUri;
        private ImageSource _albumArtImage;
        private string _currentlyLoadedFile;

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
                return _albumArtImage;
            }
            private set
            {
                _albumArtImage = value;

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

                OnPropertyChanged("PlayPauseImage");
            }
        }
        public ICommand PlayPauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand ForwardCommand { get; private set; }

        public PlaybackMonitorViewModel(IMediaPlayer mediaPlayer, IAudioPlaylist audioPlaylist,
            IGlobalMultimediaHotKeyService globalHotKeyService, IAlbumArtCacheService albumArtCache,
            IMusicLibraryEventService eventService) : base()
        {
            _mediaPlayer = mediaPlayer;
            _audioPlaylist = audioPlaylist;
            _globalHotKeyService = globalHotKeyService;
            _albumArtCache = albumArtCache;
            _eventService = eventService;

            UpdateAlbumArt(ImageResources.DefaultAlbumArtUri);
            PlayPauseImage = ImageResources.PlayImage;

            SetupAudioEventListeners();
            SetupPlaybackControlCommands();

            RegisterHotKeys();
        }

        private void SetupAudioEventListeners()
        {
            _mediaPlayer.LoadedTrackFromFile += (o, e) => LoadedTrackFromFile(e);
            _audioPlaylist.NewTrackLoaded += (o, e) => LoadNewTrackArtIfNeccessary(e);

            _mediaPlayer.FinishedPlayingTrack += (o, e) => TrackFinishedPlaying();
            _mediaPlayer.AudioPositionChanged += (o, e) => TrackPositionChanged();

            // pause/play image toggles
            _mediaPlayer.StartPlayingTrack += (o, e) => PlayPauseImage = ImageResources.PauseImage;
            _mediaPlayer.TrackPaused += (o, e) => PlayPauseImage = ImageResources.PlayImage;
            _mediaPlayer.TrackResumed += (o, e) => PlayPauseImage = ImageResources.PauseImage;
            _mediaPlayer.ErrorHandlingAction = (errMsg, title) => MessageBox.Show(errMsg, title, MessageBoxButton.OK, MessageBoxImage.Error);

            _eventService.SongUpdated += (o, e) =>
            {
                if (e.FileName.Equals(_currentlyLoadedFile))
                {
                    UpdateTrackDisplay(e);
                }
            };
        }

        private void SetupPlaybackControlCommands()
        {
            PlayPauseCommand = new RelayCommand(_audioPlaylist.PausePlay);
            StopCommand = new RelayCommand(_audioPlaylist.Stop);
            BackCommand = new RelayCommand(_audioPlaylist.Back);
            ForwardCommand = new RelayCommand(_audioPlaylist.Forward);
        }

        private void RegisterHotKeys()
        {
            var hotKeysRegistered = _globalHotKeyService.RegisterMultimediaHotKeys(true, RetryRegisterHotKeys);

            if (!hotKeysRegistered)
            {
                return;
            }

            _globalHotKeyService.PlayPausePressed += (o, e) => _audioPlaylist.PausePlay();
            _globalHotKeyService.StopPressed += (o, e) => _audioPlaylist.Stop();
            _globalHotKeyService.PreviousTrackPressed += (o, e) => _audioPlaylist.Back();
            _globalHotKeyService.NextTrackPressed += (o, e) => _audioPlaylist.Forward();
        }

        private bool RetryRegisterHotKeys()
        {
            var msgBoxResult = MessageBox.Show(retryRegisterHotKeysDialogText, retryRegisterHotKeysDialogTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            var shouldRetry = msgBoxResult == MessageBoxResult.Yes;

            return shouldRetry;
        }

        private void LoadedTrackFromFile(TrackLoadedFromFileEventArgs trackLoadedArgs)
        {
            TrackMinutesTotal = _mediaPlayer.AudioLengthInMins;

            Artist = trackLoadedArgs.Metadata.ArtistName;
            Track = trackLoadedArgs.Metadata.TrackName;
            Album = trackLoadedArgs.Metadata.AlbumName;

            _currentlyLoadedFile = trackLoadedArgs.FileName;
        }

        private void UpdateTrackDisplay(Song song)
        {
            LoadedTrackFromFile(new TrackLoadedFromFileEventArgs
            {
                FileName = song.FileName,
                Metadata = new MediaPlayerMetadata
                {
                    TrackName = song.Title,
                    ArtistName = song.ArtistName,
                    AlbumName = song.AlbumName
                }
            });
        }

        private void LoadNewTrackArtIfNeccessary(NewTrackLoadedEventArgs newTrackArgs)
        {
            var albumId = newTrackArgs.Track.Album.Id;

            try
            {
                if (_albumArtCache.CheckCacheForAlbum(albumId))
                {
                    var albumArtCachedImagePath = _albumArtCache.GetAlbumArtPathFromCache(albumId);
                    UpdateAlbumArt(albumArtCachedImagePath);

                    return;
                }

                if (!newTrackArgs.Track.Metadata.HasAlbumArt)
                {
                    UpdateAlbumArt(ImageResources.DefaultAlbumArtUri);
                    return;
                }

                var albumArtTempImagePath = newTrackArgs.Track.Metadata.SaveAlbumArtToTempFile();
                UpdateAlbumArt(albumArtTempImagePath);
            }
            catch
            {
                UpdateAlbumArt(ImageResources.DefaultAlbumArtUri);
            }            
        }

        private void UpdateAlbumArt(string albumArtUri)
        {
            if(string.IsNullOrEmpty(albumArtUri))
            {
                throw new Exception("Unable to update album art for playback monitor: album art uri string was null or empty");
            }

            if (_albumArtUri == albumArtUri)
            {
                // if currently displaying requested image
                return;
            }
            
            ImageSource albumArtImage = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                albumArtImage = ImageResourceLoader.LoadImageFromResourceUri(albumArtUri);
            });

            AlbumArt = albumArtImage;
            _albumArtUri = albumArtUri;
        }

        private void TrackFinishedPlaying()
        {
            PlayPauseImage = ImageResources.PlayImage;
            TrackMinutesPassed = string.Format(MediaPlayer.MinuteFormat, emptyTrackTimeText, emptyTrackTimeText);
        }

        private void TrackPositionChanged()
        {
            TrackMinutesPassed = _mediaPlayer.MinutesPlayed;
        }
    }
}
