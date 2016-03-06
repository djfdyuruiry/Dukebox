using System;
using System.Collections.Generic;
using Dukebox.Library.Model;
using Dukebox.Model.Services;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibrary
    {
        event EventHandler SongAdded;
        event EventHandler ArtistAdded;
        event EventHandler AlbumAdded;
        event EventHandler PlaylistAdded;
        IList<album> OrderedAlbums { get; }
        IList<artist> OrderedArtists { get; }
        IList<playlist> OrderedPlaylists { get; }
        List<Track> RecentlyPlayed { get; set; }
        album GetAlbumById(long? albumId);
        int GetAlbumCount();
        artist GetArtistById(long? artistId);
        int GetArtistCount();
        playlist GetPlaylistById(long? playlistId);
        int GetPlaylistCount();
        playlist GetPlaylistFromFile(string playlistFile);
        Track GetTrackFromFile(string fileName, AudioFileMetaData metadata = null);
        List<Track> GetTracksForArtist(album album);
        List<Track> GetTracksForAlbum(album album);
        List<Track> SearchForTracks(string searchTerm, List<SearchAreas> searchAreas); 
        List<Track> GetTracksByAttribute(SearchAreas attribute, long attributeId);
        List<Track> GetTracksByAttribute(SearchAreas attribute, string nameOrTitle);
        List<Track> GetTracksForDirectory(string directory, bool subDirectories);
        void AddDirectory(string directory, bool subDirectories, Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler);
        void AddFile(string filename, AudioFileMetaData metadata);
        void AddPlaylist(string name, IEnumerable<string> filenames);
        void AddPlaylistFile(string filename);
        void RemoveTrack(Track track);
    }
}
