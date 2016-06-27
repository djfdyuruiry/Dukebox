using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Dukebox.Audio;
using Dukebox.Configuration.Interfaces;
using Dukebox.Library.Factories;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services.MusicLibrary
{
    public class MusicLibraryImportService : IMusicLibraryImportService
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDukeboxSettings _settings;
        private readonly AudioFileFormats _audioFormats;

        private readonly IMusicLibraryDbContextFactory _dbContextFactory;
        private readonly AudioFileMetadataFactory _audioFileMetadataFactory;
        private readonly TrackFactory _trackFactory;

        private readonly IMusicLibraryCacheService _cacheService;
        private readonly IMusicLibraryEventService _eventService;
        private readonly IMusicLibraryUpdateService _updateService;
        private readonly IAlbumArtCacheService _albumArtCache;

        private readonly IPlaylistGeneratorService _playlistGenerator;

        public MusicLibraryImportService(IDukeboxSettings settings, AudioFileFormats audioFormats, IMusicLibraryDbContextFactory dbContextFactory,
            AudioFileMetadataFactory metadataFactory, TrackFactory trackFactory, IMusicLibraryCacheService cacheService, IMusicLibraryUpdateService updateService,
            IMusicLibraryEventService eventService, IAlbumArtCacheService albumCacheServices, IPlaylistGeneratorService playlistGenerator)
        {
            _settings = settings;
            _audioFormats = audioFormats;

            _dbContextFactory = dbContextFactory;
            _audioFileMetadataFactory = metadataFactory;
            _trackFactory = trackFactory;

            _cacheService = cacheService;
            _updateService = updateService;
            _eventService = eventService;
            _albumArtCache = albumCacheServices;

            _playlistGenerator = playlistGenerator;
        }

        public async Task AddSupportedFilesInDirectory(string directory, bool subDirectories, 
            Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler)
        {
            await Task.Run(() => DoAddSupportedFilesInDirectory(directory, subDirectories, progressHandler, completeHandler));
        }

        private void DoAddSupportedFilesInDirectory(string directory, bool subDirectories, 
            Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(string.Format("Unable add files from directory '{0}' as it does not exist on this system", directory));
            }

            var stopwatch = Stopwatch.StartNew();

            var concurrencyLimit = _settings.AddDirectoryConcurrencyLimit;
            var allfiles = Directory.GetFiles(@directory, "*.*", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            var filesToAdd = allfiles.Where(f => !_cacheService.FilesCache.Contains(f) && _audioFormats.FileSupported(f));
            var filesToRemove = _cacheService.FilesCache.Where(f => f.StartsWith(directory) && !allfiles.Contains(f)).ToList();
            var numFilesToAdd = filesToAdd.Count();

            var filesWithMetadata = ExtractMetadataFromFiles(filesToAdd, progressHandler, concurrencyLimit, numFilesToAdd);
            List<Tuple<Album, IAudioFileMetadata>> albumsWithMetadata;

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                albumsWithMetadata = AddFilesToDatabaseModel(dukeboxData, filesWithMetadata, concurrencyLimit, progressHandler, numFilesToAdd);

                logger.Info($"Removing {filesToRemove.Count} library song(s) missing (deleted, renamed or moved) from the path '{directory}'");
                filesToRemove.ForEach(f => _updateService.RemoveSongByFilePath(f));

                _dbContextFactory.SaveDbChanges(dukeboxData);
            }

            var filesAdded = albumsWithMetadata.Count;

            if (!albumsWithMetadata.Any())
            {
                logger.WarnFormat("No new supported files were found in directory '{0}'", directory);
                Task.Run(() => completeHandler(this, filesAdded));
                return;
            }

            AddAlbumArtToCache(albumsWithMetadata, concurrencyLimit);

            stopwatch.Stop();

            logger.InfoFormat("Added {0} tracks from directory: {1}", filesAdded, directory);
            logger.DebugFormat("Adding {0} tracks to library from a directory took {1}ms. Directory path: {2} (Sub-directories searched: {3})",
                filesAdded, stopwatch.ElapsedMilliseconds, directory, subDirectories);

            CallMetadataAndCompleteHandlers(completeHandler, filesAdded);

            if (numFilesToAdd > filesAdded)
            {
                logger.WarnFormat("Not all files found in directory '{0}' were added to the database [{1}/{2} files added]",
                    directory, filesAdded, numFilesToAdd);
            }
        }

        private List<Tuple<string, IAudioFileMetadata>> ExtractMetadataFromFiles(IEnumerable<string> filesToAdd, 
            Action<object, AudioFileImportedEventArgs> progressHandler,
            int concurrencyLimit, int numFilesToAdd)
        {
            return filesToAdd.AsParallel().WithDegreeOfParallelism(concurrencyLimit).Select(f =>
            {
                var metadataTuple = new Tuple<string, IAudioFileMetadata>(f, _audioFileMetadataFactory.BuildAudioFileMetadataInstance(f));

                Task.Run(() => progressHandler?.Invoke(this, new AudioFileImportedEventArgs
                {
                    JustProcessing = true,
                    FileAdded = f,
                    TotalFilesThisImport = numFilesToAdd
                }));

                return metadataTuple;
            }).ToList();
        }

        private List<Tuple<Album, IAudioFileMetadata>> AddFilesToDatabaseModel(IMusicLibraryDbContext dukeboxData, 
            List<Tuple<string, IAudioFileMetadata>> filesWithMetadata,
            int concurrencyLimit, Action<object, AudioFileImportedEventArgs> progressHandler, int numFilesToAdd)
        {
            return filesWithMetadata.AsParallel().WithDegreeOfParallelism(concurrencyLimit).Select(fileWithMetdata =>
            {
                try
                {
                    var song = AddFile(dukeboxData, fileWithMetdata.Item1, fileWithMetdata.Item2);

                    Task.Run(() => progressHandler?.Invoke(this, new AudioFileImportedEventArgs
                    {
                        JustProcessing = false,
                        FileAdded = fileWithMetdata.Item1,
                        TotalFilesThisImport = numFilesToAdd
                    }));

                    return new Tuple<Album, IAudioFileMetadata>(song.Album, fileWithMetdata.Item2);
                }
                catch (Exception ex)
                {
                    logger.Error("Error adding audio file to the database while adding files from directory", ex);
                    return null;
                }
            }).Where(am => am != null).ToList();
        }

        public void AddAlbumArtToCache(List<Tuple<Album, IAudioFileMetadata>> albumsWithMetadata, int concurrencyLimit)
        {
            albumsWithMetadata = albumsWithMetadata
                .Where(at => at.Item1 != null && (at.Item2?.HasAlbumArt).HasValue && (at.Item2?.HasAlbumArt).Value)
                .GroupBy(at => at.Item1.Name)
                .Select(g => g.First())
                .ToList();

            albumsWithMetadata.AsParallel().WithDegreeOfParallelism(concurrencyLimit).Select(am =>
            {
                _albumArtCache.AddAlbumToCache(am.Item1, am.Item2);
                return 0;
            }).ToList();
        }

        private void CallMetadataAndCompleteHandlers(Action<object, int> completeHandler, int filesAdded)
        {
            _eventService.TriggerEvent(MusicLibraryEvent.AlbumsAdded);
            _eventService.TriggerEvent(MusicLibraryEvent.ArtistsAdded);
            _eventService.TriggerEvent(MusicLibraryEvent.SongsAdded);

            Task.Run(() => completeHandler?.Invoke(this, filesAdded));
        }

        public Song AddFile(string filename)
        {
            return AddFile(filename, null);
        }

        public Song AddFile(string filename, IAudioFileMetadata metadata)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                if (_cacheService.FilesCache.Contains(filename))
                {
                    return dukeboxData.Songs.First(s => s.FileName.Equals(filename));
                }

                var song = AddFile(dukeboxData, filename, metadata);

                dukeboxData.SaveChanges();

                _eventService.TriggerSongAdded(song);
                _eventService.TriggerEvent(MusicLibraryEvent.ArtistsAdded);
                _eventService.TriggerEvent(MusicLibraryEvent.AlbumsAdded);

                return song;
            }
        }

        private Song AddFile(IMusicLibraryDbContext dukeboxData, string filename)
        {
            return AddFile(dukeboxData, filename, null);
        }

        private Song AddFile(IMusicLibraryDbContext dukeboxData, string filename, IAudioFileMetadata metadata)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(string.Format("Could not add file '{0}' as it is was not found on this system", filename));
            }

            if (!_audioFormats.FileSupported(filename))
            {
                throw new FileFormatException(string.Format("Error adding file '{0}': this format is not supported", filename));
            }

            if (metadata == null)
            {
                metadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(filename);
            }

            var newSong = AddSongWithMetadataToDb(dukeboxData, filename, metadata);

            return newSong;
        }

        private Song AddSongWithMetadataToDb(IMusicLibraryDbContext dukeboxData, string filename, IAudioFileMetadata metadata)
        {
            var existingSongFile = _cacheService.FilesCache.FirstOrDefault(f => f.Equals(filename, StringComparison.CurrentCulture));

            if (existingSongFile != null)
            {
                var existingSong = dukeboxData.Songs.FirstOrDefault(s => s.FileName.Equals(existingSongFile, StringComparison.CurrentCulture));

                logger.WarnFormat("Not adding song from file '{0}' as a song with that filename already exists in the database", filename);
                return existingSong;
            }

            var extendedMetadataJson = JsonConvert.SerializeObject(metadata.ExtendedMetadata);
            var newSong = new Song()
            {
                FileName = filename,
                Title = metadata.Title,
                ArtistName = metadata.Artist,
                AlbumName = metadata.Album,
                ExtendedMetadataJson = extendedMetadataJson,
                LengthInSeconds = metadata.Length
            };

            logger.DebugFormat("Title for file '{0}': {1} [artist = '{2}', album = '{3}']",
                newSong.FileName, newSong.Title, newSong.Artist, newSong.Album);

            if (string.IsNullOrEmpty(newSong.Title))
            {
                var errMsg = string.Format("Title for file '{0}' is null or empty", filename);

                logger.Error(errMsg);
                throw new InvalidDataException(errMsg);
            }

            dukeboxData.SynchronisedAddSong(newSong);

            _cacheService.FilesCache.Add(filename);

            logger.InfoFormat("New song: {0}.", filename);

            return newSong;
        }
        
        public async Task<List<ITrack>> AddPlaylistFiles(string filename)
        {
            return await Task.Run(() =>
            {
                var playlist = _playlistGenerator.GetPlaylistFromFile(filename);
                var addFilesTasks = playlist.Files.Select(f =>
                {
                    Song song = null;

                    try
                    {
                        using (var dukeboxData = _dbContextFactory.GetInstance())
                        {
                            song = AddFile(dukeboxData, f);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(string.Format("Error adding file from ('{0}') playlist to the database", f), ex);
                    }

                    return song;
                }).ToArray();

                var filesAdded = addFilesTasks.Where(s => s != null).Select(s => _trackFactory.BuildTrackInstance(s)).ToList();
                return filesAdded;
            });
        }

        public async Task<Playlist> AddPlaylist(string name, IEnumerable<string> filenames)
        {
            try
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }

                Playlist existingPlaylist;

                using (var dukeboxData = _dbContextFactory.GetInstance())
                {
                    existingPlaylist = dukeboxData.Playlists.FirstOrDefault(a => a.Name.Equals(name, StringComparison.CurrentCulture));
                }

                if (existingPlaylist != null)
                {
                    logger.WarnFormat("Not adding playlist with name '{0}' as another playlist with the same name already exists in the database", name);
                    return existingPlaylist;
                }

                if (filenames == null)
                {
                    throw new ArgumentNullException("filenames");
                }

                var stopwatch = Stopwatch.StartNew();
                var newPlaylist = new Playlist() { Name = name, FilenamesCsv = string.Join(",", filenames) };

                using (var dukeboxData = _dbContextFactory.GetInstance())
                {
                    dukeboxData.Playlists.Add(newPlaylist);
                    await _dbContextFactory.SaveDbChanges(dukeboxData);
                }

                // Invalidate playlist cache.
                _cacheService.RefreshCaches();

                stopwatch.Stop();
                logger.InfoFormat("Added playlist to library: {0}", newPlaylist.Name);
                logger.DebugFormat("Adding playlist to library took {0}ms. Playlist id: {1}", stopwatch.ElapsedMilliseconds, newPlaylist.Id);

                _eventService.TriggerEvent(MusicLibraryEvent.PlaylistsAdded);

                return newPlaylist;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error adding playlist '{0}' to database", name), ex);
            }

            return null;
        }

        public async Task<WatchFolder> AddWatchFolder(WatchFolder watchFolder)
        {
            if (watchFolder == null)
            {
                throw new ArgumentNullException("watchFolder");
            }
            else if (string.IsNullOrWhiteSpace(watchFolder.FolderPath))
            {
                throw new ArgumentNullException("watchFolder.FolderPath");
            }

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var existingWatchFolder = dukeboxData.WatchFolders.FirstOrDefault(w => w.FolderPath.Equals(watchFolder.FolderPath));

                if (existingWatchFolder != null)
                {
                    return existingWatchFolder;
                }

                dukeboxData.WatchFolders.Add(watchFolder);
                await _dbContextFactory.SaveDbChanges(dukeboxData);

                return watchFolder;
            }
        }
    }
}
