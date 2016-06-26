using System.Collections.Generic;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryRepository
    {
        int GetArtistCount();
        int GetAlbumCount();
        int GetPlaylistCount();
        List<ITrack> GetTracksForArtist(string artistName);
        List<ITrack> GetTracksForAlbum(string albumName);
        Playlist GetPlaylistById(long? playlistId);
    }
}
