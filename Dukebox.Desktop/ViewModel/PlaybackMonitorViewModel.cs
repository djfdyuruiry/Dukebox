using Dukebox.Desktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dukebox.Desktop.ViewModel
{
    public class PlaybackMonitorViewModel : ViewModelBase, IPlaybackMonitorViewModel
    {
        private const string defaultAlbumArtUri = @"pack://application:,,,/Resources/black_7_music_node.png";
        private const string pauseImageUri = @"pack://application:,,,/Resources/black_4_audio_play.png";
        private const string playImageUri = @"pack://application:,,,/Resources//black_4_audio_pause.png";

        private string _artist;
        private string _track;
        private string _album;
        private string _trackMinutesPassed;
        private string _trackMinutesTotal;
        private ImageSource _defaultAlbumArt;
        private ImageSource _albumArt;
        private ImageSource _playImage;
        private ImageSource _pauseImage;
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

        public PlaybackMonitorViewModel() : base()
        {
            BuildDefaultAlbumArtImage();
            BuildPlayPauseImages();

            AlbumArt = _defaultAlbumArt;
            PlayPauseImage = _playImage;
        }

        private void BuildDefaultAlbumArtImage()
        {
            var defaultAlbumArtImage = new BitmapImage();
            defaultAlbumArtImage.BeginInit();
            defaultAlbumArtImage.BaseUri = new Uri(defaultAlbumArtUri, UriKind.RelativeOrAbsolute);
            defaultAlbumArtImage.EndInit();

            _defaultAlbumArt = defaultAlbumArtImage;
        }

        private void BuildPlayPauseImages()
        {
            var pauseImage = new BitmapImage();
            pauseImage.BeginInit();
            pauseImage.BaseUri = new Uri(pauseImageUri, UriKind.RelativeOrAbsolute);
            pauseImage.EndInit();

            _pauseImage = pauseImage;
            
            var playImage = new BitmapImage();
            playImage.BeginInit();
            playImage.BaseUri = new Uri(playImageUri, UriKind.RelativeOrAbsolute);
            playImage.EndInit();

            _playImage = playImage;
        }
    }
}
