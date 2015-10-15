using Dukebox.Audio;
using Dukebox.Library.Model;
using Dukebox.Library.Services;
using Dukebox.Model;
using Dukebox.Model.Services;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MusicLibrary));
        /// <summary>
        /// The ADO entities model for the SQL music database.
        /// </summary>
        private DukeboxEntities DukeboxData { get; set; }

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

        public List<Track> RecentlyPlayed { get; set; }

        #endregion
        
        /// <summary>
        /// Singleton pattern private constructor. Open connection to
        /// SQL lite database file and populate track cache
        /// </summary>
        private MusicLibrary()
        {
            DukeboxData = new DukeboxEntities();
            DukeboxData.Database.Connection.Open();

            RecentlyPlayed = new List<Track>();
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
        public List<Track> GetTracksFromPlaylistFile(string playlistFile)
        {
            StreamReader playlistFileReader = new StreamReader(playlistFile);
            string jsonTracks = playlistFileReader.ReadToEnd();

            List<string> files = JsonConvert.DeserializeObject<List<string>>(jsonTracks);

            List<Track> tracks = files.Where(t => File.Exists(t))
                                      .Select(t => MusicLibrary.GetInstance().GetTrackFromFile(t))
                                      .ToList();
            return tracks;
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
        public void AddDirectoryToLibrary(string directory, bool subDirectories, Action<object, AudioFileImportedEventArgs> progressHandler, Action<object, int> completeHandler)
        {
            var stopwatch = Stopwatch.StartNew();
            var tracks = new Dictionary<string, AudioFileMetaData>();
            var filesToAddMutex = new Mutex();
        
            var allfiles = Directory.GetFiles(@directory, "*.*", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            var supportedFiles = allfiles.Where(f => AudioFileFormats.SupportedFormats.Contains(GetFileExtension(f)));
            var filesToAdd = supportedFiles.Where(sf => !DukeboxData.songs.Any((s) => s.filename.Equals(sf, StringComparison.CurrentCulture)));

            int numFilesToAdd = filesToAdd.Count();

            // For each set of 5 files, create a new thread
            // which processes the metadata of those 
            foreach (var file in filesToAdd)
            {
                progressHandler.Invoke(this, new AudioFileImportedEventArgs() { JustProcessing = true, FileAdded = file, TotalFilesThisImport = numFilesToAdd });
                tracks.Add(file, new AudioFileMetaData(file));
            }

            foreach (KeyValuePair<string, AudioFileMetaData> kvp in tracks)
            {
                AddFileToLibrary(kvp);
                progressHandler.Invoke(this, new AudioFileImportedEventArgs() { JustProcessing = false, FileAdded = kvp.Key, TotalFilesThisImport = numFilesToAdd });
            }
          
            ClearCaches();
            completeHandler.Invoke(this, numFilesToAdd);

            if (numFilesToAdd > 0)
            {
                stopwatch.Stop();
                _logger.InfoFormat("Added {0} tracks from directory: {1}", numFilesToAdd, directory);
                _logger.DebugFormat("Adding {0} tracks to library from a directory took {1}ms. Directory path: {2} (Sub-directories searched: {3})",
                    numFilesToAdd, stopwatch.ElapsedMilliseconds, directory, subDirectories);
            }
        }

        /// <summary>
        /// Add a file to the library and save changes
        /// to database.
        /// </summary>
        /// <param name="kvp"></param>
        public void AddFileToLibrary(KeyValuePair<string, AudioFileMetaData> kvp)
        {
            var stopwatch = Stopwatch.StartNew();

            // Find artist and album information from the library.
            artist artistObj = DukeboxData.artists.Where(a => a.name == kvp.Value.Artist).FirstOrDefault();
            album albumObj = DukeboxData.albums.Where(a => a.name == kvp.Value.Album).FirstOrDefault();

            // Insert artist/album information if missing from library.
            if (artistObj == null && kvp.Value.Artist != string.Empty)
            {
                AddArtistToLibrary(kvp.Value);
                artistObj = DukeboxData.artists.Where(a => a.name == kvp.Value.Artist).FirstOrDefault();
            }

            if (albumObj == null && kvp.Value.Album != string.Empty)
            {
                AddAlbumToLibrary(kvp.Value);
                albumObj = DukeboxData.albums.Where(a => a.name == kvp.Value.Album).FirstOrDefault();
            }

            if (kvp.Value.Title != string.Empty || albumObj != null || artistObj != null)
            {
                AddSongToLibrary(kvp, artistObj, albumObj);
                
                stopwatch.Stop();
                _logger.InfoFormat("Added file to library: {0}", kvp.Key);
                _logger.DebugFormat("Adding file to library took {0}ms. File path: {1}", stopwatch.ElapsedMilliseconds, kvp.Key);
            }
        }

        /// <summary>
        /// Add all files specified in a playlist to the library and save changes
        /// to database.
        /// </summary>
        /// <param name="filename"></param>
        public void AddPlaylistFileToLibrary(string filename)
        {
            var stopwatch = Stopwatch.StartNew();

            var playlistFiles = GetTracksFromPlaylistFile(filename).Select(t => t.Song.filename).ToList();
            var tracksToAdd = new Dictionary<string, AudioFileMetaData>();
            var existingFiles = DukeboxData.songs.Select(s => s.filename);

            var filesToAdd = playlistFiles.Except(DukeboxData.songs.Select(s => s.filename)).ToList();
            filesToAdd.ForEach(f => tracksToAdd.Add(f, new AudioFileMetaData(f)));

            foreach (KeyValuePair<string, AudioFileMetaData> kvp in tracksToAdd)
            {
                AddFileToLibrary(kvp);
            }

            if (tracksToAdd.Count > 0)
            {
                stopwatch.Stop();
                _logger.InfoFormat("Added {0} from playlist file: {1}", tracksToAdd.Count, filename);
                _logger.DebugFormat("Adding {0} tracks to library from a playlist took {1}ms. Playlist path: {2}",
                    tracksToAdd.Count, stopwatch.ElapsedMilliseconds, filename);
            }
        }

        /// <summary>
        /// Add an artist to the library and save changes
        /// to database.
        /// </summary>
        private void AddArtistToLibrary(AudioFileMetaData tag)
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
        private void AddAlbumToLibrary(AudioFileMetaData tag)
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
        private void AddSongToLibrary(KeyValuePair<string, AudioFileMetaData> track, artist artistObj, album albumObj)
        {
            if (DukeboxData.songs.Any(a => a.filename.Equals(track.Key, StringComparison.CurrentCulture)))
            {
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            song newSong = null;

            int songId = DukeboxData.songs.Count() + 1;

            // Build new song with all available information.
            if (albumObj != null && artistObj != null)
            {
                newSong = new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = albumObj.id, artistId = artistObj.id };
            }
            else if (albumObj != null && artistObj == null)
            {
                newSong = new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = albumObj.id, artistId = null };
            }
            else if (albumObj == null && artistObj != null)
            {
                newSong = new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = null, artistId = artistObj.id };
            }
            else if (albumObj == null && artistObj == null)
            {
                newSong = new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = null, artistId = null };
            }

            DukeboxData.songs.Add(newSong);
            DukeboxData.SaveChanges();

            stopwatch.Stop();
            _logger.InfoFormat("Added song with id {0} to library.", songId);
            _logger.DebugFormat("Adding song to library took {0}ms. Song id: {1}", stopwatch.ElapsedMilliseconds, songId);

            AlbumArtCacheService.GetInstance().AddSongToCache(newSong, track.Value, albumObj);            
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

            var albums = OrderedAlbums;
            var artists = OrderedArtists;

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

            if (attribute == null || attribute == SearchAreas.All)
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

            stopwatch.Stop();
            _logger.DebugFormat("Getting tracks by attribute {0} where name or title equal '{1}' took {2}ms and returned {3} results.",
                Enum.GetName(typeof(SearchAreas), attribute), nameOrTitle, stopwatch.ElapsedMilliseconds, matchingSongs.Count());

            return matchingSongs.Count() < 1 ? new List<Track>() : matchingSongs.Select(s => new Track() { Song = s }).ToList();
        }


        /// <summary>
        /// Get a list of tracks who's attribute type equals the id specified.
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

            if (attribute == null || attribute == SearchAreas.All)
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
            var result = DukeboxData.artists.Where(a => a.id == artistId).FirstOrDefault();

            stopwatch.Stop();
            _logger.DebugFormat("Look up for artist with id {0} took {1}ms.", artistId, stopwatch.ElapsedMilliseconds);
            return result;
        }

        public int GetArtistCount()
        {
            return DukeboxData.artists.Count();
        }

        public album GetAlbumById(long? albumId)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = DukeboxData.albums.Where(a => a.id == albumId).FirstOrDefault();

            stopwatch.Stop();
            _logger.DebugFormat("Look up for album with id {0} took {1}ms.", albumId, stopwatch.ElapsedMilliseconds);
            return result;
        }

        public int GetAlbumCount()
        {
            return DukeboxData.albums.Count();
        }
    }
}
