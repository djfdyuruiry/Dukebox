using Dukebox.Library.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dukebox.Library.Model;
using System.IO;
using System.Timers;
using Dukebox.Audio;
using Dukebox.Library.Services;
using log4net;
using System.Reflection;

namespace Dukebox.Library.Services
{
    public class AudioCdDriveMonitoringService : IAudioCdDriveMonitoringService
    {
        private const double cdDrivePollingTimeInMs = 1000;
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMusicLibrary _musicLibrary;
        private readonly AudioFileFormats _audioFileFormats;

        private readonly Timer _pollCdDrivesTimer;
        private readonly List<DriveInfo> _cdDrives;
        private DriveInfo _currentDrive;

        public event EventHandler<AudioCdDriveEventArguments> AudioCdInsertedOnLoad;
        public event EventHandler<AudioCdDriveEventArguments> AudioCdInserted;
        public event EventHandler AudioCdEjected;

        public AudioCdDriveMonitoringService(IMusicLibrary musicLibrary, AudioFileFormats audioFileFormats)
        {
            _musicLibrary = musicLibrary;
            _audioFileFormats = audioFileFormats;

            _cdDrives = GetAudioDrives();
            
            if (_cdDrives.Any())
            {
                _pollCdDrivesTimer = new Timer(cdDrivePollingTimeInMs);
                _pollCdDrivesTimer.Elapsed += (o, e) => PollCdDrives();
            }
        }

        private List<DriveInfo> GetAudioDrives()
        {
            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom);
                return drives.ToList();
            }
            catch (Exception ex)
            {
                var errMsg = "Unable to query the system for audio drives";
                logger.Error(errMsg, ex);

                return new List<DriveInfo>();
            }
        }

        public void StartMonitoring()
        {
            if(!_cdDrives.Any())
            {
                return;
            }

            // ensure we dont load audio cd's until it audio file formats are loaded (to see if .cda is supported)
            _audioFileFormats.FormatsLoaded += (o, e) =>
            {
                PollCdDrives(true);
                _pollCdDrivesTimer.Start();
            };
        }

        private void PollCdDrives(bool initalLoad = false)
        {
            if (_currentDrive != null && !_currentDrive.IsReady)
            {
                DoAudioCdEjected();
            }

            var firstReadyCdDrive = _cdDrives.Where(d => d.IsReady && d != _currentDrive).FirstOrDefault();

            if (firstReadyCdDrive == null)
            {
                return;
            }

            _currentDrive = firstReadyCdDrive;

            if (AudioCdInserted != null)
            {
                DoAudioCdInserted(initalLoad);
            }
        }

        private void DoAudioCdEjected()
        {
            if (AudioCdEjected != null)
            {
                AudioCdEjected(this, EventArgs.Empty);
            }

            _currentDrive = null;
        }

        private void DoAudioCdInserted(bool onLoad)
        {
            var tracks = GetTracksForCd();

            if (!tracks.Any())
            {
                return;
            }

            var args = new AudioCdDriveEventArguments
            {
                DriveDirectory = _currentDrive.RootDirectory.FullName,
                CdTracks = tracks
            };

            if (onLoad)
            {
                AudioCdInsertedOnLoad(this, args);
            }
            else
            {
                AudioCdInserted(this, args);
            }
        }

        private List<ITrack> GetTracksForCd()
        {
            try
            {
                return _musicLibrary.GetTracksForDirectory(_currentDrive.RootDirectory.FullName, false);
            }
            catch (Exception ex)
            {
                var errMsg = "Unable to get tracks from cd drive";
                logger.Error(errMsg, ex);

                return new List<ITrack>();
            }
        }
    }
}
