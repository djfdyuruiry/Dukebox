using System.Collections.Generic;
using System.Windows.Input;
using Dukebox.Desktop.Services;

namespace Dukebox.Desktop.Interfaces
{
    public interface IWatchFolderSettingsViewModel
    {
        List<WatchFolderWrapper> WatchFolders { get; }
        ICommand AddWatchFolder { get; }
        ICommand DeleteWatchFolder { get; }
    }
}
