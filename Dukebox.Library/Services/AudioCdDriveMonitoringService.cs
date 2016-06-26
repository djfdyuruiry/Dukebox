using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using log4net;
using Dukebox.Audio;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services
{
    public class AudioCdDriveMonitoringService : IAudioCdDriveMonitoringService, IDisposable
    {
        private const double cdDrivePollingTimeInMs = 1000;
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ITrackGeneratorService _trackGenerator;
        private readonly AudioFileFormats _audioFileFormats;

        private readonly Timer _pollCdDrivesTimer;
        private readonly List<DriveInfo> _cdDrives;
        private DriveInfo _currentDrive;

        public event EventHandler<AudioCdDriveEventArgs> AudioCdInsertedOnLoad;
        public event EventHandler<AudioCdDriveEventArgs> AudioCdInserted;
        public event EventHandler AudioCdEjected;

        public AudioCdDriveMonitoringService(ITrackGeneratorService musicLibrary, AudioFileFormats audioFileFormats)
        {
            _trackGenerator = musicLibrary;
            _audioFileFormats = audioFileFormats;

            _cdDrives = GetAudioDrives();
            
            if (_cdDrives.Any())
            {
                _pollCdDrivesTimer = new Timer(cdDrivePollingTimeInMs);
                _pollCdDrivesTimer.Elapsed += (o, e) => PollCdDrives();
            }
        }

        public List<string> GetAudioCdDrivePaths()
        {
            return _cdDrives.Select(di => di.RootDirectory.FullName).ToList();
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

            var firstReadyCdDrive = _cdDrives.FirstOrDefault(d => d.IsReady && d != _currentDrive);

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
            AudioCdEjected?.Invoke(this, EventArgs.Empty);

            _currentDrive = null;
        }

        private void DoAudioCdInserted(bool onLoad)
        {
            var tracks = GetTracksForCd();

            if (!tracks.Any())
            {
                return;
            }

            var args = new AudioCdDriveEventArgs
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
                return _trackGenerator.GetTracksForDirectory(_currentDrive.RootDirectory.FullName, false);
            }
            catch (Exception ex)
            {
                var errMsg = "Unable to get tracks from cd drive";
                logger.Error(errMsg, ex);

                return new List<ITrack>();
            }
        }

        public bool IsDriveReady(string audioDrivePath)
        {
            if (string.IsNullOrEmpty(audioDrivePath))
            {
                return false;
            }

            var drive = _cdDrives.FirstOrDefault(di => string.Equals(di.RootDirectory.FullName, audioDrivePath, StringComparison.OrdinalIgnoreCase));

            if (drive == null)
            {
                return false;
            }

            return drive.IsReady;
        }

        protected virtual void Dispose(bool cleanAllResources)
        {
            if (cleanAllResources)
            {
                _pollCdDrivesTimer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
