using System.Collections.Generic;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IWatchFolderManagerService
    {
        List<IWatchFolderService> WatchFolders { get; }
        WatchFolder LastWatchFolderUpdated { get; }
        void ManageWatchFolder(WatchFolder watchFolder);
        void StopManagingWatchFolder(WatchFolder watchFolder);
    }
}
