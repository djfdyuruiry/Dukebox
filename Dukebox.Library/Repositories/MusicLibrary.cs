using Dukebox.Audio;
using Dukebox.Library.Factories;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Services;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private const int defaultAddDirectoryConcurrencyLimit = 10;
        private const string addDirectoryConcurrencyLimitConfigKey = "addDirectoryConcurrencyLimit";
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly SemaphoreSlim _dbContextMutex;

        private readonly IMusicLibraryDbContext _dukeboxData;
        private readonly IDukeboxSettings _settings;
        private readonly AudioFileFormats _audioFormats;
        private readonly IAlbumArtCacheService _albumArtCache;
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
                    _dbContextMutex.Wait();
                    _allArtistsCache = _dukeboxData.Artists.OrderBy(a => a.Name).ToList();
                    _dbContextMutex.Release();

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
                    _dbContextMutex.Wait();
                    _allAlbumsCache = _dukeboxData.Albums.OrderBy(a => a.Name).ToList();
                    _dbContextMutex.Release();

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
                    _dbContextMutex.Wait();
                    _allPlaylistsCache = _dukeboxData.Playlists.OrderBy(a => a.Name).ToList();
                    _dbContextMutex.Release();

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
        
        public MusicLibrary(IMusicLibraryDbContext libraryDbContext, IDukeboxSettings settings, 
            IAlbumArtCacheService albumArtCache, AudioFileFormats audioFormats)
        {
            _dbContextMutex = new SemaphoreSlim(1, 1);

            _dukeboxData = libraryDbContext;
            _settings = settings;
            _audioFormats = audioFormats;
            _albumArtCache = albumArtCache;
            _trackFactory = new TrackFactory(_settings);
            _searchService = new MusicLibrarySearchService(libraryDbContext, this, _trackFactory, _dbContextMutex);

            _allFilesCache = _dukeboxData.Songs.Select(s => s.FileName).ToList();

            RecentlyPlayed = new ObservableCollection<ITrack>();
            RecentlyPlayed.CollectionChanged += RecentlyPlayedChangedHander;

            RefreshCaches();
        }

        private void RecentlyPlayedChangedHander(object source, NotifyCollectionChangedEventArgs eventData)
        {
            if (RecentlyPlayedListModified != null)
            {
                RecentlyPlayedListModified(this, eventData);
            }
        }

        #region Folder/Playlist/File playback methods

        /// <summary>
        /// Fetch track objects from a directory of files.
        /// </summary>
        /// <param name="directory">The path to load files from.</param>
        /// <param name="subDirectories">Search all sub directories in the path given?</param>
        /// <returns>A list of tracks </returns>
        /// <exception cref="Exception">If the directory lookup operation fails.</exception>
        public List<ITrack> GetTracksForDirectory(string directory, bool subDirectories)
        {
            var searchOption = subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var supportedFormats = _audioFormats.SupportedFormats;

            var validFiles = Directory.EnumerateFiles(directory, "*.*", searchOption).Where((f) => supportedFormats.Any(sf => f.EndsWith(sf))).ToList();

            _dbContextMutex.Wait();
            validFiles = validFiles.Except(_dukeboxData.Songs.Select((s) => s.FileName)).ToList();
            _dbContextMutex.Release();

            var tracksToReturn = validFiles.Select((f) => _trackFactory.BuildTrackInstance(f)).ToList();
            return tracksToReturn;
        }

        public List<ITrack> GetTracksForPlaylist(Playlist playlist)
        {
            var tracks = Enumerable.Empty<ITrack>();
            var nonLibraryFiles = new List<string>();

            foreach (var file in playlist.Files)
            {
                var foundTracks = GetTracksByAttributeValue(SearchAreas.Filename, file);

                if (foundTracks.Count > 0)
                {
                    tracks = tracks.Concat(foundTracks);
                }
                else
                {
                    nonLibraryFiles.Add(file);
                }
            }

            tracks = tracks.Concat(nonLibraryFiles.Select(file =>
            {
                try
                {
                    return _trackFactory.BuildTrackInstance(file);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error while loading track from file '{0}' for playlist {1}", file, ToString()), ex);
                }
            }));

            return tracks.ToList();
        }

        /// <summary>
        /// Fetch track objects from a playlist of files.
        /// </summary>
        /// <param name="playlistFile">The playlist to load files from.</param>
        /// <returns>A list of tracks </returns>
        /// <exception cref="Exception">If the directory lookup operation fails.</exception>
        public Playlist GetPlaylistFromFile(string playlistFile)
        {
            if (!File.Exists(playlistFile))
            {
                throw new FileNotFoundException(string.Format("The playlist file '{0}' does not exist on this system", playlistFile));
            }

            var playlistFileReader = new StreamReader(playlistFile);
            var jsonTracks = playlistFileReader.ReadToEnd();

            var files = JsonConvert.DeserializeObject<List<string>>(jsonTracks);

            var playlist = new Playlist() { Id = -1, FilenamesCsv = string.Join(",", files) };
            return playlist;
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
            var albumsWithMetadata = AddFilesToDatabaseModel(filesWithMetadata, concurrencyLimit, progressHandler, numFilesToAdd);
            var filesAdded = albumsWithMetadata.Count;

            if (!albumsWithMetadata.Any())
            {
                logger.WarnFormat("No new supported files were found in directory '{0}'", directory);
                Task.Run(() => completeHandler(this, filesAdded));
                return;
            }

            SaveDbChanges();
            RefreshCaches();

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
                var metadataTuple = new Tuple<string, IAudioFileMetadata>(f, AudioFileMetadata.BuildAudioFileMetaData(f));

                Task.Run(() => progressHandler?.Invoke(this, new AudioFileImportedEventArgs
                {
                    JustProcessing = true,
                    FileAdded = f,
                    TotalFilesThisImport = numFilesToAdd
                }));

                return metadataTuple;
            }).ToList();
        }

        private List<Tuple<Album, IAudioFileMetadata>> AddFilesToDatabaseModel(List<Tuple<string, IAudioFileMetadata>> filesWithMetadata, 
            int concurrencyLimit, Action<object, AudioFileImportedEventArgs> progressHandler, int numFilesToAdd)
        {
            // For each set of 5 files, create a new thread
            // which processes the metadata of those 
            return filesWithMetadata.AsParallel().WithDegreeOfParallelism(concurrencyLimit).Select(fileWithMetdata =>
            {
                try
                {
                    var song = AddFile(fileWithMetdata.Item1, fileWithMetdata.Item2).Result;

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
                .GroupBy(at => at.Item1.Id)
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

            Task.Run(() => completeHandler(this, filesAdded));
        }

        public async Task<Song> AddFile(string filename, IAudioFileMetadata metadata = null)
        {
            try
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
                    metadata = AudioFileMetadata.BuildAudioFileMetaData(filename);
                }

                var artist = await GetArtistForTag(metadata);
                var album = await GetAlbumForTag(metadata);
                var newSong = await BuildSongFromMetadata(filename, metadata, artist, album);

                return newSong;
            }
            catch (Exception ex)
            {
                EnsureDbLockReleased();
                throw ex;
            }
        }

        private void EnsureDbLockReleased()
        {
            try
            {
                _dbContextMutex.Release();
            }
            catch
            {
                //
            }
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
                    Task<Song> task = null;

                    try
                    {
                        task = AddFile(f);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(string.Format("Error adding file from ('{0}') playlist to the database", f), ex);
                    }

                    return task;
                }).ToArray();

                Task.WaitAll(addFilesTasks);

                var filesAdded = addFilesTasks.Select(t => t.Result).Where(s => s != null).Select(s => _trackFactory.BuildTrackInstance(s)).ToList();
                return filesAdded;
            });
        }
        
        private async Task<Artist> GetArtistForTag(IAudioFileMetadata tag)
        {
            if(string.IsNullOrWhiteSpace(tag.Artist))
            {
                return null;
            }

            await _dbContextMutex.WaitAsync();
            var existingArtist = OrderedArtists.FirstOrDefault(a => a.Name.Equals(tag.Artist, StringComparison.CurrentCulture));

            if (existingArtist != null)
            {
                _dbContextMutex.Release();

                logger.WarnFormat("Not adding artist with name '{0}' as an artist with the same name already exists in the database", tag.Artist);
                return existingArtist;
            }

            var newArtist = new Artist() { Name = tag.Artist };
            
            logger.DebugFormat("New artist: {0}", newArtist.Name);
            
            _dukeboxData.Artists.Add(newArtist);
            _allArtistsCache.Add(newArtist);
            _dbContextMutex.Release();

            return newArtist;
        }
        
        private async Task<Album> GetAlbumForTag(IAudioFileMetadata tag)
        {
            if(string.IsNullOrWhiteSpace(tag.Album))
            {
                return null;
            }

            await _dbContextMutex.WaitAsync();
            var existingAlbum = OrderedAlbums.FirstOrDefault(a => a.Name.Equals(tag.Album, StringComparison.CurrentCulture));

            if (existingAlbum != null)
            {
                _dbContextMutex.Release();

                logger.WarnFormat("Not adding album with name '{0}' as another album with the same name already exists in the database", tag.Album);
                return existingAlbum;
            }

            var newAlbum = new Album() { Name = tag.Album, hasAlbumArt = tag.HasAlbumArt ? 1 : 0 };
            
            logger.DebugFormat("New album: {0}", newAlbum.Name);
            
            _dukeboxData.Albums.Add(newAlbum);
            _allAlbumsCache.Add(newAlbum);
            _dbContextMutex.Release();
            
            return newAlbum;
        }
        
        private async Task<Song> BuildSongFromMetadata(string filename, IAudioFileMetadata metadata, Artist artistObj, Album albumObj)
        {
            await _dbContextMutex.WaitAsync();
            var existingSongFile = _allFilesCache.FirstOrDefault(f => f.Equals(filename, StringComparison.CurrentCulture));

            if (existingSongFile != null)
            {
                var existingSong = _dukeboxData.Songs.FirstOrDefault(s => s.FileName.Equals(existingSongFile, StringComparison.CurrentCulture));
                _dbContextMutex.Release();

                logger.WarnFormat("Not adding song from file '{0}' as a song with that filename already exists in the database", filename);
                return existingSong;
            }

            var newSong = new Song() { FileName = filename, Title = metadata.Title, Album = albumObj, Artist = artistObj };
            
            logger.DebugFormat("Title for file '{0}': {1}", newSong.FileName, newSong.Title);
            
            if(string.IsNullOrEmpty(newSong.Title))
            {
                var errMsg = string.Format("Title for file '{0}' is null or empty", filename);

                logger.Error(errMsg);
                throw new InvalidDataException(errMsg);
            }
                    
            _dukeboxData.Songs.Add(newSong);
            _allFilesCache.Add(filename);
            _dbContextMutex.Release();

            logger.InfoFormat("New song: {0}.", filename);

            return newSong;
        }

        public async Task<Playlist> AddPlaylist(string name, IEnumerable<string> filenames)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            await _dbContextMutex.WaitAsync();
            var existingPlaylist = _dukeboxData.Playlists.FirstOrDefault(a => a.Name.Equals(name, StringComparison.CurrentCulture));

            if (existingPlaylist != null)
            {
                _dbContextMutex.Release();

                logger.WarnFormat("Not adding playlist with name '{0}' as another playlist with the same name already exists in the database", name);
                return existingPlaylist;
            }

            if (filenames == null)
            {
                throw new ArgumentNullException("filenames");
            }

            var stopwatch = Stopwatch.StartNew();
            var newPlaylist = new Playlist() { Name = name, FilenamesCsv = string.Join(",", filenames) };
            
            try
            {
                _dukeboxData.Playlists.Add(newPlaylist);
                await _dukeboxData.SaveChangesAsync();

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
                logger.ErrorFormat("Error adding playlist '{0}' to database due to entity validation errors", newPlaylist.Name);
                _dukeboxData.LogEntityValidationException(ex);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error adding playlist '{0}' to database", newPlaylist.Name), ex);
            }
            finally
            {
                _dbContextMutex.Release();
            }

            return null;
        }

        private async void SaveDbChanges()
        {
            try
            {
                await _dbContextMutex.WaitAsync();
                await _dukeboxData.SaveChangesAsync();
            }
            catch (DbEntityValidationException ex)
            {
                logger.Error("Error updating database due to entity validation errors", ex);
                _dukeboxData.LogEntityValidationException(ex);
            }
            catch (Exception ex)
            {
                logger.Error("Error updating database", ex);
            }
            finally
            {
                _dbContextMutex.Release();
            }
        }

        public async Task RemoveTrack (ITrack track)
        {
            if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            await _dbContextMutex.WaitAsync();
            _dukeboxData.Songs.Remove(track.Song);
            await _dukeboxData.SaveChangesAsync();
            _dbContextMutex.Release();
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
            
            _dbContextMutex.Wait();
            _allFilesCache = _dukeboxData.Songs.Select(s => s.FileName).ToList();
            _dbContextMutex.Release();

            stopwatch.Stop();
            logger.Info("Music library artist and album caches refreshed! (This happens after a DB update routine)");
            logger.DebugFormat("Refreshing library artist and album caches took {0}ms.", stopwatch.ElapsedMilliseconds);

            Task.Run(() => CachesRefreshed?.Invoke(this, EventArgs.Empty));
        }

        #endregion

        #region Library Lookups
        
        public List<ITrack> GetTracksForArtist(Artist artist)
        {
            return artist?.Songs?.Select(s => _trackFactory.BuildTrackInstance(s)).ToList() ?? new List<ITrack>();
        }

        public List<ITrack> GetTracksForAlbum(Album album)
        {
            return album?.Songs?.Select(s => _trackFactory.BuildTrackInstance(s)).ToList() ?? new List<ITrack>();
        }

        public List<ITrack> GetTracksForArtist(long artistId)
        {
            var artist = OrderedArtists.FirstOrDefault(a => a.Id == artistId);

            if (artist == null)
            {
                throw new Exception(string.Format("Artist with ID {0} cannot be found in the database", artistId));
            }

            return GetTracksForArtist(artist);
        }

        public List<ITrack> GetTracksForAlbum(long albumId)
        {
            var album = OrderedAlbums.First(a => a.Id == albumId);

            if (album == null)
            {
                throw new Exception(string.Format("Album with ID {0} cannot be found in the database", albumId));
            }

            return GetTracksForAlbum(album);
        }

        public Artist GetArtistById(long? artistId)
        {
            var stopwatch = Stopwatch.StartNew();
            var artist = _dukeboxData.Artists.Where(a => a.Id == artistId).FirstOrDefault();

            stopwatch.Stop();

            if (artist == null)
            {
                throw new Exception(string.Format("No artist with the id '{0}' was found in the database.", artistId));
            }

            logger.DebugFormat("Look up for artist with id '{0}' took {1}ms.", artistId, stopwatch.ElapsedMilliseconds);
            return artist;
        }

        public int GetArtistCount()
        {
            return _dukeboxData.Artists.Count();
        }

        public Album GetAlbumById(long? albumId)
        {
            var stopwatch = Stopwatch.StartNew();
            var album = _dukeboxData.Albums.Where(a => a.Id == albumId).FirstOrDefault();

            stopwatch.Stop();

            if (album == null)
            {
                throw new Exception(string.Format("No album with the id '{0}' was found in the database.", albumId));
            }

            logger.DebugFormat("Look up for album with id '{0}' took {1}ms.", albumId, stopwatch.ElapsedMilliseconds);
            return album;
        }

        public int GetAlbumCount()
        {
            return _dukeboxData.Albums.Count();
        }

        public Playlist GetPlaylistById(long? playlistId)
        {
            var stopwatch = Stopwatch.StartNew();
            var playlist = _dukeboxData.Playlists.Where(p => p.Id == playlistId).FirstOrDefault();

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
            return _dukeboxData.Playlists.Count();
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
        public List<ITrack> GetTracksByAttributeId(SearchAreas attribute, long attributeId)
        {
            return _searchService.GetTracksByAttributeId(attribute, attributeId);
        }

        #endregion
    }
}
