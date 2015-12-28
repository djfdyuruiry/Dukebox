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
    public class MediaPlayer
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly string MINUTE_FORMAT = "{0}:{1}";

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
        private MediaPlayer()
        {
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

            Logger.Info(fileName + " loaded for playback by the media player");
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
                _stream = BassCd.BASS_CD_StreamCreate(GetCdDriveIndex(_fileName[0]), GetTrackNumberFromCdaFilename(_fileName), BASSFlag.BASS_DEFAULT);
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
                Logger.Error(msg);

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

        #region Singleton pattern instance and accessor

        private static MediaPlayer instance;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static MediaPlayer GetInstance()
        {
            if (instance == null)
            {
                instance = new MediaPlayer();
            }

            return instance;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <returns></returns>
        public static int GetCdDriveIndex(char driveLetter)
        {
            driveLetter = Char.ToLower(driveLetter);

            DriveInfo[] drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom).ToArray();
            DriveInfo drive = null;

            int driveIndex = 0;

            for (; driveIndex < drives.Length; driveIndex++)
            {
                if (drives[driveIndex].Name.ToLower() == driveLetter + @":\")
                {
                    drive = drives[driveIndex];
                    break;
                }
            }

            // Drive does not exist or is not a CD drive.
            if (drive == null)
            {
                throw new Exception("Drive '" + driveLetter + "' does not exist or is not a CD drive!");
            }
            else if (!IsAudioCd(drive))
            {
                throw new Exception("Drive '" + driveLetter + "' does not contain an audio CD!");
            }

            return driveIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        private static bool IsAudioCd(DriveInfo drive)
        {
            bool result = false;
            string[] driveContents = Directory.GetFiles(drive.RootDirectory.FullName);

            result = (driveContents.Count() == driveContents.Where(f => (new FileInfo(f)).Extension == ".cda").Count());

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_fileName"></param>
        /// <returns></returns>
        public static int GetTrackNumberFromCdaFilename(string fileName)
        {
            return Int32.Parse(fileName.Split('\\').LastOrDefault().Substring(5, 2)) - 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_fileName"></param>
        /// <returns></returns>
        public static void ConvertCdaFileToMp3(string cdaFileName, string mp3OutFile, BaseEncoder.ENCODEFILEPROC progressCallback, bool overwriteOuputFile)
        {
            EncoderLAME lameEncoder = new EncoderLAME(0);
            lameEncoder.EncoderDirectory = Dukebox.Audio.Properties.Settings.Default.lameEncoderPath;

            lameEncoder.InputFile = @cdaFileName;
            lameEncoder.OutputFile = @mp3OutFile;

            (new Thread(() => 
            {
                // Extract cda file to wav file on disk.
                string wavFile = lameEncoder.OutputFile + ".wav";
                bool conversionResult = WriteCdaToWavFile(lameEncoder.InputFile, wavFile, progressCallback);

                if (!conversionResult && lameEncoder.EncoderExists)
                {
                    string msg = "Encoder result: " + conversionResult + "\nLast BASS error: " + Bass.BASS_ErrorGetCode().ToString();

                    Logger.Info(msg);
                    return;
                }

                // Call lame encoder with arguments.
                lameEncoder.InputFile = wavFile;
                CallLameEncoder(lameEncoder);

                // Clean up.
                File.Delete(wavFile);
            })).Start();
        }

        public static bool WriteCdaToWavFile(string inCdaFile, string outWavFile, BaseEncoder.ENCODEFILEPROC progressCallback)
        {
            int measuringStream = BassCd.BASS_CD_StreamCreateFile(inCdaFile, BASSFlag.BASS_DEFAULT);
            long totalLength = 0;

            if(measuringStream != 0)
            {
                totalLength = Bass.BASS_ChannelGetLength(measuringStream, BASSMode.BASS_POS_BYTES);
                Bass.BASS_StreamFree(measuringStream);

                int stream = BassCd.BASS_CD_StreamCreateFile(inCdaFile, BASSFlag.BASS_STREAM_DECODE);
                
                if (stream != 0)
                {
                    try
                    {
                        WaveWriter ww = new WaveWriter(outWavFile, stream, true);
                        short[] data = new short[32768];

                        long bytesSoFar = 0;

                        while (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
                        {
                            int length = Bass.BASS_ChannelGetData(stream, data, 32768);

                            if (length > 0)
                            {
                                ww.Write(data, length);

                                bytesSoFar += length;

                                progressCallback.Invoke(totalLength, bytesSoFar);
                            }
                        }

                        // finilize the wave file!
                        ww.Close();
                        return Bass.BASS_StreamFree(stream);
                    }
                    catch (Exception ex)
                    {
                        Logger.Info("Error copying cda file '" + inCdaFile + "' to wav file '" + outWavFile + "': " + ex.Message);
                    }
                }
            }

            return false;
        }

        public static void CallLameEncoder(EncoderLAME lameEncoder)
        {
            Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.CreateNoWindow = true;

            string lamePath = lameEncoder.EncoderCommandLine.Split(' ')[0];
            pProcess.StartInfo.FileName = lamePath;

            string commandArguments = lameEncoder.EncoderCommandLine.Replace(lamePath, string.Empty);
            pProcess.StartInfo.Arguments = commandArguments;

            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;

            pProcess.StartInfo.WorkingDirectory = Dukebox.Audio.Properties.Settings.Default.lameEncoderPath;
            pProcess.Start();

            //Get program output
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            Logger.Info("Output from lame encoding: " + strOutput);

            pProcess.WaitForExit();
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
        public static string GetMinutesFromChannelPosition(int stream, long channelPosition)
        {
            double lengthInSecs = Bass.BASS_ChannelBytes2Seconds(stream, channelPosition);
            int minutes = (int)(lengthInSecs / 60f);
            int seconds = (int)(lengthInSecs % 60f);

            if (minutes > 99)
            {
                return string.Format(MINUTE_FORMAT, minutes.ToString("000"), seconds.ToString("00"));
            }

            return string.Format(MINUTE_FORMAT, minutes.ToString("00"), seconds.ToString("00"));
        }


        /// <summary>
        /// Gets the total seconds from a channel at the given position.
        /// </summary>
        /// <param name="stream">The channel handler.</param>
        /// <param name="channelPosition">The position in the channel to convert to seconds.</param>
        /// <returns>Number of seconds equivalent to the current position in the channel.</returns>
        public static double GetSecondsFromChannelPosition(int stream, long channelPosition)
        {
            return Bass.BASS_ChannelBytes2Seconds(stream, channelPosition);
        }
    }
    
}
