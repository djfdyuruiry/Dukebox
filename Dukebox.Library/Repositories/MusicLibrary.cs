using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Dukebox.Audio;
using Dukebox.Library.Factories;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Services;
using Dukebox.Configuration.Interfaces;

namespace Dukebox.Library.Repositories
{
    /// <summary>
    /// Handles the SQL music library ADO entities model. Preforms
    /// lookups and adds content to the music library database. Also
    /// allows you to fetch temporary models of audio files from directories 
    /// and playlists.
    /// </summary>
    public class MusicLibrary : IMusicLibrary
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly IMusicLibraryDbContextFactory _dbContextFactory;
        private readonly IDukeboxSettings _settings;
        private readonly AudioFileFormats _audioFormats;
        private readonly IAlbumArtCacheService _albumArtCache;
        private readonly AudioFileMetadataFactory _audioFileMetadataFactory;
        private readonly TrackFactory _trackFactory;
        private readonly IMusicLibrarySearchService _searchService;

        private List<Artist> _allArtistsCache;
        private List<Album> _allAlbumsCache;
        private List<Playlist> _allPlaylistsCache;
        private List<string> _allFilesCache;

        public event EventHandler SongAdded;
        public event EventHandler ArtistAdded;
        public event EventHandler AlbumAdded;
        public event EventHandler PlaylistAdded;
        public event EventHandler AlbumCacheRefreshed;
        public event EventHandler ArtistCacheRefreshed;
        public event EventHandler PlaylistCacheRefreshed;
        public event EventHandler CachesRefreshed;
        public event EventHandler DatabaseChangesSaved;
        public event EventHandler<NotifyCollectionChangedEventArgs> RecentlyPlayedListModified;

        #region Views on music library data
        
        /// <summary>
        /// All artists currently stored in the database. Cached.
        /// </summary>
        public List<Artist> OrderedArtists
        {
            get
            {
                if (_allArtistsCache == null)
                {
                    using (var dukeboxData = _dbContextFactory.GetInstance())
                    { 
                        _allArtistsCache = dukeboxData.Artists.OrderBy(a => a.Name).ToList();
                    }

                    ArtistCacheRefreshed?.Invoke(this, EventArgs.Empty);
                }

                return _allArtistsCache;
            }
        }

        /// <summary>
        /// All albums currently stored in the database. Cached.
        /// </summary>
        public List<Album> OrderedAlbums
        {
            get
            {
                if (_allAlbumsCache == null)
                {
                    using (var dukeboxData = _dbContextFactory.GetInstance())
                    {
                        _allAlbumsCache = dukeboxData.Albums.OrderBy(a => a.Name).ToList();
                    }

                    AlbumCacheRefreshed?.Invoke(this, EventArgs.Empty);
                }

                return _allAlbumsCache;
            }
        }

        public List<Playlist> OrderedPlaylists        
        {
            get
            {
                if (_allPlaylistsCache == null)
                {
                    using (var dukeboxData = _dbContextFactory.GetInstance())
                    { 
                        _allPlaylistsCache = dukeboxData.Playlists.OrderBy(a => a.Name).ToList();
                    }

                    PlaylistCacheRefreshed?.Invoke(this, EventArgs.Empty);
                }

                return _allPlaylistsCache;
            }
        }
        
        public ObservableCollection<ITrack> RecentlyPlayed { get; private set; }

        public List<ITrack> RecentlyPlayedAsList
        {
            get
            {
                return RecentlyPlayed.ToList();
            }
        }

        #endregion
        
