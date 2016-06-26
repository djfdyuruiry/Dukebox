using System;
using System.Reflection;
using System.Threading.Tasks;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services.MusicLibrary
{
    public class MusicLibraryEventService : IMusicLibraryEventService
    {
        public event EventHandler AlbumAdded;
        public event EventHandler AlbumCacheRefreshed;
        public event EventHandler ArtistAdded;
        public event EventHandler<Song> SongUpdated;
        public event EventHandler ArtistCacheRefreshed;
        public event EventHandler CachesRefreshed;
        public event EventHandler DatabaseChangesSaved;
        public event EventHandler PlaylistAdded;
        public event EventHandler PlaylistCacheRefreshed;
        public event EventHandler SongAdded;

        public void TriggerEvent(MusicLibraryEvent eventToTrigger)
        {
            TriggerEvent(eventToTrigger, null);
        }

        public void TriggerEvent(MusicLibraryEvent eventToTrigger, object eventArgs)
        {
            var eventDelegate = GetType()
                .GetField(eventToTrigger.Name, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this) as Delegate;            

            Task.Run(() => eventDelegate?.DynamicInvoke(this, eventArgs));
        }
    }
}
