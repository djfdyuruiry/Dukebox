using Dukebox.Audio;
using Dukebox.Library.Config;
using Dukebox.Library.Model;
using Dukebox.Library.Services;
using Dukebox.Model.Services;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    public class MusicLibrary
    {
        private const int defaultAddDirectoryConcurrencyLimit = 10;
        private const string addDirectoryConcurrencyLimitConfigKey = "addDirectoryConcurrencyLimit";
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MusicLibrary));
        
        /// <summary>
        /// The ADO entities model for the SQL music database.
        /// </summary>
        private Library DukeboxData { get; set; }

        private Mutex AddAlbumMutex;
        private Mutex AddArtistMutex;

        #region Views on music library data
        
        /// <summary>
        /// All artists currently stored in the database. Cached.
        /// </summary>
        private List<artist> _allArtistsCache;
        public IList<artist> OrderedArtists
        {
            get
            {
                if (_allArtistsCache == null)
                {
                    _allArtistsCache = DukeboxData.artists.OrderBy(a => a.name).ToList();
                }

                return _allArtistsCache;
            }
        }

        /// <summary>
        /// All albums currently stored in the database. Cached.
        /// </summary>
        private List<album> _allAlbumsCache;
        public IList<album> OrderedAlbums
        {
            get
            {
                if (_allAlbumsCache == null)
                {
                    _allAlbumsCache = DukeboxData.albums.OrderBy(a => a.name).ToList();
                }

                return _allAlbumsCache;
            }
        }

        public List<playlist> _allPlaylistsCache;
        public IList<playlist> OrderedPlaylists        
        {
            get
            {
                if (_allPlaylistsCache == null)
                {
                    _allPlaylistsCache = DukeboxData.playlists.OrderBy(a => a.name).ToList();
                }

                return _allPlaylistsCache;
            }
        }

        public List<Track> RecentlyPlayed { get; set; }

        #endregion
        
        /// <summary>
        /// Singleton pattern private constructor. Open connection to
        /// SQL lite database file and populate track cache
        /// </summary>
        private MusicLibrary()
        {
            DukeboxData = new Library();
            DukeboxData.Database.Connection.Open();

            RecentlyPlayed = new List<Track>();

            AddAlbumMutex = new Mutex();
            AddArtistMutex = new Mutex();
        }

        #region Folder/Playlist/File playback methods

        /// <summary>
        /// Fetch track objects from a directory of files.
        /// </summary>
        /// <param name="directory">The path to load files from.</param>
        /// <param name="subDirectories">Search all sub directories in the path given?</param>
        /// <returns>A list of tracks </returns>
        /// <exception cref="Exception">If the directory lookup operation fails.</exception>
        public List<Track> GetTracksForDirectory(string directory, bool subDirectories)
        {
            var searchOption = subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var supportedFormats = AudioFileFormats.SupportedFormats;

            var validFiles = Directory.EnumerateFiles(directory, "*.*", searchOption).Where((f) => supportedFormats.Any(sf => f.EndsWith(sf))).ToList();
            validFiles = validFiles.Except(DukeboxData.songs.Select((s) => s.filename)).ToList();

            var tracksToReturn = validFiles.Select((f) => GetTrackFromFile(f)).ToList();
            return tracksToReturn;
        }

        /// <summary>
        /// Construct a track object from a given audio file.
        /// </summary>
        /// <param name="fileName">The file to model in the track object.</param>
        /// <param name="metadata">Optional metadata object, if you wish to build this manually.</param>
        /// <returns></returns>
        public Track GetTrackFromFile(string fileName, AudioFileMetaData metadata = null)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(string.Format("The audio file '{0}' does not exist on this system", fileName));
            }

            song trackSong = new song() { id = -1, albumId = -1, artistId = -1, filename = fileName };
       
            // Find artist and album information from the library.
            artist artistObj = new artist() { id = -1 };
            album albumObj = new album() { id = -1 };

            Track track = new Track() { Album = albumObj, Artist = artistObj, Song = trackSong };

            if (metadata != null)
            {
                track.Metadata = metadata;
            }
            else
            {
                track.Metadata = new AudioFileMetaData(fileName);
            }

            track.Song.title = track.Metadata.Title;
            track.Album.name = track.Metadata.Album;
            track.Artist.name = track.Metadata.Artist;

            return track;
        }

        /// <summary>
        /// Fetch track objects from a playlist of files.
        /// </summary>
        /// <param name="playlistFile">The playlist to load files from.</param>
        /// <returns>A list of tracks </returns>
        /// <exception cref="Exception">If the directory lookup operation fails.</exception>
        public playlist GetPlaylistFromFile(string playlistFile)
        {
            if (!File.Exists(playlistFile))
            {
                throw new FileNotFoundException(string.Format("The playlist file '{0}' does not exist on this system", playlistFile));
            }

            var playlistFileReader = new StreamReader(playlistFile);
            var jsonTracks = playlistFileReader.ReadToEnd();

            var files = JsonConvert.DeserializeObject<List<string>>(jsonTracks);

            var playlist = new playlist() { id = -1, filenamesCsv = string.Join(",", files) };
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
            var tracks = new Dictionary<string, AudioFileMetaData>();
            var filesToAddMutex = new Mutex();
        
            var allfiles = Directory.GetFiles(@directory, "*.*", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            var supportedFiles = allfiles.Where(f => AudioFileFormats.SupportedFormats.Contains(GetFileExtension(f)));
            var filesToAdd = supportedFiles.Where(sf => !DukeboxData.songs.Any((s) => s.filename.Equals(sf, StringComparison.CurrentCulture)));

            var numFilesToAdd = filesToAdd.Count();
            int concurrencyLimit = defaultAddDirectoryConcurrencyLimit;
            var filesAdded = 0;

            if (DukeboxSettings.IsConfigSettingValidInt(addDirectoryConcurrencyLimitConfigKey))
            {
                concurrencyLimit = DukeboxSettings.GetSettingAsInt(addDirectoryConcurrencyLimitConfigKey);
            }
            else
            {
                _logger.WarnFormat("Unable to find or parse the value for '{0}' in the current app.config file, using default of {1}", 
                    addDirectoryConcurrencyLimitConfigKey, defaultAddDirectoryConcurrencyLimit);
            }

            // For each set of 5 files, create a new thread
            // which processes the metadata of those 
            filesToAdd.AsParallel().WithDegreeOfParallelism(concurrencyLimit).ForAll(file =>
            {
                try
                {
                    if (progressHandler != null)
                    {
                        progressHandler.Invoke(this, new AudioFileImportedEventArgs() { JustProcessing = true, FileAdded = file, TotalFilesThisImport = numFilesToAdd });
                    }

                    tracks.Add(file, new AudioFileMetaData(file));
                }
                catch (Exception ex)
                {
                    _logger.Error("Error opening/reading audio file while adding files from directory", ex);
                }
            });

            foreach (var kvp in tracks)
            {
                try
                {
                    AddFile(kvp.Key, kvp.Value);

                    if (progressHandler != null)
                    {
                        progressHandler.Invoke(this, new AudioFileImportedEventArgs() { JustProcessing = false, FileAdded = kvp.Key, TotalFilesThisImport = numFilesToAdd });
                    }

                    filesAdded++;
                }
                catch (Exception ex)
                {
                    _logger.Error("Error adding audio file to the database while adding files from directory", ex);
                }
            }
          
            ClearCaches();

            if (completeHandler != null)
            {
                completeHandler.Invoke(this, numFilesToAdd);
            }

            if (numFilesToAdd > 0)
            {
                stopwatch.Stop();
                _logger.InfoFormat("Added {0} tracks from directory: {1}", filesAdded, directory);
                _logger.DebugFormat("Adding {0} tracks to library from a directory took {1}ms. Directory path: {2} (Sub-directories searched: {3})",
                    filesAdded, stopwatch.ElapsedMilliseconds, directory, subDirectories);

                if (numFilesToAdd > filesAdded)
                {
                    _logger.WarnFormat("Not all files found in directory '{0}' were added to the database [{1}/{2} files added]", directory, filesAdded, numFilesToAdd); 
                }
            }
        }

        /// <summary>
        /// Add a file to the library and save changes
        /// to database.
        /// </summary>
        /// <param name="kvp"></param>
        public void AddFile(string filename, AudioFileMetaData metadata)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(string.Format("Could not add file '{0}' as it is was not found on this system", filename));
            } 
            
            if (DukeboxData.songs.Any(a => a.filename.Equals(filename, StringComparison.CurrentCulture)))
            {
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            AddArtistMutex.WaitOne();

            // Find artist and album information from the library.
            artist artistObj = DukeboxData.artists.Where(a => a.name.Equals(metadata.Artist, StringComparison.CurrentCulture)).FirstOrDefault();

            // Insert artist/album information if missing from library.
            if (artistObj == null && metadata.Artist != string.Empty)
            {
                AddArtist(metadata);
                artistObj = DukeboxData.artists.Where(a => a.name.Equals(metadata.Artist, StringComparison.CurrentCulture)).FirstOrDefault();
            }

            AddArtistMutex.ReleaseMutex();

            AddAlbumMutex.WaitOne();

            album albumObj = DukeboxData.albums.Where(a => a.name.Equals(metadata.Album, StringComparison.CurrentCulture)).FirstOrDefault();

            if (albumObj == null && metadata.Album != string.Empty)
            {
                AddAlbum(metadata);
                albumObj = DukeboxData.albums.Where(a => a.name.Equals(metadata.Album, StringComparison.CurrentCulture)).FirstOrDefault();
            }

            AddAlbumMutex.ReleaseMutex();

            if (metadata.Title != string.Empty || albumObj != null || artistObj != null)
            {
                AddSong(filename, metadata, artistObj, albumObj);
                
                stopwatch.Stop();
                _logger.InfoFormat("Added file to library: {0}", filename);
                _logger.DebugFormat("Adding file to library took {0}ms. File path: {1}", stopwatch.ElapsedMilliseconds, filename);
            }
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
            var libraryFilenames = DukeboxData.songs.Select(s => s.filename).Distinct();

            var tracksToAdd = playlistTracks.Where(t => !libraryFilenames.Contains(t.Song.filename));

            foreach (var track in tracksToAdd)
            {
                AddFile(track.Song.filename, track.Metadata);
            }

            if (tracksToAdd.Count() > 0)
            {
                stopwatch.Stop();
                _logger.InfoFormat("Added {0} from playlist file: {1}", tracksToAdd.Count(), filename);
                _logger.DebugFormat("Adding {0} tracks to library from a playlist took {1}ms. Playlist path: {2}",
                    tracksToAdd.Count(), stopwatch.ElapsedMilliseconds, filename);
            }
        }

        /// <summary>
        /// Add an artist to the library and save changes
        /// to database.
        /// </summary>
        private void AddArtist(AudioFileMetaData tag)
        {
            var stopwatch = Stopwatch.StartNew();

            if(tag.Artist != null)
            {
                artist newArtist = new artist() { id = DukeboxData.artists.Count(), name = tag.Artist };

                if (!DukeboxData.artists.Any(a => a.name.Equals(newArtist.name, StringComparison.CurrentCulture)))
                {
                    DukeboxData.artists.Add(newArtist);
                    DukeboxData.SaveChanges();

                    // Invalidate artist cache.
                    _allArtistsCache = null;
                    
                    stopwatch.Stop();
                    _logger.InfoFormat("Added artist to library: {0}", newArtist.name);
                    _logger.DebugFormat("Adding artist to library took {0}ms. Artist name: {1}", stopwatch.ElapsedMilliseconds, newArtist.name);
                }
            }
        }

        /// <summary>
        /// Add an album to the library and save changes
        /// to database.
        /// </summary>
        private void AddAlbum(AudioFileMetaData tag)
        {
            var stopwatch = Stopwatch.StartNew();

            if(tag.Album != null)
            {
                album newAlbum = new album() { id = DukeboxData.albums.Count(), name = tag.Album };

                if (!DukeboxData.albums.Any(a => a.name.Equals(newAlbum.name, StringComparison.CurrentCulture)))
                {
                    DukeboxData.albums.Add(newAlbum);
                    DukeboxData.SaveChanges();

                    // Invalidate album cache.
                    _allAlbumsCache = null;

                    stopwatch.Stop();
                    _logger.InfoFormat("Added album to library: {0}", newAlbum.name);
                    _logger.DebugFormat("Adding album to library took {0}ms. Album name: {1}", stopwatch.ElapsedMilliseconds, newAlbum.name);
                }
            }
        }

        /// <summary>
        /// Add a song to the library and save changes
        /// to database.
        /// </summary>
        private void AddSong(string filename, AudioFileMetaData metadata, artist artistObj, album albumObj)
        {
            var stopwatch = Stopwatch.StartNew();
            song newSong = null;
            
            // Build new song with all available information.
            if (albumObj != null && artistObj != null)
            {
                newSong = new song() { filename = filename, title = metadata.Title, albumId = albumObj.id, artistId = artistObj.id };
            }
            else if (albumObj != null && artistObj == null)
            {
                newSong = new song() { filename = filename, title = metadata.Title, albumId = albumObj.id, artistId = null };
            }
            else if (albumObj == null && artistObj != null)
            {
                newSong = new song() { filename = filename, title = metadata.Title, albumId = null, artistId = artistObj.id };
            }
            else if (albumObj == null && artistObj == null)
            {
                newSong = new song() { filename = filename, title = metadata.Title, albumId = null, artistId = null };
            }

            DukeboxData.songs.Add(newSong);
            DukeboxData.SaveChanges();

            stopwatch.Stop();
            _logger.InfoFormat("Added song with id {0} to library.", newSong.id);
            _logger.DebugFormat("Adding song to library took {0}ms. Song id: {1}", stopwatch.ElapsedMilliseconds, newSong.id);

            AlbumArtCacheService.GetInstance().AddSongToCache(newSong, metadata, albumObj);            
        }

        public void RemoveTrack (Track track)
        {
            if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            DukeboxData.songs.Remove(track.Song);
            DukeboxData.SaveChanges();
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
            var newPlaylist = new playlist() { name = name, filenamesCsv = string.Join(",", filenames) };
                      
            DukeboxData.playlists.Add(newPlaylist);
            DukeboxData.SaveChanges();

            // Invalidate playlist cache.
            _allPlaylistsCache = null;

            stopwatch.Stop();
            _logger.InfoFormat("Added playlist to library: {0}", newPlaylist.name);
            _logger.DebugFormat("Adding album to library took {0}ms. Playlist name: {1}", stopwatch.ElapsedMilliseconds, newPlaylist.name);
        }

        /// <summary>
        /// Clear the cache of artists and albums. The cache
        /// is automatically refilled at the end of this method.
        /// </summary>
        private void ClearCaches()
        {
            var stopwatch = Stopwatch.StartNew();

            _allArtistsCache = null;
            _allAlbumsCache = null;
            _allPlaylistsCache = null;

            var albums = OrderedAlbums;
            var artists = OrderedArtists;
            var playlists = OrderedPlaylists;

            stopwatch.Stop();
            _logger.Info("Music library artist and album caches refreshed! (This happens after a DB update routine)");
            _logger.DebugFormat("Refreshing library artist and album caches took {0}ms.", stopwatch.ElapsedMilliseconds);
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
        public List<Track> SearchForTracks(string searchTerm, List<SearchAreas> searchAreas)
        {
            var stopwatch = Stopwatch.StartNew();

            var songs = DukeboxData.songs;
            var matchingSongs = Enumerable.Empty<song>();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return songs.Select(s => new Track() { Song = s }).ToList();
            }
                        
            searchTerm = searchTerm.ToLower();
            searchAreas = searchAreas ?? new List<SearchAreas>();

            if (searchAreas.Contains(SearchAreas.All))
            {
                AddSearchAreasToList(searchAreas);
            }

            if (searchAreas.Contains(SearchAreas.Album))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.albumId != null ? instance.GetAlbumById(s.albumId).ToString().ToLower().Contains(searchTerm) : false));
            }

            if (searchAreas.Contains(SearchAreas.Artist))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(t => t.artistId != null ? instance.GetArtistById(t.artistId).ToString().ToLower().Contains(searchTerm) : false));
            }

            if (searchAreas.Contains(SearchAreas.Song))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.ToString().ToLower().Contains(searchTerm)));
            }

            if (searchAreas.Contains(SearchAreas.Filename))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.filename.ToLower().Contains(searchTerm)));
            }


            stopwatch.Stop();
            var searchAreasString = searchAreas.Select(sa => Enum.GetName(typeof(SearchAreas), sa)).Aggregate((c, n) => c + ", " + n);
            _logger.DebugFormat("Getting tracks by attribute(s) '{0}' where name or title equal '{1}' took {2}ms and returned {3} results.",
                searchAreasString, searchTerm, stopwatch.ElapsedMilliseconds, matchingSongs.Count());

            return matchingSongs.Count() < 1 ? new List<Track>() : matchingSongs.Select(s => new Track() { Song = s }).ToList();
        }

        /// <summary>
        /// Get a list of tracks who's attribute type equals the string specified 
        /// as their name, or in the case of songs title. This search is case sensitive.
        /// </summary>
        /// <param name="attribute">The attribute type to select.</param>
        /// <param name="nameOrTitle">The value or the attributes name/title.</param>
        /// <returns>A list of tracks that match the given attribute keypair.</returns>
        public List<Track> GetTracksByAttribute(SearchAreas attribute, string nameOrTitle)
        {
            var stopwatch = Stopwatch.StartNew();

            var searchAreas = new List<SearchAreas>();
            var songs = DukeboxData.songs;
            var matchingSongs = Enumerable.Empty<song>();

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
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.albumId != null ? instance.GetAlbumById(s.albumId).Equals(nameOrTitle) : false));
            }

            if (searchAreas.Contains(SearchAreas.Artist))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(t => t.artistId != null ? instance.GetArtistById(t.artistId).Equals(nameOrTitle) : false));
            }

            if (searchAreas.Contains(SearchAreas.Song))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.Equals(nameOrTitle)));
            }

            if (searchAreas.Contains(SearchAreas.Filename))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.filename.Equals(nameOrTitle)));
            }

            stopwatch.Stop();
            _logger.DebugFormat("Getting tracks by attribute {0} where name or title equal '{1}' took {2}ms and returned {3} results.",
                Enum.GetName(typeof(SearchAreas), attribute), nameOrTitle, stopwatch.ElapsedMilliseconds, matchingSongs.Count());

            return matchingSongs.Count() < 1 ? new List<Track>() : matchingSongs.Select(s => new Track() { Song = s }).ToList();
        }


        /// <summary>
        /// Get a list of tracks who's attribute type equals the id specified. Filename 
        /// attribute is supported by this lookup method.
        /// </summary>
        /// <param name="attribute">The attribute type to select.</param>
        /// <param name="attributeId">The id of the attribute.</param>
        /// <returns>A list of tracks that match the given attribute keypair.</returns>
        public List<Track> GetTracksByAttribute(SearchAreas attribute, long attributeId)
        {
            var stopwatch = Stopwatch.StartNew();

            var searchAreas = new List<SearchAreas>();
            var songs = DukeboxData.songs;
            var matchingSongs = Enumerable.Empty<song>();

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
            _logger.DebugFormat("Getting tracks by attribute {0} and value {1} took {2}ms and returned {3} results.", 
                Enum.GetName(typeof(SearchAreas), attribute), attributeId, stopwatch.ElapsedMilliseconds, matchingSongs.Count());

            return  matchingSongs.Count() < 1 ? new List<Track>() : matchingSongs.Select(s => new Track() { Song = s }).ToList();
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

        #region Singleton pattern instance and accessor

        private static MusicLibrary instance;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static MusicLibrary GetInstance()
        {
            if (instance == null)
            {
                instance = new MusicLibrary();
            }

            return instance;
        }

        #endregion

        public artist GetArtistById(long? artistId)
        {
            var stopwatch = Stopwatch.StartNew();
            var artist = DukeboxData.artists.Where(a => a.id == artistId).FirstOrDefault();

            stopwatch.Stop();

            if (artist == null)
            {
                throw new Exception(string.Format("No artist with the id '{0}' was found in the database.", artistId));
            }

            _logger.DebugFormat("Look up for artist with id '{0}' took {1}ms.", artistId, stopwatch.ElapsedMilliseconds);
            return artist;
        }

        public int GetArtistCount()
        {
            return DukeboxData.artists.Count();
        }

        public album GetAlbumById(long? albumId)
        {
            var stopwatch = Stopwatch.StartNew();
            var album = DukeboxData.albums.Where(a => a.id == albumId).FirstOrDefault();

            stopwatch.Stop();

            if (album == null)
            {
                throw new Exception(string.Format("No album with the id '{0}' was found in the database.", albumId));
            }

            _logger.DebugFormat("Look up for album with id '{0}' took {1}ms.", albumId, stopwatch.ElapsedMilliseconds);
            return album;
        }

        public int GetAlbumCount()
        {
            return DukeboxData.albums.Count();
        }

        public playlist GetPlaylistById(long? playlistId)
        {
            var stopwatch = Stopwatch.StartNew();
            var playlist = DukeboxData.playlists.Where(p => p.id == playlistId).FirstOrDefault();

            stopwatch.Stop();

            if (playlist == null)
            {
                throw new Exception(string.Format("No playlist with the id '{0}' was found in the database.", playlistId));
            }

            _logger.DebugFormat("Look up for artist with id '{0}' took {1}ms.", playlistId, stopwatch.ElapsedMilliseconds);
            return playlist;
        }

        public int GetPlaylistCount()
        {
            return DukeboxData.playlists.Count();
        }
    }
}
