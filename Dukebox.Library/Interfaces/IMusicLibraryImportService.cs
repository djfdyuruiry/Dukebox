using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryImportService
    {
        Song AddFile(string filename);
        Song AddFile(string filename, IAudioFileMetadata metadata);
        Task AddSupportedFilesInDirectory(string directory, bool subDirectories, Action<AudioFileImportedInfo> progressHandler, Action<DirectoryImportReport> completeHandler);
        Task<List<ITrack>> AddPlaylistFiles(string filename);
        Task<Playlist> AddPlaylist(string name, IEnumerable<string> filenames);
        Task<WatchFolder> AddWatchFolder(WatchFolder watchFolder);
    }
}
