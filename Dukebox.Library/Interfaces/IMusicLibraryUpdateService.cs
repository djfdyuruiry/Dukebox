using System.Threading.Tasks;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryUpdateService
    {
        Task RemoveTrack(ITrack track);
        Task RemoveSongByFilePath(string filePath);
        Task SaveSongChanges(Song song);
        Task<bool> UpdateSongFilePath(string oldFullPath, string fullPath);
        Task RemoveWatchFolder(WatchFolder watchFolder);
        Task SaveWatchFolderChanges(WatchFolder watchFolder);
    }
}
