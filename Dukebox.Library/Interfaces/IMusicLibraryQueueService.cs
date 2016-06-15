using System;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryQueueService
    {
        event EventHandler DatabaseChangesSaved;
        void QueueMusicLibrarySaveChanges();
    }
}