        public MusicLibrary(IMusicLibraryDbContextFactory dbContextFactory, IDukeboxSettings settings, IAlbumArtCacheService albumArtCache, 
            AudioFileFormats audioFormats, AudioFileMetadataFactory audioFileMetadataFactory, TrackFactory trackFactory)
        {
            _dbContextFactory = dbContextFactory;
            _settings = settings;
            _audioFormats = audioFormats;
            _albumArtCache = albumArtCache;
            _audioFileMetadataFactory = audioFileMetadataFactory;

            _trackFactory = trackFactory;

            _searchService = new MusicLibrarySearchService(dbContextFactory, this, _trackFactory);
            
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                _allFilesCache = dukeboxData.Songs.Select(s => s.FileName).ToList();
            }

            RecentlyPlayed = new ObservableCollection<ITrack>();
            RecentlyPlayed.CollectionChanged += RecentlyPlayedChangedHander;

            RefreshCaches();
        }

        private void RecentlyPlayedChangedHander(object source, NotifyCollectionChangedEventArgs eventData)
        {
            RecentlyPlayedListModified?.Invoke(this, eventData);
        }

        #region Folder/Playlist/File playback methods
        
        public List<ITrack> GetTracksForDirectory(string directory, bool subDirectories)
        {
            var searchOption = subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var supportedFormats = _audioFormats.SupportedFormats;

            var validFiles = Directory.EnumerateFiles(directory, "*.*", searchOption).Where((f) => supportedFormats.Any(sf => f.EndsWith(sf))).ToList();
            var libraryTracks = validFiles.SelectMany(f => GetTracksByAttributeValue(SearchAreas.Filename, f)).ToList();

            validFiles = validFiles.Except(libraryTracks.Select(t => t.Song.FileName)).ToList();

            var tracksToReturn = validFiles.Select((f) => _trackFactory.BuildTrackInstance(f)).Concat(libraryTracks).ToList();
            return tracksToReturn;
        }

        public List<ITrack> GetTracksForPlaylist(Playlist playlist)
        {
            var libraryTracks = playlist.Files.SelectMany(f => GetTracksByAttributeValue(SearchAreas.Filename, f)).ToList();
            var nonLibraryFiles = playlist.Files.Except(libraryTracks.Select(t => t.Song.FileName)).ToList();

            var tracks = nonLibraryFiles.Select(file =>
            {
                try
                {
                    return _trackFactory.BuildTrackInstance(file);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error while loading track from file '{0}' for playlist {1}", file, ToString()), ex);
                }
            }).Concat(libraryTracks).ToList();

            return tracks;
        }
        
        public Playlist GetPlaylistFromFile(string playlistFile)
        {
            if (!File.Exists(playlistFile))
            {
                throw new FileNotFoundException(string.Format("The playlist file '{0}' does not exist on this system", playlistFile));
            }

            using (var playlistFileReader = new StreamReader(playlistFile))
            {
                var jsonTracks = playlistFileReader.ReadToEnd();

                var files = JsonConvert.DeserializeObject<List<string>>(jsonTracks);

                var playlist = new Playlist() { Id = -1, FilenamesCsv = string.Join(",", files) };
                return playlist;
            }
        }

        #endregion

        #region Database update methods
        
        public async Task AddSupportedFilesInDirectory(string directory, bool subDirectories, Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler)
        {
            await Task.Run(() => DoAddSupportedFilesInDirectory(directory, subDirectories, progressHandler, completeHandler));
        }

        private void DoAddSupportedFilesInDirectory(string directory, bool subDirectories, Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(string.Format("Unable add files from directory '{0}' as it does not exist on this system", directory));
            }

            var stopwatch = Stopwatch.StartNew();

            var concurrencyLimit = _settings.AddDirectoryConcurrencyLimit;
            var allfiles = Directory.GetFiles(@directory, "*.*", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            var filesToAdd = allfiles.Where(f => !_allFilesCache.Contains(f) && _audioFormats.FileSupported(f));
            var numFilesToAdd = filesToAdd.Count();
            
            var filesWithMetadata = ExtractMetadataFromFiles(filesToAdd, progressHandler, concurrencyLimit, numFilesToAdd);
            List<Tuple<Album, IAudioFileMetadata>> albumsWithMetadata;

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                albumsWithMetadata = AddFilesToDatabaseModel(dukeboxData, filesWithMetadata, concurrencyLimit, progressHandler, numFilesToAdd);
                SaveDbChanges(dukeboxData);
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

        private List<Tuple<string, IAudioFileMetadata>> ExtractMetadataFromFiles(IEnumerable<string> filesToAdd, Action<object, AudioFileImportedEventArgs> progressHandler,
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

        private List<Tuple<Album, IAudioFileMetadata>> AddFilesToDatabaseModel(IMusicLibraryDbContext dukeboxData, List<Tuple<string, IAudioFileMetadata>> filesWithMetadata, 
            int concurrencyLimit, Action<object, AudioFileImportedEventArgs> progressHandler, int numFilesToAdd)
        {
            // For each set of 5 files, create a new thread
            // which processes the metadata of those 
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
            Task.Run(() => ArtistAdded?.Invoke(this, EventArgs.Empty));
            Task.Run(() => AlbumAdded?.Invoke(this, EventArgs.Empty));
            Task.Run(() => SongAdded?.Invoke(this, EventArgs.Empty));

            Task.Run(() => completeHandler?.Invoke(this, filesAdded));
        }

        public Song AddFile(string filename)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var song = AddFile(dukeboxData, filename);

                dukeboxData.SaveChanges();

                return song;
            }
        }

        private Song AddFile(IMusicLibraryDbContext dukeboxData, string filename)
        {
            return AddFile(dukeboxData, filename, null);
        }

        public Song AddFile(string filename, IAudioFileMetadata metadata)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var song = AddFile(dukeboxData, filename, metadata);

                dukeboxData.SaveChanges();

                return song;
            }
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
            
            var newSong = BuildSongFromMetadata(dukeboxData, filename, metadata);

            return newSong;
        }
        
        private Song BuildSongFromMetadata(IMusicLibraryDbContext dukeboxData, string filename, IAudioFileMetadata metadata)
        {
            var existingSongFile = _allFilesCache.FirstOrDefault(f => f.Equals(filename, StringComparison.CurrentCulture));

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
            
            if(string.IsNullOrEmpty(newSong.Title))
            {
                var errMsg = string.Format("Title for file '{0}' is null or empty", filename);

                logger.Error(errMsg);
                throw new InvalidDataException(errMsg);
            }

            dukeboxData.SynchronisedAddSong(newSong);

            _allFilesCache.Add(filename);

            logger.InfoFormat("New song: {0}.", filename);

            return newSong;
        }

        /// <summary>
        /// Add all files specified in a playlist to the library and save changes
        /// to database.
        /// </summary>
        public async Task<List<ITrack>> AddPlaylistFiles(string filename)
        {
            return await Task.Run(() =>
            {
                var playlist = GetPlaylistFromFile(filename);
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
                    await dukeboxData.SaveChangesAsync();
                }

                // Invalidate playlist cache.
                _allPlaylistsCache = null;

                stopwatch.Stop();
                logger.InfoFormat("Added playlist to library: {0}", newPlaylist.Name);
                logger.DebugFormat("Adding playlist to library took {0}ms. Playlist id: {1}", stopwatch.ElapsedMilliseconds, newPlaylist.Id);

                PlaylistAdded?.Invoke(this.PlaylistAdded, EventArgs.Empty);

                return newPlaylist;
            }
            catch (DbEntityValidationException ex)
            {
                logger.ErrorFormat("Error adding playlist '{0}' to database due to entity validation errors", name);
                _dbContextFactory.LogEntityValidationException(ex);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error adding playlist '{0}' to database", name), ex);
            }

            return null;
        }

        public async Task SaveSongChanges(Song song)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                dukeboxData.Songs.Attach(song);
                dukeboxData.Entry(song).State = EntityState.Modified;

                await SaveDbChanges(dukeboxData);
            }
        }

        public async Task SaveDbChanges(IMusicLibraryDbContext dukeboxData)
        {
            try
            {
                await dukeboxData.SaveChangesAsync();
                DatabaseChangesSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (DbEntityValidationException ex)
            {
                logger.Error("Error updating database due to entity validation errors", ex);
                _dbContextFactory.LogEntityValidationException(ex);
            }
            catch (Exception ex)
            {
                logger.Error("Error updating database", ex);            
            }

            RefreshCaches();
        }

        public async Task RemoveTrack (ITrack track)
        {
            if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                dukeboxData.Songs.Remove(track.Song);
                await dukeboxData.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Clear the cache of artists and albums. The cache
        /// is automatically refilled at the end of this method.
        /// </summary>
        private void RefreshCaches()
        {
            var stopwatch = Stopwatch.StartNew();

            _allArtistsCache = null;
            _allAlbumsCache = null;
            _allPlaylistsCache = null;
            _allFilesCache = null;

            var albums = OrderedAlbums;
            var artists = OrderedArtists;
            var playlists = OrderedPlaylists;

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                _allFilesCache = dukeboxData.Songs.Select(s => s.FileName).ToList();
            }

            stopwatch.Stop();
            logger.Info("Music library artist and album caches refreshed! (This happens after a DB update routine)");
            logger.DebugFormat("Refreshing library artist and album caches took {0}ms.", stopwatch.ElapsedMilliseconds);

            Task.Run(() => CachesRefreshed?.Invoke(this, EventArgs.Empty));
        }

        #endregion

        #region Library Lookups

        public List<ITrack> GetTracksForArtist(string artistName)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            { 
                return dukeboxData.Songs
                    .Where(s => s.ArtistName.Equals(artistName))
                    .ToList()
                    .Select(s => _trackFactory.BuildTrackInstance(s))
                    .ToList();
            }
        }

        public List<ITrack> GetTracksForAlbum(string albumName)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            { 
                return dukeboxData.Songs
                    .Where(s => s.AlbumName.Equals(albumName))
                    .ToList()
                    .Select(s => _trackFactory.BuildTrackInstance(s))
                    .ToList();
            }
        }

        public int GetArtistCount()
        {
            return OrderedArtists.Count;
        }

        public int GetAlbumCount()
        {
            return OrderedAlbums.Count;
        }

        public Playlist GetPlaylistById(long? playlistId)
        {
            var stopwatch = Stopwatch.StartNew();
            Playlist playlist;

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                playlist = dukeboxData.Playlists.Where(p => p.Id == playlistId).FirstOrDefault();
            }

            stopwatch.Stop();

            if (playlist == null)
            {
                throw new Exception(string.Format("No playlist with the id '{0}' was found in the database.", playlistId));
            }

            logger.DebugFormat("Look up for artist with id '{0}' took {1}ms.", playlistId, stopwatch.ElapsedMilliseconds);
            return playlist;
        }

        public int GetPlaylistCount()
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                return dukeboxData.Playlists.Count();
            }
        }

        #endregion

        #region Library search

        public List<ITrack> SearchForTracks(string searchTerm, List<SearchAreas> searchAreas)
        {
            return _searchService.SearchForTracks(searchTerm, searchAreas);
        }

        public List<ITrack> SearchForTracksInArea(SearchAreas attribute, string nameOrTitle)
        {
            return SearchForTracks(nameOrTitle, new List<SearchAreas> { attribute });
        }

        public List<ITrack> GetTracksByAttributeValue(SearchAreas attribute, string attributeValue)
        {
            return _searchService.GetTracksByAttributeValue(attribute, attributeValue);
        }
        public List<ITrack> GetTracksByAttributeId(SearchAreas attribute, string attributeId)
        {
            return _searchService.GetTracksByAttributeId(attribute, attributeId);
        }

        #endregion
    }
}
