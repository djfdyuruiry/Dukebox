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
        BlockingCollection<string> FilesCache { get; }
        List<Song> SongsCache { get; }
        void RefreshCaches();
    }
}
