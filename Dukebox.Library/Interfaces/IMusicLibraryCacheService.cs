using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryCacheService
    {
        List<Album> OrderedAlbums { get; }
        List<Artist> OrderedArtists { get; }
        List<Playlist> OrderedPlaylists { get; }
        List<string> FilesCache { get; }
        void RefreshCaches();
        bool HasFileBeenUpdatedSinceLastScan(string file, DateTime lastWriteTime);
    }
}
