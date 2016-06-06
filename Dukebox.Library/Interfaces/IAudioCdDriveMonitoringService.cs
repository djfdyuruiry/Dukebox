using System;
using System.Collections.Generic;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IAudioCdDriveMonitoringService
    {
        event EventHandler<AudioCdDriveEventArguments> AudioCdInsertedOnLoad;
        event EventHandler<AudioCdDriveEventArguments> AudioCdInserted;
        event EventHandler AudioCdEjected;
        void StartMonitoring();
        List<string> GetAudioCdDrivePaths();
        bool IsDriveReady(string audioDrivePath);
    }
}
