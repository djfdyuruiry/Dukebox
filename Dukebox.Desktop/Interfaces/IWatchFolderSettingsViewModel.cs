using System.Collections.Generic;
using System.Windows.Input;
using Dukebox.Library.Model;

namespace Dukebox.Desktop.Interfaces
{
    public interface IWatchFolderSettingsViewModel
    {
        List<WatchFolder> WatchFolders { get; }
        ICommand AddWatchFolder { get; }
        ICommand DeleteWatchFolder { get; }
    }
}
