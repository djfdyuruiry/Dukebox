using Dukebox.Library.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library.Interfaces
{
    public interface IAudioCdDriveMonitoringService
    {
        event EventHandler<AudioCdDriveEventArguments> AudioCdInsertedOnLoad;
        event EventHandler<AudioCdDriveEventArguments> AudioCdInserted;
        event EventHandler AudioCdEjected;
        void StartMonitoring();
    }
}
