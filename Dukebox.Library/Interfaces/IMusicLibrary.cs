using System;
using System.Collections.Generic;
using Dukebox.Library.Model;
using Dukebox.Model.Services;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibrary
    {
        event EventHandler SongAdded;
        event EventHandler ArtistAdded;
        event EventHandler AlbumAdded;
        event EventHandler PlaylistAdded;
        event EventHandler AlbumCacheRefreshed;
        event EventHandler ArtistCacheRefreshed;
        event EventHandler PlaylistCacheRefreshed;
        event EventHandler CachesRefreshed;
        event EventHandler<NotifyCollectionChangedEventArgs> RecentlyPlayedListModified;
        List<album> OrderedAlbums { get; }
        List<artist> OrderedArtists { get; }
        List<playlist> OrderedPlaylists { get; }
        ObservableCollection<Track> RecentlyPlayed { get; }
        List<Track> RecentlyPlayedAsList { get; }
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
        List<Track> SearchForTracksInArea(SearchAreas attribute, string nameOrTitle);
        List<Track> GetTracksByAttributeValue(SearchAreas attribute, string nameOrTitle);
        List<Track> GetTracksByAttributeId(SearchAreas attribute, long attributeId);
        List<Track> GetTracksForDirectory(string directory, bool subDirectories);
        void AddDirectory(string directory, bool subDirectories, Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler);
        void AddFile(string filename, AudioFileMetaData metadata);
        void AddPlaylist(string name, IEnumerable<string> filenames);
        void AddPlaylistFile(string filename);
        void RemoveTrack(Track track);
    }
}
