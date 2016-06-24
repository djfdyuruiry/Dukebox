using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Dukebox.Library.Model;

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
        event EventHandler DatabaseChangesSaved;
        event EventHandler<NotifyCollectionChangedEventArgs> RecentlyPlayedListModified;
        List<Album> OrderedAlbums { get; }
        List<Artist> OrderedArtists { get; }
        List<Playlist> OrderedPlaylists { get; }
        ObservableCollection<ITrack> RecentlyPlayed { get; }
        List<ITrack> RecentlyPlayedAsList { get; }
        int GetAlbumCount();
        int GetArtistCount();
        Playlist GetPlaylistById(long? playlistId);
        int GetPlaylistCount();
        Playlist GetPlaylistFromFile(string playlistFile);
        List<ITrack> GetTracksForArtist(string artistName);
        List<ITrack> GetTracksForAlbum(string albumName);
        List<ITrack> SearchForTracks(string searchTerm, List<SearchAreas> searchAreas);
        List<ITrack> SearchForTracksInArea(SearchAreas attribute, string nameOrTitle);
        List<ITrack> GetTracksByAttributeValue(SearchAreas attribute, string nameOrTitle);
        List<ITrack> GetTracksByAttributeId(SearchAreas attribute, string attributeId);
        List<ITrack> GetTracksForDirectory(string directory, bool subDirectories);
        List<ITrack> GetTracksForPlaylist(Playlist playlist);
        Song AddFile(string filename);
        Song AddFile(string filename, IAudioFileMetadata metadata);
        Task AddSupportedFilesInDirectory(string directory, bool subDirectories, Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler);
        Task<List<ITrack>> AddPlaylistFiles(string filename);
        Task<Playlist> AddPlaylist(string name, IEnumerable<string> filenames);
        Task RemoveTrack(ITrack track);
        Task SaveSongChanges(Song song);
    }
}
