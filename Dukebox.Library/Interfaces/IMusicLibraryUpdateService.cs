using System.Threading.Tasks;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryUpdateService
    {
        Task SaveSongChanges(Song song);
        Task SaveWatchFolderChanges(WatchFolder watchFolder);
        Task SavePlaylistChanges(Playlist _playlist);
        Task<bool> UpdateSongFilePath(string oldFullPath, string fullPath);
        Task RemoveWatchFolder(WatchFolder watchFolder);
        Task RemoveTrack(ITrack track);
        Task RemoveSongByFilePath(string filePath);
        Task RemovePlaylist(Playlist data);
    }
}
