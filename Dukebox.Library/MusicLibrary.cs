using Dukebox.Logging;
using Dukebox.Model;
using Dukebox.Audio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Un4seen.Bass.AddOn.Tags;
using Dukebox.Library.Model;
using System.Threading;
using System.Drawing;

namespace Dukebox.Library
{
    /// <summary>
    /// 
    /// </summary>
    public class MusicLibrary
    {
        public DukeboxEntities DukeboxData { get; set; }

        #region Views on music library data

        /// <summary>
        /// 
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
        /// 
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
        /// 
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

        // Singleton pattern private constructor.
        private MusicLibrary()
        {
            DukeboxData = new DukeboxEntities();
            DukeboxData.Database.Connection.Open();
            var loadTheTracks = Tracks;
        }

        #region Folder/Playlist/File playback methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="subDirectories"></param>
        /// <returns></returns>
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
                Track t = GetTrackFromFile(file);

                tracksToReturn.Add(t);
            }

            return tracksToReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="song"></param>
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

            track.Song.title = track.Metadata.Title;
            track.Album.name = track.Metadata.Album;
            track.Album.name = track.Metadata.Artist;

            return track;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public List<Track> GetTracksFromPlaylistFile(string filename)
        {
            StreamReader playlistFile = new StreamReader(filename);
            string jsonTracks = playlistFile.ReadToEnd();

            List<string> files = JsonConvert.DeserializeObject<List<string>>(jsonTracks);

            List<Track> tracks = files.Where(t => File.Exists(t)).
                                            Select(t => MusicLibrary.GetInstance().GetTrackFromFile(t)).
                                            ToList();
            return tracks;
        }

        #endregion

        #region Database update methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="subDirectories"></param>
        /// <returns></returns>
        /// <exception cref="Exception">If the directory lookup operation fails.</exception>
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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
        /// 
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

                if(albumObj !=  null && track.Value.Tag.PictureCount > 0)
                {
                    if (!Directory.Exists("./albumArtCache"))
                    {
                        Directory.CreateDirectory("./albumArtCache");
                    }

                    Image img = track.Value.Tag.PictureGet(0).PictureImage;

                    if (img != null && !File.Exists("./albumArtCache/" + albumObj.id))
                    {
                        img.Save("./albumArtCache/" + albumObj.id);
                    }
                }

                Logger.log("Added the file '" + track.Key + "' to the library [Track => {" + (artistObj != null ? artistObj.name : "Unkown Artist") + " - " + track.Value.Title + "}");
            }
        }

        /// <summary>
        /// 
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

        /// <summary>
        /// Convenience method to extract file extension
        /// from a file info object build using the given
        /// file name.
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
    /// 
    /// </summary>
    public class AudioFileImportedEventArgs : EventArgs
    {
        public bool JustProcessing { get; set; }
        public string FileAdded { get; set; }
        public int TotalFilesThisImport { get; set; }

        public AudioFileImportedEventArgs() : base()
        {
        }
    }
}
