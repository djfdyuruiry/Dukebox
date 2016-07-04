using System;
using System.Reflection;
using System.Threading.Tasks;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services.MusicLibrary
{
    public class MusicLibraryEventService : IMusicLibraryEventService
    {
        public event EventHandler AlbumsAdded;
        public event EventHandler ArtistsAdded;
        public event EventHandler SongsAdded;
        public event EventHandler<Song> SongAdded;
        public event EventHandler<Song> SongUpdated;
        public event EventHandler<Song> SongDeleted;
        public event EventHandler PlaylistsAdded;
        public event EventHandler ArtistCacheRefreshed;
        public event EventHandler AlbumCacheRefreshed;
        public event EventHandler PlaylistCacheRefreshed;
        public event EventHandler FilesCacheRefreshed; 
        public event EventHandler CachesRefreshed;
        public event EventHandler DatabaseChangesSaved;
        public event EventHandler<WatchFolder> WatchFolderUpdated;
        public event EventHandler<WatchFolder> WatchFolderDeleted;

        public void TriggerEvent(MusicLibraryEvent eventToTrigger)
        {
            var eventDelegate = GetType()
                .GetField(eventToTrigger.Name, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this) as Delegate;            

            Task.Run(() => eventDelegate?.DynamicInvoke(this, EventArgs.Empty));
        }

        public void TriggerSongAdded(Song song)
        {
            Task.Run(() => SongAdded?.Invoke(this, song));
        }

        public void TriggerSongUpdated(Song song)
        {
            Task.Run(() => SongUpdated?.Invoke(this, song));
        }

        public void TriggerSongDeleted(Song song)
        {
            Task.Run(() => SongDeleted?.Invoke(this, song));
        }

        public void TriggerWatchFolderUpdated(WatchFolder watchFolder)
        {
            Task.Run(() => WatchFolderUpdated?.Invoke(this, watchFolder));
        }

        public void TriggerWatchFolderDeleted(WatchFolder watchFolder)
        {
            Task.Run(() => WatchFolderDeleted?.Invoke(this, watchFolder));
        }
    }
}
