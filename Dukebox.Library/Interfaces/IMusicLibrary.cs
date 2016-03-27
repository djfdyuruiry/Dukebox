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
        List<album> OrderedAlbums { get; }
        List<artist> OrderedArtists { get; }
        List<playlist> OrderedPlaylists { get; }
        ObservableCollection<ITrack> RecentlyPlayed { get; }
        List<ITrack> RecentlyPlayedAsList { get; }
        album GetAlbumById(long? albumId);
        int GetAlbumCount();
        artist GetArtistById(long? artistId);
        int GetArtistCount();
        playlist GetPlaylistById(long? playlistId);
        int GetPlaylistCount();
        playlist GetPlaylistFromFile(string playlistFile);
        ITrack GetTrackFromFile(string fileName, AudioFileMetadata metadata = null);
        List<ITrack> GetTracksForArtist(artist artist);
        List<ITrack> GetTracksForAlbum(album album);
        List<ITrack> SearchForTracks(string searchTerm, List<SearchAreas> searchAreas);
        List<ITrack> SearchForTracksInArea(SearchAreas attribute, string nameOrTitle);
        List<ITrack> GetTracksByAttributeValue(SearchAreas attribute, string nameOrTitle);
        List<ITrack> GetTracksByAttributeId(SearchAreas attribute, long attributeId);
        List<ITrack> GetTracksForDirectory(string directory, bool subDirectories);
        void AddDirectory(string directory, bool subDirectories, Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler);
        void AddFile(string filename, AudioFileMetadata metadata);
        void AddPlaylist(string name, IEnumerable<string> filenames);
        void AddPlaylistFile(string filename);
        void RemoveTrack(ITrack track);
    }
}
