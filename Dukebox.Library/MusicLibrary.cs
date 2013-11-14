using Dukebox.Audio;
using Dukebox.Logging;
using Dukebox.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace Dukebox.Library
{
    /// <summary>
    /// Handles the SQL music library ADO entities model. Preforms
    /// lookups and adds content to the music library database. Also
    /// allows you to fetch temporary models of audio files from directories 
    /// and playlists.
    /// </summary>
    public class MusicLibrary
    {
        /// <summary>
        /// The ADO entities model for the SQL music database.
        /// </summary>
        private DukeboxEntities DukeboxData { get; set; }

        #region Views on music library data

        /// <summary>
        /// All tracks currently stored in the database. Cached.
        /// </summary>
        private List<Track> _allTrackCache;
        public List<Track> Tracks 
        {
            get
            {
                if (_allTrackCache == null)
                {
                    _allTrackCache = new List<Track>();

                    foreach (song s in DukeboxData.songs)
                    { 
                        _allTrackCache.Add(new Track() { Song = s });
                    }

                    if (_allTrackCache.Count > 0)
                    {
                        Logger.log(_allTrackCache.Count + " tracks stored in memory cache from DB!");
                    }
                }

                return _allTrackCache;
            }
        }

        /// <summary>
        /// All artists currently stored in the database. Cached.
        /// </summary>
        private List<artist> _allArtistsCache;
        public List<artist> Artists
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
        public List<album> Albums
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

        #endregion

        /// <summary>
        /// Singleton pattern private constructor. Open connection to
        /// SQL lite database file and populate track cache
        /// </summary>
        private MusicLibrary()
        {
            DukeboxData = new DukeboxEntities();
            DukeboxData.Database.Connection.Open();
            var loadTheTracks = Tracks;
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
            List<string> allfiles;
            List<string> supportedFiles;
            List<string> existingFiles;
            List<string> validFiles;

            List<Track> tracksToReturn = new List<Track>();

            allfiles = Directory.GetFiles(@directory, "*.*", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            supportedFiles = allfiles.Where(f => AudioFileFormats.SupportedFormats.Contains(GetFileExtension(f))).ToList();
            existingFiles = Tracks.Select(t => t.Song.filename).ToList();

            var otherFile = allfiles.Where(f => !supportedFiles.Contains(f)).ToList();

            validFiles = supportedFiles.Where(sf => !existingFiles.Contains(sf)).ToList();

            foreach (string file in validFiles)
            {
                tracksToReturn.Add(GetTrackFromFile(file));
            }

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
            Dictionary<string, AudioFileMetaData> tracks = new Dictionary<string, AudioFileMetaData>();
            Mutex filesToAddMutex = new Mutex();
        
            List<string> allfiles = Directory.GetFiles(@directory, "*.*", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            List<string> supportedFiles = allfiles.Where(f => AudioFileFormats.SupportedFormats.Contains(GetFileExtension(f))).ToList();
            List<string> existingFiles = Tracks.Select(t => t.Song.filename).ToList();
            Queue<string> filesToAdd = new Queue<string>(supportedFiles.Where(sf => !existingFiles.Contains(sf)));

            int numFilesToAdd = filesToAdd.Count;

            // For each set of 5 files, create a new thread
            // which processes the metadata of those 
            while(filesToAdd.Count != 0)
            {
                string file = filesToAdd.Dequeue();

                progressHandler.Invoke(this, new AudioFileImportedEventArgs() { JustProcessing = true, FileAdded = file, TotalFilesThisImport = numFilesToAdd });
                tracks.Add(file, new AudioFileMetaData(file));
            }

            foreach (KeyValuePair<string, AudioFileMetaData> kvp in tracks)
            {
                AddFileToLibrary(kvp);
                progressHandler.Invoke(this, new AudioFileImportedEventArgs() { JustProcessing = false, FileAdded = kvp.Key, TotalFilesThisImport = numFilesToAdd });
            }
          
            ClearTrackCache();
            completeHandler.Invoke(this, numFilesToAdd);
        }

        /// <summary>
        /// Add a file to the library and save changes
        /// to database.
        /// </summary>
        /// <param name="kvp"></param>
        public void AddFileToLibrary(KeyValuePair<string, AudioFileMetaData> kvp)
        {
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
            }
        }

        /// <summary>
        /// Add all files specified in a playlist to the library and save changes
        /// to database.
        /// </summary>
        /// <param name="filename"></param>
        public void AddPlaylistFileToLibrary(string filename)
        {
            List<string> playlistFiles = GetTracksFromPlaylistFile(filename).Select(t => t.Song.filename).ToList();
            Dictionary<string, AudioFileMetaData> tracksToAdd = new Dictionary<string, AudioFileMetaData>();
            List<string> existingFiles;
            List<string> filesToAdd;

            existingFiles = Tracks.Select(t => t.Song.filename).ToList();
            filesToAdd = playlistFiles.Where(t => !existingFiles.Contains(t)).ToList();
            filesToAdd.ForEach(f => tracksToAdd.Add(f, new AudioFileMetaData(f)));

            foreach (KeyValuePair<string, AudioFileMetaData> kvp in tracksToAdd)
            {
                AddFileToLibrary(kvp);
            }

            if (tracksToAdd.Count > 0)
            {
                Logger.log("Added " + tracksToAdd + " from the playlist file '" + filename + "'");
            }
        }

        /// <summary>
        /// Add an artist to the library and save changes
        /// to database.
        /// </summary>
        private void AddArtistToLibrary(AudioFileMetaData tag)
        {
            if(tag.Artist != null)
            {
                artist newArtist = new artist() { id = DukeboxData.artists.Count(), name = tag.Artist };

                if (!Artists.Select(a => a.name).Contains(newArtist.name))
                {
                    DukeboxData.artists.Add(newArtist);
                    DukeboxData.SaveChanges();
                    _allArtistsCache = null;
                }
            }
        }

        /// <summary>
        /// Add an album to the library and save changes
        /// to database.
        /// </summary>
        private void AddAlbumToLibrary(AudioFileMetaData tag)
        {
            if(tag.Album != null)
            {
                album newAlbum = new album() { id = DukeboxData.albums.Count(), name = tag.Album };

                if (!Albums.Select(a => a.name).Contains(newAlbum.name))
                {
                    DukeboxData.albums.Add(newAlbum);
                    DukeboxData.SaveChanges();
                    _allAlbumsCache = null;
                }
            }
        }

        /// <summary>
        /// Add a song to the library and save changes
        /// to database.
        /// </summary>
        private void AddSongToLibrary(KeyValuePair<string, AudioFileMetaData> track, artist artistObj, album albumObj)
        {
            if (!DukeboxData.songs.Select(a => a.filename).Contains(track.Key))
            {
                int songId = DukeboxData.songs.Count() + 1;

                // Insert the song will all available information.
                if (albumObj != null && artistObj != null)
                {
                    DukeboxData.songs.Add(new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = albumObj.id, artistId = artistObj.id });
                }
                else if (albumObj != null && artistObj == null)
                {
                    DukeboxData.songs.Add(new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = albumObj.id, artistId = null });
                }
                else if (albumObj == null && artistObj != null)
                {
                    DukeboxData.songs.Add(new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = null, artistId = artistObj.id });
                }
                else if (albumObj == null && artistObj == null)
                {
                    DukeboxData.songs.Add(new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = null, artistId = null });
                }

                DukeboxData.SaveChanges();

                if(albumObj !=  null && track.Value.HasFutherMetadataTag && track.Value.HasAlbumArt)
                {
                    if (!Directory.Exists("./albumArtCache"))
                    {
                        Directory.CreateDirectory("./albumArtCache");
                    }

                    try
                    {
                        Image img = track.Value.AlbumArt;

                        if (img != null && !File.Exists("./albumArtCache/" + albumObj.id))
                        {
                            img.Save("./albumArtCache/" + albumObj.id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.log("Error extracting image for caching [" + track.Key + "]: " + ex.Message);
                    }
                }

                Logger.log("Added the file '" + track.Key + "' to the library [Track => {" + (artistObj != null ? artistObj.name : "Unkown Artist") + " - " + track.Value.Title + "}");
            }
        }

        /// <summary>
        /// Clear the cache of tracks, artists and albums. The cache
        /// is automatically refilled at the end of this method.
        /// </summary>
        private void ClearTrackCache()
        {
            _allTrackCache = null;
            _allArtistsCache = null;
            _allAlbumsCache = null;

            var getTheTracks = Tracks;

            Logger.log("Music library track cache was cleared! (This happens after a DB update routine prompts a refresh)");
        }

        #endregion

        #region Library search

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
        public List<Track> SearchForTracks(string searchTerm, SearchAreas searchArea)
        {
            List<Track> results = null;
            searchTerm = searchTerm.ToLower();

            switch (searchArea)
            {
                case SearchAreas.Album:
                {
                    results = Tracks.Where(t => t.Album != null ? t.Album.ToString().ToLower().Contains(searchTerm) : false).ToList();
                    break;
                }
                case SearchAreas.Artist:
                {
                    results = Tracks.Where(t => t.Artist != null ? t.Artist.ToString().ToLower().Contains(searchTerm) : false).ToList();
                    break;
                }
                case SearchAreas.Song:
                {
                    results = Tracks.Where(t => t.Song != null ? t.Song.ToString().ToLower().Contains(searchTerm) : false).ToList();
                    break;
                }
                default:
                {
                    results = Tracks.Where(t => t.ToString().ToLower().Contains(searchTerm)).ToList();
                    break;
                }
            }

            return results == null ? new List<Track>() : results;
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
            List<Track> results = null;

            switch (attribute)
            {
                case SearchAreas.Album:
                {
                    results = Tracks.Where(t => t.Album != null ? t.Album.name == nameOrTitle : false).ToList();
                    break;
                }
                case SearchAreas.Artist:
                {
                    results = Tracks.Where(t => t.Artist != null ? t.Artist.name== nameOrTitle : false).ToList();
                    break;
                }
                case SearchAreas.Song:
                {
                    results = Tracks.Where(t => t.Song != null ? t.Song.title == nameOrTitle : false).ToList();
                    break;
                }
                default:
                {
                    results = Tracks.Where(t => t.ToString().Contains(nameOrTitle)).ToList();
                    break;
                }
            }

            return results == null ? new List<Track>() : results;
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
    }
    
    /// <summary>
    /// Arguments in the event of an audio file being imported
    /// or being pre-processed.
    /// </summary>
    public class AudioFileImportedEventArgs : EventArgs
    {
        /// <summary>
        /// Is the track going through pre-processing?
        /// </summary>
        public bool JustProcessing { get; set; }

        /// <summary>
        /// The absolute filename that was added to the library.
        /// </summary>
        public string FileAdded { get; set; }

        /// <summary>
        /// Total files that are scheduled for this events parent operation.
        /// </summary>
        public int TotalFilesThisImport { get; set; }

        /// <summary>
        /// Call EventArgs constructor.
        /// </summary>
        public AudioFileImportedEventArgs() : base()
        {
        }
    }
}
