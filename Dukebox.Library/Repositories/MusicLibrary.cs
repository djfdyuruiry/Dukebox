using Dukebox.Audio;
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
        
        /// <summary>
        /// The ADO entities model for the SQL music database.
        /// </summary>
        private IMusicLibraryDbContext _dukeboxData;
        private IDukeboxSettings _settings;
        private AudioFileFormats _audioFormats;
        private IAlbumArtCacheService _albumArtCache;
        private IMusicLibrarySearchService _searchService;
        
        private SemaphoreSlim _addFileMutex;
        private SemaphoreSlim _addPlaylistMutex;

        private List<Artist> _allArtistsCache;
        private List<Album> _allAlbumsCache;
        private List<Playlist> _allPlaylistsCache;

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
                    _allArtistsCache = _dukeboxData.Artists.OrderBy(a => a.name).ToList();

                    if (ArtistCacheRefreshed != null)
                    {
                        ArtistCacheRefreshed(this, EventArgs.Empty);
                    }
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
                    _allAlbumsCache = _dukeboxData.Albums.OrderBy(a => a.name).ToList();

                    if (AlbumCacheRefreshed != null)
                    {
                        AlbumCacheRefreshed(this, EventArgs.Empty);
                    }
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
                    _allPlaylistsCache = _dukeboxData.Playlists.OrderBy(a => a.name).ToList();

                    if (PlaylistCacheRefreshed != null)
                    {
                        PlaylistCacheRefreshed(this, EventArgs.Empty);
                    }
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
            _dukeboxData = libraryDbContext;

            _settings = settings;
            _audioFormats = audioFormats;
            _albumArtCache = albumArtCache;

            _searchService = new MusicLibrarySearchService(libraryDbContext, this);

            RecentlyPlayed = new ObservableCollection<ITrack>();
            RecentlyPlayed.CollectionChanged += RecentlyPlayedChangedHander;
            
            _addFileMutex = new SemaphoreSlim(1, 1);
            _addPlaylistMutex = new SemaphoreSlim(1, 1);
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
            validFiles = validFiles.Except(_dukeboxData.Songs.Select((s) => s.filename)).ToList();

            var tracksToReturn = validFiles.Select((f) => Track.BuildTrackInstance(f)).ToList();
            return tracksToReturn;
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

            var playlist = new Playlist() { id = -1, filenamesCsv = string.Join(",", files) };
            return playlist;
        }

        #endregion

        #region Database update methods
        
        public async Task<List<ITrack>> AddSupportedFilesInDirectory(string directory, bool subDirectories, Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler)
        {
            return await Task.Run(() => DoAddSupportedFilesInDirectory(directory, subDirectories, progressHandler, completeHandler));
        }

        private List<ITrack> DoAddSupportedFilesInDirectory(string directory, bool subDirectories, Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(string.Format("Unable add files from directory '{0}' as it does not exist on this system", directory));
            }

            var stopwatch = Stopwatch.StartNew();

            var allfiles = Directory.GetFiles(@directory, "*.*", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            var filesToAdd = allfiles.Where(_audioFormats.FileSupported);
            var numFilesToAdd = filesToAdd.Count();

            var concurrencyLimit = _settings.AddDirectoryConcurrencyLimit;

            // For each set of 5 files, create a new thread
            // which processes the metadata of those 
            //filesToAdd.AsParallel().WithDegreeOfParallelism(concurrencyLimit).ForAll(file =>
            var addFileTasks = filesToAdd.Select(file => Task.Run(async () =>
            {
                try
                {
                    var newTrack = await AddFile(file);

                    progressHandler?.Invoke(this, new AudioFileImportedEventArgs()
                    {
                        JustProcessing = false,
                        FileAdded = file,
                        TotalFilesThisImport = numFilesToAdd
                    });

                    return newTrack;
                }
                catch (Exception ex)
                {
                    logger.Error("Error adding audio file to the database while adding files from directory", ex);
                    return null;
                }
            })).ToArray();

            Task.WaitAll(addFileTasks);

            var newTracks = addFileTasks.Select(t => t.Result).Where(t => t != null).ToList<ITrack>();

            var filesAdded = newTracks.Count;

            RefreshCaches();
            completeHandler?.Invoke(this, numFilesToAdd);

            if (filesAdded < 1)
            {
                logger.WarnFormat("No new supported files were found in directory '{0}'", directory);
                return new List<ITrack>();
            }

            stopwatch.Stop();
            logger.InfoFormat("Added {0} tracks from directory: {1}", filesAdded, directory);
            logger.DebugFormat("Adding {0} tracks to library from a directory took {1}ms. Directory path: {2} (Sub-directories searched: {3})",
                filesAdded, stopwatch.ElapsedMilliseconds, directory, subDirectories);

            if (numFilesToAdd > filesAdded)
            {
                logger.WarnFormat("Not all files found in directory '{0}' were added to the database [{1}/{2} files added]",
                    directory, filesAdded, numFilesToAdd);
            }

            return newTracks;
        }
        
        public async Task<ITrack> AddFile(string filename, IAudioFileMetadata metadata = null)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(string.Format("Could not add file '{0}' as it is was not found on this system", filename));
            }

            if(!_audioFormats.FileSupported(filename))
            {
                throw new FileFormatException(string.Format("Error adding file '{0}': this format is not supported", filename));
            }
            
            if (metadata == null)
            {
                metadata = AudioFileMetadata.BuildAudioFileMetaData(filename);
            }

            await _addFileMutex.WaitAsync();

            var artist = await AddArtist(metadata);
            var album = await AddAlbum(metadata);           
            var newSong = await AddSong(filename, metadata, artist, album);

            _addFileMutex.Release();

            return newSong != null ? Track.BuildTrackInstance(newSong) : null;
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
                var addFilesTasks = playlist.Files.Select(f => AddFile(f)).ToArray();

                Task.WaitAll(addFilesTasks);

                var filesAdded = addFilesTasks.Select(t => t.Result).Where(t => t != null).ToList();
                return filesAdded;
            });
        }

        /// <summary>
        /// Add an artist to the library and save changes
        /// to database.
        /// </summary>
        private async Task<Artist> AddArtist(IAudioFileMetadata tag)
        {
            var stopwatch = Stopwatch.StartNew();

            if(string.IsNullOrWhiteSpace(tag.Artist))
            {
                return null;
            }

            var existingArtist = _dukeboxData.Artists.FirstOrDefault(a => a.name.Equals(tag.Artist, StringComparison.CurrentCulture));

            if (existingArtist != null)
            {
                logger.WarnFormat("Not adding artist with name '{0}' as an artist with the same name already exists in the database", tag.Artist);
                return existingArtist;
            }

            var newArtist = new Artist() { id = _dukeboxData.Artists.Count(), name = tag.Artist };

            try
            {
                logger.DebugFormat("Adding artist: {0}", newArtist.name);

                _dukeboxData.Artists.Add(newArtist);
                await _dukeboxData.SaveChangesAsync();

                // Invalidate artist cache.
                _allArtistsCache = null;
                    
                stopwatch.Stop();
                logger.InfoFormat("Added artist to library: {0}", newArtist.name);
                logger.DebugFormat("Adding artist to library took {0}ms. Artist id: {1}", stopwatch.ElapsedMilliseconds, newArtist.id);

                ArtistAdded?.Invoke(this, EventArgs.Empty);
                
                return newArtist;
            }
            catch (DbEntityValidationException ex)
            {
                logger.ErrorFormat("Error adding artist '{0}' to database due to entity validation errors", newArtist.name);
                _dukeboxData.LogEntityValidationException(ex);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error adding artist '{0}' to database", newArtist.name), ex);
            }

            return null;            
        }

        /// <summary>
        /// Add an album to the library and save changes
        /// to database.
        /// </summary>
        private async Task<Album> AddAlbum(IAudioFileMetadata tag)
        {
            var stopwatch = Stopwatch.StartNew();

            if(string.IsNullOrWhiteSpace(tag.Album))
            {
                return null;
            }

            var existingAlbum = _dukeboxData.Albums.FirstOrDefault(a => a.name.Equals(tag.Album, StringComparison.CurrentCulture));

            if (existingAlbum != null)
            {
                logger.WarnFormat("Not adding album with name '{0}' as another album with the same name already exists in the database", tag.Album);
                return existingAlbum;
            }

            var newAlbum = new Album() { id = _dukeboxData.Albums.Count(), name = tag.Album, hasAlbumArt = tag.HasAlbumArt ? 1 : 0 };

            try
            {
                logger.DebugFormat("Adding album: {0}", newAlbum.name);

                _dukeboxData.Albums.Add(newAlbum);
                await _dukeboxData.SaveChangesAsync();

                // Invalidate album cache.
                _allAlbumsCache = null;

                stopwatch.Stop();
                logger.InfoFormat("Added album to library: {0}", newAlbum.name);
                logger.DebugFormat("Adding album to library took {0}ms. Album id: {1}", stopwatch.ElapsedMilliseconds, newAlbum.id);

                _albumArtCache.AddAlbumToCache(newAlbum, tag);

                AlbumAdded?.Invoke(this, EventArgs.Empty);

                return newAlbum;
            }
            catch (DbEntityValidationException ex)
            {
                logger.ErrorFormat("Error adding album '{0}' to database due to entity validation errors", newAlbum.name);
                _dukeboxData.LogEntityValidationException(ex);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error adding album '{0}' to database", newAlbum.name), ex);
            }

            return null;
        }

        /// <summary>
        /// Add a song to the library and save changes
        /// to database.
        /// </summary>
        private async Task<Song> AddSong(string filename, IAudioFileMetadata metadata, Artist artistObj, Album albumObj)
        {
            var stopwatch = Stopwatch.StartNew();
            Song newSong = null;

            var existingSong = _dukeboxData.Songs.FirstOrDefault(a => a.filename.Equals(filename, StringComparison.CurrentCulture));

            if (existingSong != null)
            {
                logger.WarnFormat("Not adding song from file '{0}' as a song with that filename already exists in the database", filename);
                return existingSong;
            }

            // Build new song with all available information.
            if (albumObj != null && artistObj != null)
            {
                newSong = new Song() { filename = filename, title = metadata.Title, album = albumObj, artist = artistObj };
            }
            else if (albumObj != null && artistObj == null)
            {
                newSong = new Song() { filename = filename, title = metadata.Title, album = albumObj };
            }
            else if (albumObj == null && artistObj != null)
            {
                newSong = new Song() { filename = filename, title = metadata.Title, artist = artistObj };
            }
            else if (albumObj == null && artistObj == null)
            {
                newSong = new Song() { filename = filename, title = metadata.Title };
            }

            try
            {
                logger.DebugFormat("Title for file '{0}': {1}", newSong.filename, newSong.title);

                _dukeboxData.Songs.Add(newSong);
                await _dukeboxData.SaveChangesAsync();

                stopwatch.Stop();
                logger.InfoFormat("Added song with id {0} to library.", newSong.id);
                logger.DebugFormat("Adding song to library took {0}ms. Song id: {1}", stopwatch.ElapsedMilliseconds, newSong.id);
                
                SongAdded?.Invoke(this, EventArgs.Empty);

                return newSong;
            }
            catch (DbEntityValidationException ex)
            {
                logger.ErrorFormat("Error adding song from file '{0}' to database due to entity validation errors", newSong.filename);
                _dukeboxData.LogEntityValidationException(ex);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error adding song from file '{0}' to database", newSong.filename), ex);
            }

            return null;
        }

        public async Task<Playlist> AddPlaylist(string name, IEnumerable<string> filenames)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            
            var existingPlaylist = _dukeboxData.Playlists.FirstOrDefault(a => a.name.Equals(name, StringComparison.CurrentCulture));

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
            var newPlaylist = new Playlist() { name = name, filenamesCsv = string.Join(",", filenames) };

            await _addPlaylistMutex.WaitAsync();

            try
            {
                _dukeboxData.Playlists.Add(newPlaylist);
                await _dukeboxData.SaveChangesAsync();

                // Invalidate playlist cache.
                _allPlaylistsCache = null;

                stopwatch.Stop();
                logger.InfoFormat("Added playlist to library: {0}", newPlaylist.name);
                logger.DebugFormat("Adding playlist to library took {0}ms. Playlist id: {1}", stopwatch.ElapsedMilliseconds, newPlaylist.id);

                PlaylistAdded?.Invoke(this.PlaylistAdded, EventArgs.Empty);

                return newPlaylist;
            }
            catch (DbEntityValidationException ex)
            {
                logger.ErrorFormat("Error adding playlist '{0}' to database due to entity validation errors", newPlaylist.name);
                _dukeboxData.LogEntityValidationException(ex);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error adding playlist '{0}' to database", newPlaylist.name), ex);
            }
            finally
            {
                _addPlaylistMutex.Release();
            }

            return null;
        }

        public async Task RemoveTrack (ITrack track)
        {
            if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            _dukeboxData.Songs.Remove(track.Song);
            await _dukeboxData.SaveChangesAsync();
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

            var albums = OrderedAlbums;
            var artists = OrderedArtists;
            var playlists = OrderedPlaylists;

            stopwatch.Stop();
            logger.Info("Music library artist and album caches refreshed! (This happens after a DB update routine)");
            logger.DebugFormat("Refreshing library artist and album caches took {0}ms.", stopwatch.ElapsedMilliseconds);

            CachesRefreshed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Library Lookups
        
        public List<ITrack> GetTracksForArtist(Artist artist)
        {
            return artist.songs.Select(Track.BuildTrackInstance).ToList();
        }

        public List<ITrack> GetTracksForAlbum(Album album)
        {
            return album.songs.Select(Track.BuildTrackInstance).ToList();
        }

        public Artist GetArtistById(long? artistId)
        {
            var stopwatch = Stopwatch.StartNew();
            var artist = _dukeboxData.Artists.Where(a => a.id == artistId).FirstOrDefault();

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
            var album = _dukeboxData.Albums.Where(a => a.id == albumId).FirstOrDefault();

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
            var playlist = _dukeboxData.Playlists.Where(p => p.id == playlistId).FirstOrDefault();

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
