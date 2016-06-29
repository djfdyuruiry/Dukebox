using System;
using System.IO;
using System.Threading.Tasks;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IWatchFolderService : IDisposable
    {
        event EventHandler<DirectoryImportReport> ImportCompleted;
        event EventHandler<AudioFileImportedInfo> FileEventProcessed;
        WatchFolder WatchFolder { get; }
        Task StartWatching();
        void StopWatching();
    }
}
