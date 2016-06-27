using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services.MusicLibrary
{
    public class MusicLibraryUpdateService : IMusicLibraryUpdateService
    {
        private readonly IMusicLibraryDbContextFactory _dbContextFactory;
        private readonly IMusicLibraryEventService _eventService;

        public MusicLibraryUpdateService(IMusicLibraryDbContextFactory dbContextFactory, IMusicLibraryEventService eventService)
        {
            _dbContextFactory = dbContextFactory;
            _eventService = eventService;
        }

        public async Task RemoveTrack(ITrack track)
        {
            if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var song = dukeboxData.Songs.FirstOrDefault(s => s.Id == track.Song.Id);

                if (song == null)
                {
                    return;
                }

                dukeboxData.Songs.Remove(song);
                await _dbContextFactory.SaveDbChanges(dukeboxData);

                _eventService.TriggerSongDeleted(song);
            }
        }

        public async Task RemoveSongByFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var song = dukeboxData.Songs.FirstOrDefault(s => s.FileName.Equals(filePath));

                if (song == null)
                {
                    return;
                }

                dukeboxData.Songs.Remove(song);
                await _dbContextFactory.SaveDbChanges(dukeboxData);

                _eventService.TriggerSongDeleted(song);
            }
        }

        public async Task SaveSongChanges(Song song)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                dukeboxData.Songs.Attach(song);

                var dbEntity = dukeboxData.Entry(song);

                if (dbEntity != null)
                {
                    dbEntity.State = EntityState.Modified;
                    await _dbContextFactory.SaveDbChanges(dukeboxData);

                    _eventService.TriggerSongUpdated(song);
                }
            }
        }

        public async Task<bool> UpdateSongFilePath(string oldFullPath, string fullPath)
        {
            if (string.IsNullOrWhiteSpace(oldFullPath))
            {
                throw new ArgumentNullException("oldFullPath");
            }
            else if (string.IsNullOrWhiteSpace(fullPath))
            {
                throw new ArgumentNullException("filePath");
            }
            
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var song = dukeboxData.Songs.FirstOrDefault(s => s.FileName.Equals(oldFullPath));

                if (song == null)
                {
                    return false;
                }

                song.FileName = fullPath;

                await _dbContextFactory.SaveDbChanges(dukeboxData);

                _eventService.TriggerSongUpdated(song);

                return true;
            }
        }

        public async Task SaveWatchFolderChanges(WatchFolder watchFolder)
        {
            if (watchFolder == null)
            {
                throw new ArgumentNullException("watchFolder");
            }

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                dukeboxData.WatchFolders.Attach(watchFolder);

                var dbEntity = dukeboxData.Entry(watchFolder);

                if (dbEntity != null)
                {
                    dbEntity.State = EntityState.Modified;
                    await _dbContextFactory.SaveDbChanges(dukeboxData);

                    _eventService.TriggerWatchFolderUpdated(watchFolder);
                }
            }
        }

        public async Task RemoveWatchFolder(WatchFolder watchFolder)
        {
            if (watchFolder == null)
            {
                throw new ArgumentNullException("watchFolder");
            }

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var dbWatchFolder = dukeboxData.WatchFolders.FirstOrDefault(s => s.FolderPath.Equals(watchFolder.FolderPath));

                if (dbWatchFolder == null)
                {
                    return;
                }

                dukeboxData.WatchFolders.Remove(dbWatchFolder);
                await _dbContextFactory.SaveDbChanges(dukeboxData);

                _eventService.TriggerWatchFolderDeleted(watchFolder);
            }
        }
    }
}
