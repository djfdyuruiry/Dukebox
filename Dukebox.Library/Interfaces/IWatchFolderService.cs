using System;
using System.Threading.Tasks;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IWatchFolderService : IDisposable
    {
        event EventHandler FileEventProcessed;
        WatchFolder WatchFolder { get; }
        Task StartWatching();
        void StopWatching();
    }
}
