﻿using System;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryEventService
    {
        event EventHandler AlbumsAdded;
        event EventHandler ArtistsAdded;
        event EventHandler SongsAdded;
        event EventHandler<Song> SongAdded;
        event EventHandler<Song> SongUpdated;
        event EventHandler<Song> SongDeleted;
        event EventHandler PlaylistsAdded;
        event EventHandler ArtistCacheRefreshed;
        event EventHandler AlbumCacheRefreshed;
        event EventHandler PlaylistCacheRefreshed;
        event EventHandler CachesRefreshed;
        event EventHandler DatabaseChangesSaved;
        event EventHandler<WatchFolder> WatchFolderUpdated;
        event EventHandler<WatchFolder> WatchFolderDeleted;
        void TriggerEvent(MusicLibraryEvent eventToTrigger);
        void TriggerSongAdded(Song song);
        void TriggerSongUpdated(Song song);
        void TriggerSongDeleted(Song song);
        void TriggerWatchFolderUpdated(WatchFolder song);
        void TriggerWatchFolderDeleted(WatchFolder song);
    }
}
