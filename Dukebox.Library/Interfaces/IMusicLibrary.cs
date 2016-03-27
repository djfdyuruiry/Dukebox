using System;
using System.Collections.Generic;
using Dukebox.Library.Model;
using Dukebox.Library.Services;
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
        List<Album> OrderedAlbums { get; }
        List<Artist> OrderedArtists { get; }
        List<Playlist> OrderedPlaylists { get; }
        ObservableCollection<ITrack> RecentlyPlayed { get; }
        List<ITrack> RecentlyPlayedAsList { get; }
        Album GetAlbumById(long? albumId);
        int GetAlbumCount();
        Artist GetArtistById(long? artistId);
        int GetArtistCount();
        Playlist GetPlaylistById(long? playlistId);
        int GetPlaylistCount();
        Playlist GetPlaylistFromFile(string playlistFile);
        List<ITrack> GetTracksForArtist(Artist artist);
        List<ITrack> GetTracksForAlbum(Album album);
        List<ITrack> SearchForTracks(string searchTerm, List<SearchAreas> searchAreas);
        List<ITrack> SearchForTracksInArea(SearchAreas attribute, string nameOrTitle);
        List<ITrack> GetTracksByAttributeValue(SearchAreas attribute, string nameOrTitle);
        List<ITrack> GetTracksByAttributeId(SearchAreas attribute, long attributeId);
        List<ITrack> GetTracksForDirectory(string directory, bool subDirectories);
        void AddDirectory(string directory, bool subDirectories, Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler);
        void AddFile(string filename, IAudioFileMetadata metadata);
        void AddPlaylist(string name, IEnumerable<string> filenames);
        void AddPlaylistFile(string filename);
        void RemoveTrack(ITrack track);
    }
}
