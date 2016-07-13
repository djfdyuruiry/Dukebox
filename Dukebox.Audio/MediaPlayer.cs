using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;
using Dukebox.Audio.Interfaces;
using Dukebox.Audio.Model;

namespace Dukebox.Audio
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaPlayer : IMediaPlayer
    {
        public const string MinuteFormat = "{0}:{1}";
        public const string startPlayingTrackEventName = "StartPlayingTrack";

        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly MediaPlayerMetadata defaultMetadata = new MediaPlayerMetadata { ArtistName = "Unknown Artist", AlbumName = "Unknown Album", TrackName = "Unknown Song" };

        private readonly IAudioCdService _audioCdService;

        /// <summary>
        /// 
        /// </summary>
        private int _stream;
        private Thread _playbackThread;

        private string _fileName;
        private double _newPosition;
        private TrackLoadedFromFileEventArgs _lastLoadedTrackEventArgs;
        private MediaPlayerMetadata _lastMediaPlayerMetadata;

        public bool AudioLoaded { get { return _stream != 0; } }

        public string AudioLengthInMins
        {
            get
            {
                if (!AudioLoaded)
                {
                    return string.Empty;
                }

                long position = Bass.BASS_ChannelGetLength(_stream);
                return GetMinutesFromChannelPosition(_stream, position);
            }
        }

        public string MinutesPlayed 
        {
            get 
            {
                if (!AudioLoaded)
                {
                    return string.Empty;
                }

                long position = Bass.BASS_ChannelGetPosition(_stream);
                return GetMinutesFromChannelPosition(_stream, position);
            } 
        }

        public double AudioLengthInSecs
        {
            get
            {
                if (!AudioLoaded)
                {
                    return 0;
                }

                long position = Bass.BASS_ChannelGetLength(_stream);
                return GetSecondsFromChannelPosition(_stream, position);
            }
        }

        public double SecondsPlayed
        {
            get
            {
                if (!AudioLoaded)
                {
                    return 0;
                }

                long position = Bass.BASS_ChannelGetPosition(_stream);
                return GetSecondsFromChannelPosition(_stream, position);
            }
        }

        public double PercentagePlayed
        {
            get
            {
                if (!AudioLoaded)
                {
                    return 0;
                }

                return SecondsPlayed / (AudioLengthInSecs / 100);
            }
        }

        public bool Playing { get; set; }
        public bool Stopped { get; set; }
        public bool Finished { get; set; }

        public Action<string, string> ErrorHandlingAction { get; set; }

        public event EventHandler StartPlayingTrack;
        public event EventHandler TrackPaused;
        public event EventHandler TrackResumed;
        public event EventHandler FinishedPlayingTrack;
        public event EventHandler<TrackLoadedFromFileEventArgs> LoadedTrackFromFile;
        public event EventHandler AudioPositionChanged;

        /// <summary>
        /// 
        /// </summary>
        public MediaPlayer(IAudioCdService audioCdService)
        {
            _audioCdService = audioCdService;
            _newPosition = -1;
        }

        public void LoadFile(string fileName)
        {
            LoadFile(fileName, null);
        }

        public void LoadFile(string fileName, MediaPlayerMetadata mediaPlayerMetadata)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name string specified has no value or is empty.");
            }
            else if (!File.Exists(fileName))
            {
                throw new ArgumentException(string.Format("File '{0}' does not exist.", fileName));
            }

            if (_stream != 0)
            {
                Bass.BASS_StreamFree(_stream);
            }

            _stream = 0;

            _fileName = fileName;

            if (_playbackThread != null)
            {
                _playbackThread.Abort();
            }

            _lastLoadedTrackEventArgs = new TrackLoadedFromFileEventArgs
            {
                FileName = fileName,
                Metadata = mediaPlayerMetadata ?? _lastMediaPlayerMetadata ?? defaultMetadata
            };

            if (mediaPlayerMetadata != null)
            {
                _lastMediaPlayerMetadata = mediaPlayerMetadata;
            }

            _playbackThread = new Thread(PlayAudioFile);
            _playbackThread.Start();

            logger.InfoFormat("{0} loaded for playback by the media player", fileName);
        }

        /// <summary>
        /// 
        /// </summary>
        public void PausePlayAudio()
        {
            if (!Stopped)
            {
                Playing = !Playing;

                if (!Playing)
                {
                    Task.Run(() => TrackPaused?.Invoke(this, EventArgs.Empty));
                }
            }
            else if (_fileName != string.Empty)
            {
                LoadFile(_fileName);

                Task.Run(() => StartPlayingTrack?.Invoke(this, EventArgs.Empty));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopAudio()
        {
            Stopped = true;

            if (_playbackThread != null)
            {
                Bass.BASS_ChannelStop(_stream);
                _playbackThread.Abort();

                Task.Run(() => FinishedPlayingTrack?.Invoke(this, EventArgs.Empty));
            }
        }

        public void ChangeAudioPosition(double newPositionInSeconds)
        {
            if (!Playing)
            {
                PausePlayAudio();
            }

            if (newPositionInSeconds >= 0 && newPositionInSeconds <=  AudioLengthInSecs)
            {
                _newPosition = newPositionInSeconds;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void PlayAudioFile()
        {
            if ((new FileInfo(_fileName)).Extension != ".cda")
            {
                _stream = Bass.BASS_StreamCreateFile(_fileName, 0L, 0L, BASSFlag.BASS_DEFAULT);
            }
            else
            {
                var driveIndex = _audioCdService.GetCdDriveIndex(_fileName[0]);
                var trackNumber = _audioCdService.GetTrackNumberFromCdaFilename(_fileName);

                _stream = BassCd.BASS_CD_StreamCreate(driveIndex, trackNumber, BASSFlag.BASS_DEFAULT);
            }

            if (_stream != 0)
            {
                if ((new FileInfo(_fileName)).Extension == ".cda")
                {
                    Thread.Sleep(1000);
                }

                // Play the channel.
                Bass.BASS_ChannelPlay(_stream, false);

                Finished = false;

                Playing = true;
                Stopped = false;

                Task.Run(() => StartPlayingTrack?.Invoke(this, EventArgs.Empty));
                Task.Run(() => LoadedTrackFromFile?.Invoke(this, _lastLoadedTrackEventArgs));
            }
            else // Error.
            {
                string msg = string.Format("Error playing file '{0}' [BASS error code: {1}]", _fileName.Split('\\').LastOrDefault(), Bass.BASS_ErrorGetCode().ToString());
                logger.Error(msg);

                Task.Run(() => ErrorHandlingAction?.Invoke(msg, "Dukebox: Error Playing File"));
                return;
            }

            // Maintain parent thread while audio is playing.
            while (Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_PLAYING
                   && !Stopped)
            {
                // Pause audio if flagged to do so.
                if (!Playing && !Stopped)
                {
                    Bass.BASS_ChannelPause(_stream);

                    Task.Run(() => TrackPaused?.Invoke(this, EventArgs.Empty));

                    // Wait while audio is paused.
                    while (!Playing && !Stopped)
                    {
                        Thread.Sleep(10);
                    }

                    // Resume playback if audio is not flagged to stop.
                    if (!Stopped)
                    {
                        Bass.BASS_ChannelPlay(_stream, false);

                        Task.Run(() => TrackResumed?.Invoke(this, EventArgs.Empty));
                    }
                }

                if ((int)_newPosition != -1)
                {
                    Bass.BASS_ChannelSetPosition(_stream, _newPosition);
                    _newPosition = -1;
                }

                Task.Run(() => AudioPositionChanged?.Invoke(this, EventArgs.Empty));
            }

            if (Stopped)
            {
                // Stop audio before freeing channel.
                Bass.BASS_ChannelStop(_stream);
            }
               
            Finished = true;

            // Free the stream channel.
            if (_stream != 0)
            {
                Bass.BASS_StreamFree(_stream);
                _stream = 0;
            }

            Playing = false;
            Stopped = false;

            Task.Run(() => FinishedPlayingTrack?.Invoke(this, EventArgs.Empty));
        }     

        /// <summary>
        /// Gets the total minutes from a channel at the given
        /// position. Maximum length supported by formatting is
        /// '999:59'. Uses the static property 'MINUTE_FORMAT' to
        /// preform formatting.
        /// </summary>
        /// <param name="stream">The channel handler.</param>
        /// <param name="channelPosition">The position in the channel to convert to mintues.</param>
        /// <returns>A formatted string containing the equivalent minutes and second that the position is in the channel.</returns>
        private string GetMinutesFromChannelPosition(int stream, long channelPosition)
        {
            double lengthInSecs = Bass.BASS_ChannelBytes2Seconds(stream, channelPosition);
            int minutes = (int)(lengthInSecs / 60f);
            int seconds = (int)(lengthInSecs % 60f);

            if (minutes > 99)
            {
                return string.Format(MinuteFormat, minutes.ToString("000"), seconds.ToString("00"));
            }

            return string.Format(MinuteFormat, minutes.ToString("00"), seconds.ToString("00"));
        }


        /// <summary>
        /// Gets the total seconds from a channel at the given position.
        /// </summary>
        /// <param name="stream">The channel handler.</param>
        /// <param name="channelPosition">The position in the channel to convert to seconds.</param>
        /// <returns>Number of seconds equivalent to the current position in the channel.</returns>
        private double GetSecondsFromChannelPosition(int stream, long channelPosition)
        {
            return Bass.BASS_ChannelBytes2Seconds(stream, channelPosition);
        }
    }
    
}
