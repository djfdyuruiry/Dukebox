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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

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

        private Mutex _addAlbumMutex;
        private Mutex _addArtistMutex;

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
        
        public MusicLibrary(IMusicLibraryDbContext libraryDbContext, IDukeboxSettings settings, IAlbumArtCacheService albumArtCache, AudioFileFormats audioFormats)
        {
            _dukeboxData = libraryDbContext;

            _settings = settings;
            _audioFormats = audioFormats;
            _albumArtCache = albumArtCache;

            RecentlyPlayed = new ObservableCollection<ITrack>();
            RecentlyPlayed.CollectionChanged += RecentlyPlayedChangedHander;

            _addAlbumMutex = new Mutex();
            _addArtistMutex = new Mutex();
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

        /// <summary>
        /// Add all files specified in a directory to the library and save changes
        /// to database.
        /// </summary>
        /// <param name="directory">The path to the directory to ingest.</param>
        /// <param name="subDirectories">Scan all sub directories of the path?</param>
        /// <exception cref="Exception">If the library import operation fails.</exception>
        public void AddDirectory(string directory, bool subDirectories, Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(string.Format("Unable add files from directory '{0}' as it does not exist on this system", directory));
            }

            var stopwatch = Stopwatch.StartNew();
            var filesToAddMutex = new Mutex();
        
            var allfiles = Directory.GetFiles(@directory, "*.*", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            var supportedFiles = allfiles.Where(f => _audioFormats.SupportedFormats.Contains(GetFileExtension(f)));
            var filesToAdd = supportedFiles.Where(sf => !_dukeboxData.Songs.Any((s) => s.filename.Equals(sf, StringComparison.CurrentCulture)));

            var numFilesToAdd = filesToAdd.Count();
            var filesAdded = 0;
            var concurrencyLimit = _settings.AddDirectoryConcurrencyLimit;

            // For each set of 5 files, create a new thread
            // which processes the metadata of those 
            //filesToAdd.AsParallel().WithDegreeOfParallelism(concurrencyLimit).ForAll(file =>
            filesToAdd.ToList().ForEach(file =>
            {
                try
                {
                    var metadata = AudioFileMetadata.BuildAudioFileMetaData(file);
                    AddFile(file, metadata);

                    if (progressHandler != null)
                    {
                        progressHandler.Invoke(this, new AudioFileImportedEventArgs() { JustProcessing = false, FileAdded = file, TotalFilesThisImport = numFilesToAdd });
                    }

                    filesAdded++;
                }
                catch (Exception ex)
                {
                    logger.Error("Error adding audio file to the database while adding files from directory", ex);
                }
            });
          
            RefreshCaches();

            if (completeHandler != null)
            {
                completeHandler.Invoke(this, numFilesToAdd);
            }

            if (numFilesToAdd < 1)
            {
                logger.WarnFormat("No new files were found in directory '{0}'", directory);
                return;
            }

            stopwatch.Stop();
            logger.InfoFormat("Added {0} tracks from directory: {1}", filesAdded, directory);
            logger.DebugFormat("Adding {0} tracks to library from a directory took {1}ms. Directory path: {2} (Sub-directories searched: {3})",
                filesAdded, stopwatch.ElapsedMilliseconds, directory, subDirectories);

            if (numFilesToAdd > filesAdded)
            {
                logger.WarnFormat("Not all files found in directory '{0}' were added to the database [{1}/{2} files added]", directory, filesAdded, numFilesToAdd); 
            }
        }

        /// <summary>
        /// Add a file to the library and save changes
        /// to database.
        /// </summary>
        /// <param name="kvp"></param>
        public void AddFile(string filename, IAudioFileMetadata metadata)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(string.Format("Could not add file '{0}' as it is was not found on this system", filename));
            } 
            
            if (_dukeboxData.Songs.Any(a => a.filename.Equals(filename, StringComparison.CurrentCulture)))
            {
                logger.WarnFormat("Not adding song from file '{0}' as a song with that filename already exists in the database", filename);
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            _addArtistMutex.WaitOne();

            // Find artist and album information from the library.
            Artist artistObj = _dukeboxData.Artists.Where(a => a.name.Equals(metadata.Artist, StringComparison.CurrentCulture)).FirstOrDefault();

            // Insert artist/album information if missing from library.
            if (artistObj == null && !string.IsNullOrWhiteSpace(metadata.Artist))
            {
                AddArtist(metadata);
                artistObj = _dukeboxData.Artists.Where(a => a.name.Equals(metadata.Artist, StringComparison.CurrentCulture)).FirstOrDefault();
            }

            _addArtistMutex.ReleaseMutex();

            _addAlbumMutex.WaitOne();

            Album albumObj = _dukeboxData.Albums.Where(a => a.name.Equals(metadata.Album, StringComparison.CurrentCulture)).FirstOrDefault();

            if (albumObj == null && !string.IsNullOrWhiteSpace(metadata.Album))
            {
                AddAlbum(metadata);
                albumObj = _dukeboxData.Albums.Where(a => a.name.Equals(metadata.Album, StringComparison.CurrentCulture)).FirstOrDefault();
            }

            _addAlbumMutex.ReleaseMutex();

            if (metadata.IsEmpty)
            {
                logger.WarnFormat("Not adding song from filename '{0}' as no metadata could be extracted", filename);
                return;
            }

            AddSong(filename, metadata, artistObj, albumObj);
                
            stopwatch.Stop();
            logger.InfoFormat("Added file to library: {0}", filename);
            logger.DebugFormat("Adding file to library took {0}ms. File path: {1}", stopwatch.ElapsedMilliseconds, filename);  
        }

        /// <summary>
        /// Add all files specified in a playlist to the library and save changes
        /// to database.
        /// </summary>
        /// <param name="filename"></param>
        public void AddPlaylistFile(string filename)
        {
            var stopwatch = Stopwatch.StartNew();

            var playlist = GetPlaylistFromFile(filename);
            var playlistTracks = playlist.GetTracksForPlaylist();
            var libraryFilenames = _dukeboxData.Songs.Select(s => s.filename).Distinct();

            var tracksToAdd = playlistTracks.Where(t => !libraryFilenames.Contains(t.Song.filename));

            foreach (var track in tracksToAdd)
            {
                AddFile(track.Song.filename, track.Metadata);
            }

            if (tracksToAdd.Count() > 0)
            {
                stopwatch.Stop();
                logger.InfoFormat("Added {0} from playlist file: {1}", tracksToAdd.Count(), filename);
                logger.DebugFormat("Adding {0} tracks to library from a playlist took {1}ms. Playlist path: {2}",
                    tracksToAdd.Count(), stopwatch.ElapsedMilliseconds, filename);
            }
        }

        /// <summary>
        /// Add an artist to the library and save changes
        /// to database.
        /// </summary>
        private void AddArtist(IAudioFileMetadata tag)
        {
            var stopwatch = Stopwatch.StartNew();

            if(tag.Artist == null)
            {
                return;
            }

            if (_dukeboxData.Artists.Any(a => a.name.Equals(tag.Artist, StringComparison.CurrentCulture)))
            {
                logger.WarnFormat("Not adding artist with name '{0}' as an artist with the same name already exists in the database", tag.Artist);
                return;
            }

            var newArtist = new Artist() { id = _dukeboxData.Artists.Count(), name = tag.Artist };

            try
            {
                _dukeboxData.Artists.Add(newArtist);
                _dukeboxData.SaveChanges();

                // Invalidate artist cache.
                _allArtistsCache = null;
                    
                stopwatch.Stop();
                logger.InfoFormat("Added artist to library: {0}", newArtist.name);
                logger.DebugFormat("Adding artist to library took {0}ms. Artist name: {1}", stopwatch.ElapsedMilliseconds, newArtist.name);

                if (ArtistAdded != null)
                {
                    ArtistAdded(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error adding artist '{0}' to database", newArtist.name), ex);
            }
}

        /// <summary>
        /// Add an album to the library and save changes
        /// to database.
        /// </summary>
        private void AddAlbum(IAudioFileMetadata tag)
        {
            var stopwatch = Stopwatch.StartNew();

            if(tag.Album == null)
            {
                return;
            }

            if (_dukeboxData.Albums.Any(a => a.name.Equals(tag.Album, StringComparison.CurrentCulture)))
            {
                logger.WarnFormat("Not adding album with name '{0}' as another album with the same name already exists in the database", tag.Album);
            }

            var newAlbum = new Album() { id = _dukeboxData.Albums.Count(), name = tag.Album, hasAlbumArt = tag.HasAlbumArt ? 1 : 0 };

            try
            {
                _dukeboxData.Albums.Add(newAlbum);
                _dukeboxData.SaveChanges();

                // Invalidate album cache.
                _allAlbumsCache = null;

                stopwatch.Stop();
                logger.InfoFormat("Added album to library: {0}", newAlbum.name);
                logger.DebugFormat("Adding album to library took {0}ms. Album name: {1}", stopwatch.ElapsedMilliseconds, newAlbum.name);

                if (AlbumAdded != null)
                {
                    AlbumAdded(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error adding album '{0}' to database", newAlbum.name), ex);
            }
        }

        /// <summary>
        /// Add a song to the library and save changes
        /// to database.
        /// </summary>
        private void AddSong(string filename, IAudioFileMetadata metadata, Artist artistObj, Album albumObj)
        {
            var stopwatch = Stopwatch.StartNew();
            Song newSong = null;
            
            // Build new song with all available information.
            if (albumObj != null && artistObj != null)
            {
                newSong = new Song() { filename = filename, title = metadata.Title, albumId = albumObj.id, artistId = artistObj.id };
            }
            else if (albumObj != null && artistObj == null)
            {
                newSong = new Song() { filename = filename, title = metadata.Title, albumId = albumObj.id, artistId = null };
            }
            else if (albumObj == null && artistObj != null)
            {
                newSong = new Song() { filename = filename, title = metadata.Title, albumId = null, artistId = artistObj.id };
            }
            else if (albumObj == null && artistObj == null)
            {
                newSong = new Song() { filename = filename, title = metadata.Title, albumId = null, artistId = null };
            }

            try
            {
                _dukeboxData.Songs.Add(newSong);
                _dukeboxData.SaveChanges();

                stopwatch.Stop();
                logger.InfoFormat("Added song with id {0} to library.", newSong.id);
                logger.DebugFormat("Adding song to library took {0}ms. Song id: {1}", stopwatch.ElapsedMilliseconds, newSong.id);

                _albumArtCache.AddSongToCache(newSong, metadata, albumObj);

                if (SongAdded != null)
                {
                    SongAdded(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error adding song from file '{0}' to database", newSong.filename), ex);
            }
        }

        public void RemoveTrack (ITrack track)
        {
            if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            _dukeboxData.Songs.Remove(track.Song);
            _dukeboxData.SaveChanges();
        }

        public void AddPlaylist(string name, IEnumerable<string> filenames)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (filenames == null)
            {
                throw new ArgumentNullException("filenames");
            }

            var stopwatch = Stopwatch.StartNew();
            var newPlaylist = new Playlist() { name = name, filenamesCsv = string.Join(",", filenames) };
                      
            _dukeboxData.Playlists.Add(newPlaylist);
            _dukeboxData.SaveChanges();

            // Invalidate playlist cache.
            _allPlaylistsCache = null;

            stopwatch.Stop();
            logger.InfoFormat("Added playlist to library: {0}", newPlaylist.name);
            logger.DebugFormat("Adding album to library took {0}ms. Playlist name: {1}", stopwatch.ElapsedMilliseconds, newPlaylist.name);

            if (PlaylistAdded != null)
            {
                PlaylistAdded(this.PlaylistAdded, EventArgs.Empty);
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

            var albums = OrderedAlbums;
            var artists = OrderedArtists;
            var playlists = OrderedPlaylists;

            stopwatch.Stop();
            logger.Info("Music library artist and album caches refreshed! (This happens after a DB update routine)");
            logger.DebugFormat("Refreshing library artist and album caches took {0}ms.", stopwatch.ElapsedMilliseconds);

            if (CachesRefreshed != null)
            {
                CachesRefreshed(this, EventArgs.Empty);
            }
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

        /// <summary>
        /// Get all search areas and add them to a given list.
        /// </summary>
        /// <param name="searchAreas">List to add search areas to.</param>
        private void AddSearchAreasToList(List<SearchAreas> searchAreas)
        {
            foreach (var area in Enum.GetValues(typeof(SearchAreas)))
            {
                searchAreas.Add((SearchAreas)area);
            }
        }

        /// <summary>
        /// Search the library database for tracks matching
        /// the search term specified. All text returned by
        /// a call to 'ToString' on any given track object is
        /// searched for the the term provided.
        /// 
        /// This search is not case sensitive.
        /// </summary>
        /// <param name="searchTerm">The term to search for in track descriptions.</param>
        /// <returns>A list of tracks that match the given search criteria.</returns>
        public List<ITrack> SearchForTracks(string searchTerm, List<SearchAreas> searchAreas)
        {
            var stopwatch = Stopwatch.StartNew();

            var songs = _dukeboxData.Songs;
            var matchingSongs = Enumerable.Empty<Song>();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return songs.ToList().Select(Track.BuildTrackInstance).ToList();
            }
                        
            searchTerm = searchTerm.ToLower();
            searchAreas = searchAreas ?? new List<SearchAreas>();

            if (searchAreas.Contains(SearchAreas.All))
            {
                AddSearchAreasToList(searchAreas);
            }

            if (searchAreas.Contains(SearchAreas.Album))
            {
                var matchingAlbums = GetMatchingAttributeIds(SearchAreas.Album, searchTerm);
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.albumId.HasValue ? matchingAlbums.Contains(s.albumId.Value) : false));
            }

            if (searchAreas.Contains(SearchAreas.Artist))
            {
                var matchingArtists = GetMatchingAttributeIds(SearchAreas.Artist, searchTerm);
                matchingSongs = matchingSongs.Concat(songs.Where(t => t.artistId.HasValue ? matchingArtists.Contains(t.artistId.Value) : false));
            }

            if (searchAreas.Contains(SearchAreas.Song))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.title.ToLower().Contains(searchTerm)));
            }

            if (searchAreas.Contains(SearchAreas.Filename))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.filename.ToLower().Contains(searchTerm)));
            }


            stopwatch.Stop();
            var searchAreasString = searchAreas.Select(sa => Enum.GetName(typeof(SearchAreas), sa)).Aggregate((c, n) => c + ", " + n);
            logger.DebugFormat("Getting tracks by attribute(s) '{0}' where name or title contain '{1}' took {2}ms and returned {3} results.",
                searchAreasString, searchTerm, stopwatch.ElapsedMilliseconds, matchingSongs.Count());

            return matchingSongs.Count() < 1 ? new List<ITrack>() : matchingSongs.ToList().Select(Track.BuildTrackInstance).ToList();
        }

        public List<ITrack> SearchForTracksInArea(SearchAreas attribute, string nameOrTitle)
        {
            return SearchForTracks(nameOrTitle, new List<SearchAreas> { attribute });
        }

        public List<ITrack> GetTracksByAttributeValue(SearchAreas attribute, string attributeValue)
        {
            var stopwatch = Stopwatch.StartNew();

            var songs = _dukeboxData.Songs;
            var matchingSongs = Enumerable.Empty<Song>();

            var lowerAttributeValue = attributeValue.ToLower();

            if (attribute == SearchAreas.Album)
            {
                var matchingAlbums = GetMatchingAttributeIds(SearchAreas.Album, lowerAttributeValue, true);
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.albumId.HasValue ? matchingAlbums.Contains(s.albumId.Value) : false));
            }

            if (attribute == SearchAreas.Artist)
            {
                var matchingArtists = GetMatchingAttributeIds(SearchAreas.Artist, lowerAttributeValue, true);
                matchingSongs = matchingSongs.Concat(songs.Where(t => t.artistId.HasValue ? matchingArtists.Contains(t.artistId.Value) : false));
            }

            if (attribute == SearchAreas.Song)
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.title.ToLower().Equals(lowerAttributeValue)));
            }

            if (attribute == SearchAreas.Filename)
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.filename.ToLower().Equals(lowerAttributeValue)));
            }


            stopwatch.Stop();
            logger.DebugFormat("Getting tracks by attribute(s) '{0}' where name or title equal '{1}' took {2}ms and returned {3} results.",
                attribute, lowerAttributeValue, stopwatch.ElapsedMilliseconds, matchingSongs.Count());

            return matchingSongs.Count() < 1 ? new List<ITrack>() : matchingSongs.ToList().Select(Track.BuildTrackInstance).ToList();
        }

        private IEnumerable<long> GetMatchingAttributeIds(SearchAreas attribute, string searchTerm, bool exactMatch = false)
        {
            if (attribute == SearchAreas.Album)
            {
                if (exactMatch)
                {
                    return OrderedAlbums.Where(a => a.ToString().ToLower().Equals(searchTerm)).Select(a => a.id);
                }
                else
                {
                    return OrderedAlbums.Where(a => a.ToString().ToLower().Contains(searchTerm)).Select(a => a.id);
                }
            }
            else if (attribute == SearchAreas.Artist)
            {
                if (exactMatch)
                {
                    return OrderedArtists.Where(a => a.ToString().ToLower().Equals(searchTerm)).Select(a => a.id);
                }
                else
                {
                    return OrderedArtists.Where(a => a.ToString().ToLower().Contains(searchTerm)).Select(a => a.id);
                }
            }

            return Enumerable.Empty<long>();
        }

        /// <summary>
        /// Get a list of tracks who's attribute type equals the id specified. Filename 
        /// attribute is supported by this lookup method.
        /// </summary>
        /// <param name="attribute">The attribute type to select.</param>
        /// <param name="attributeId">The id of the attribute.</param>
        /// <returns>A list of tracks that match the given attribute keypair.</returns>
        public List<ITrack> GetTracksByAttributeId(SearchAreas attribute, long attributeId)
        {
            var stopwatch = Stopwatch.StartNew();

            var searchAreas = new List<SearchAreas>();
            var songs = _dukeboxData.Songs;
            var matchingSongs = Enumerable.Empty<Song>();

            if (attribute == SearchAreas.Filename)
            {
                throw new InvalidOperationException("The filename search attribute is not supported when looking up tracks by attribute id");
            }

            if (attribute == SearchAreas.All)
            {
                AddSearchAreasToList(searchAreas);
            }
            else
            {
                searchAreas.Add(attribute);
            }

            if (searchAreas.Contains(SearchAreas.Album))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.albumId != null ? s.albumId == attributeId : false));
            }

            if (searchAreas.Contains(SearchAreas.Artist))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.artistId != null ? s.artistId == attributeId : false));
            }

            if (searchAreas.Contains(SearchAreas.Song))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.id == attributeId));
            }

            stopwatch.Stop();
            logger.DebugFormat("Getting tracks by attribute {0} and value {1} took {2}ms and returned {3} results.", 
                Enum.GetName(typeof(SearchAreas), attribute), attributeId, stopwatch.ElapsedMilliseconds, matchingSongs.Count());

            return matchingSongs.Count() < 1 ? new List<ITrack>() : matchingSongs.Select(Track.BuildTrackInstance).ToList();
        }

        #endregion

        /// <summary>
        /// Convenience method to extract file extension
        /// from a filename string.
        /// </summary>
        /// <param name="fileName">The name of the file to examine.</param>
        /// <returns>The file extension of the file.</returns>
        public static string GetFileExtension(string fileName)
        {
            return (new FileInfo(fileName)).Extension.ToLower();
        }
    }
}
