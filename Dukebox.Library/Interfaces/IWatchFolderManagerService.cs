using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IWatchFolderManagerService
    {
        event EventHandler WatchFolderServiceProcessedEvent;
        List<IWatchFolderService> WatchFolders { get; }
        WatchFolder LastWatchFolderUpdated { get; }
        Task ManageWatchFolder(WatchFolder watchFolder);
        Task StopManagingWatchFolder(WatchFolder watchFolder);
    }
}
