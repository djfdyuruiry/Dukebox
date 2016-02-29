using Dukebox.Audio.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;
using Un4seen.Bass.Misc;

namespace Dukebox.Audio
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaPlayer : IMediaPlayer
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string MinuteFormat = "{0}:{1}";

        private IAudioCdService _audioCdService;

        #region Playback control properties
        
        /// <summary>
        /// 
        /// </summary>
        private int _stream;
        private Thread _playbackThread;

        private string _fileName;
        private double _newPosition;

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

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public MediaPlayer(IAudioCdService audioCdService)
        {
            _audioCdService = audioCdService;
            _newPosition = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name string specified has no value or is empty.");
            }
            else if(!File.Exists(fileName))
            {
                throw new ArgumentException(string.Format("File '{0}' does not exist.", fileName));
            }
            else if(File.ReadAllBytes(fileName).Count() == 0)
            {
                throw new ArgumentException(string.Format("File '{0}' contains no data!", fileName));
            }

            Bass.BASS_StreamFree(_stream);
            _stream = 0;

            _fileName = fileName;

            if (_playbackThread != null)
            {
                _playbackThread.Abort();
            }

            _playbackThread = new Thread(PlayAudioFile);
            _playbackThread.Start();

            logger.Info(fileName + " loaded for playback by the media player");
        }

        /// <summary>
        /// 
        /// </summary>
        public void PausePlayAudio()
        {
            if (!Stopped)
            {
                Playing = !Playing;
            }
            else if (_fileName != string.Empty)
            {
                LoadFile(_fileName);
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
            }
        }

        public void ChangeAudioPosition(double newPosition)
        {
            if (Playing && newPosition >= 0 && newPosition <=  AudioLengthInSecs)
            {
                _newPosition = newPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void PlayAudioFile()
        {
            // Create a stream channel from a file.
            
            if((new FileInfo(_fileName)).Extension != ".cda")
            {
                _stream = Bass.BASS_StreamCreateFile(_fileName, 0L, 0L, BASSFlag.BASS_DEFAULT);
            }
            else
            {
                _stream = BassCd.BASS_CD_StreamCreate(_audioCdService.GetCdDriveIndex(_fileName[0]), _audioCdService.GetTrackNumberFromCdaFilename(_fileName), BASSFlag.BASS_DEFAULT);
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
            }
            else // Error.
            {
                string msg = "Error playing file '" + _fileName.Split('\\').LastOrDefault() + "'!" + " [BASS error code: " + Bass.BASS_ErrorGetCode().ToString() + "]";
                logger.Error(msg);

                MessageBox.Show(msg, "Dukebox: Error Playing File", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    // Wait while audio is paused.
                    while (!Playing && !Stopped)
                    {
                        Thread.Sleep(10);
                    }

                    // Resume playback if audio is not flagged to stop.
                    if (!Stopped)
                    {
                        Bass.BASS_ChannelPlay(_stream, false);
                    }
                }

                if (_newPosition != -1)
                {
                    Bass.BASS_ChannelSetPosition(_stream, _newPosition);
                    _newPosition = -1;
                }

                Thread.Sleep(100);
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
        public string GetMinutesFromChannelPosition(int stream, long channelPosition)
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
        public double GetSecondsFromChannelPosition(int stream, long channelPosition)
        {
            return Bass.BASS_ChannelBytes2Seconds(stream, channelPosition);
        }
    }
    
}
