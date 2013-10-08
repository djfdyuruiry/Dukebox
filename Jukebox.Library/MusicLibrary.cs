using Jukebox.Logging;
using Jukebox.Model;
using Jukebox.Audio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Un4seen.Bass.AddOn.Tags;

namespace Jukebox.Library
{
    /// <summary>
    /// 
    /// </summary>
    public class MusicLibrary
    {
        private jukeboxEntities jukeboxData;

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

                    foreach (song s in jukeboxData.songs)
                    {
                        var artist = s.artistId.HasValue ? jukeboxData.artists.Where(a => a.id == s.artistId.Value).FirstOrDefault() : null;
                        var album = s.albumId.HasValue ? jukeboxData.albums.Where(a => a.id == s.albumId.Value).FirstOrDefault() : null;
                        
                        _allTrackCache.Add(new Track() { Song = s, Album = album, Artist = artist, Metadata = new AudioFileMetaData(s.filename) });
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
        public List<artist> Artists
        {
            get
            {

                return Tracks.Select(t => t.Artist).Where(a => a != null).Distinct().OrderBy(a => a.name).ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<album> Albums
        {
            get
            {
                return Tracks.Select(t => t.Album).Where(a => a != null).Distinct().OrderBy(a => a.name).ToList();
            }
        }

        #endregion

        // Singleton pattern private constructor.
        private MusicLibrary()
        {
            jukeboxData = new jukeboxEntities();
            jukeboxData.Database.Connection.Open();
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

            Dictionary<string, AudioFileMetaData> tracks = new Dictionary<string, AudioFileMetaData>();
            List<Track> tracksToReturn = new List<Track>();

            allfiles = Directory.GetFiles(@directory, "*.*", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            supportedFiles = allfiles.Where(f => AudioFileFormats.SupportedFormats.Contains(GetFileExtension(f))).ToList();
            existingFiles = Tracks.Select(t => t.Song.filename).ToList();

            var otherFile = allfiles.Where(f => !supportedFiles.Contains(f)).ToList();

            validFiles = supportedFiles.Where(sf => !existingFiles.Contains(sf)).ToList();
            validFiles.ForEach(f =>  tracks.Add(f, new AudioFileMetaData(f)));

            foreach (KeyValuePair<string, AudioFileMetaData> kvp in tracks)
            {
                Track t = GetTrackFromFile(kvp);

                if (t != null)
                {
                    tracksToReturn.Add(t);
                }
            }

            return tracksToReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kvp"></param>
        /// <returns></returns>
        public Track GetTrackFromFile(KeyValuePair<string, AudioFileMetaData> kvp)
        {
            // Find artist and album information from the library.
            artist artistObj = new artist() { id = -1, name = kvp.Value.Artist };
            album albumObj = new album() { id = -1, name = kvp.Value.Album };

            song trackSong = null;
            int songId = -1;

            // Insert the song will all available information.
            if (albumObj != null && artistObj != null)
            {
                trackSong = (new song() { id = songId, filename = kvp.Key, title = kvp.Value.Title, albumId = albumObj.id, artistId = artistObj.id });
            }
            else if (albumObj != null && artistObj == null)
            {
                trackSong = (new song() { id = songId, filename = kvp.Key, title = kvp.Value.Title, albumId = albumObj.id, artistId = null });
            }
            else if (albumObj == null && artistObj != null)
            {
                trackSong = (new song() { id = songId, filename = kvp.Key, title = kvp.Value.Title, albumId = null, artistId = artistObj.id });
            }
            else if (albumObj == null && artistObj == null)
            {
                trackSong = (new song() { id = songId, filename = kvp.Key, title = kvp.Value.Title, albumId = null, artistId = null });
            }

            if (trackSong != null)
            {
                return new Track() { Album = albumObj, Artist = artistObj, Song = trackSong, Metadata = kvp.Value };
            }

            return null;
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
                                            Select(t => MusicLibrary.GetInstance().GetTrackFromFile(new KeyValuePair<string, AudioFileMetaData>(t, new AudioFileMetaData(t)))).
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
            List<string> allfiles;
            List<string> supportedFiles;
            List<string> existingFiles;
            List<string> filesToAdd;
            
            Dictionary<string, AudioFileMetaData> tracks = new Dictionary<string, AudioFileMetaData>();
                        
            allfiles = Directory.GetFiles(@directory, "*.*", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            supportedFiles = allfiles.Where(f => AudioFileFormats.SupportedFormats.Contains(GetFileExtension(f))).ToList();
            existingFiles = Tracks.Select(t => t.Song.filename).ToList();

            filesToAdd = supportedFiles.Where(sf => !existingFiles.Contains(sf)).ToList();
            int numFilesToAdd = filesToAdd.Count;

            for (int i = 0; i < numFilesToAdd; i++)
            {
                string file = filesToAdd[i];

                progressHandler.Invoke(this, new AudioFileImportedEventArgs() { JustProcessing = true, FileAdded = file, TotalFilesThisImport = numFilesToAdd, ImportIndex = i });
                tracks.Add(file, new AudioFileMetaData(file));
            }

            int idx = 0;

            foreach (KeyValuePair<string, AudioFileMetaData> kvp in tracks)
            {
                AddFileToLibrary(kvp);
                idx++;

                progressHandler.Invoke(this, new AudioFileImportedEventArgs() { JustProcessing = false, FileAdded = kvp.Key, ImportIndex = idx, TotalFilesThisImport = numFilesToAdd });
            }

            progressHandler.Invoke(this, new AudioFileImportedEventArgs() { JustProcessing = false, FileAdded = " the track cache, this may take some time.", ImportIndex = numFilesToAdd, TotalFilesThisImport = numFilesToAdd });
                        
            ClearTrackCache();
            var invoke = MusicLibrary.GetInstance().Tracks.FirstOrDefault();

            completeHandler.Invoke(this, filesToAdd.Count());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kvp"></param>
        public void AddFileToLibrary(KeyValuePair<string, AudioFileMetaData> kvp)
        {
            // Find artist and album information from the library.
            artist artistObj = jukeboxData.artists.Where(a => a.name == kvp.Value.Artist).FirstOrDefault();
            album albumObj = jukeboxData.albums.Where(a => a.name == kvp.Value.Album).FirstOrDefault();

            // Insert artist/album information if missing from library.
            if (artistObj == null && kvp.Value.Artist != string.Empty)
            {
                AddArtistToLibrary(kvp.Value);
                artistObj = jukeboxData.artists.Where(a => a.name == kvp.Value.Artist).FirstOrDefault();
            }

            if (albumObj == null && kvp.Value.Album != string.Empty)
            {
                AddAlbumToLibrary(kvp.Value);
                albumObj = jukeboxData.albums.Where(a => a.name == kvp.Value.Album).FirstOrDefault();
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
                artist newArtist = new artist() { id = jukeboxData.artists.Count(), name = tag.Artist };

                if (!jukeboxData.artists.Select(a => a.name).Contains(newArtist.name))
                {
                    jukeboxData.artists.Add(newArtist);
                    jukeboxData.SaveChanges();    
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
                album newAlbum = new album() { id = jukeboxData.albums.Count(), name = tag.Album };

                if (!jukeboxData.albums.Select(a => a.name).Contains(newAlbum.name))
                {
                    jukeboxData.albums.Add(newAlbum);
                    jukeboxData.SaveChanges();    
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddSongToLibrary(KeyValuePair<string, AudioFileMetaData> track, artist artistObj, album albumObj)
        {
            if (!jukeboxData.songs.Select(a => a.filename).Contains(track.Key))
            {
                int songId = jukeboxData.albums.Count();

                // Insert the song will all available information.
                if (albumObj != null && artistObj != null)
                {
                    jukeboxData.songs.Add(new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = albumObj.id, artistId = artistObj.id });
                }
                else if (albumObj != null && artistObj == null)
                {
                    jukeboxData.songs.Add(new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = albumObj.id, artistId = null });
                }
                else if (albumObj == null && artistObj != null)
                {
                    jukeboxData.songs.Add(new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = null, artistId = artistObj.id });
                }
                else if (albumObj == null && artistObj == null)
                {
                    jukeboxData.songs.Add(new song() { id = songId, filename = track.Key, title = track.Value.Title, albumId = null, artistId = null });
                }

                jukeboxData.SaveChanges();

                Logger.log("Added the file '" + track.Key + "' to the library [Track => {" + artistObj.name + " - " + track.Value.Title + "}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClearTrackCache()
        {
            _allTrackCache = null;

            Logger.log("Music library track cache was cleared! (This happens after a DB update to prompt a refresh)");
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
        public int ImportIndex { get; set; }
        public int TotalFilesThisImport { get; set; }
        public int NumFilesRemainingToImport { get { return TotalFilesThisImport - ImportIndex; } }

        public AudioFileImportedEventArgs() : base()
        {
        }
    }
}
