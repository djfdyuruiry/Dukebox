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
        List<ITrack> GetTracksForPlaylist(string playlistName);
        Playlist GetPlaylistById(long? playlistId);
        List<WatchFolder> GetWatchFolders();
    }
}
