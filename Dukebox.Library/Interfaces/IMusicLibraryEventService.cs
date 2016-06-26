using System;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryEventService
    {
        event EventHandler SongAdded;
        event EventHandler ArtistAdded;
        event EventHandler AlbumAdded;
        event EventHandler PlaylistAdded;
        event EventHandler AlbumCacheRefreshed;
        event EventHandler ArtistCacheRefreshed;
        event EventHandler PlaylistCacheRefreshed;
        event EventHandler CachesRefreshed;
        event EventHandler DatabaseChangesSaved;
        void TriggerEvent(MusicLibraryEvent eventToTrigger);
        void TriggerEvent(MusicLibraryEvent eventToTrigger, object eventArgs);
    }
}
