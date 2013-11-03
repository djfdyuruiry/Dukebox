using Dukebox.Audio;
using Dukebox.Library.Model;
using Dukebox.Logging;
using org.jaudiotagger.audio;
using org.jaudiotagger.tag;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;
using Un4seen.Bass.AddOn.Tags;

namespace Dukebox.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class AudioFileMetaData
    {
        public static readonly Uri CDDB_SERVER = new Uri("http://www.freedb.org/");

        #region Metadata properties

        private string _title = string.Empty;
        private string _artist = string.Empty;
        private string _album = string.Empty;

        private AudioFile _audioFile;
        private Tag _tag;
        
        public string Title 
        {
            get
            {
                if (HasFutherMetadataTag)
                {
                    return ExtractFieldText(FieldKey.TITLE);
                }

                return _title;
            }
            set
            {
                if (HasFutherMetadataTag)
                {
                    _tag.setField(FieldKey.TITLE, value);
                }
                else
                {
                    _artist = value;
                }
            }
        }

        public string Artist 
        {
            get
            {
                if (HasFutherMetadataTag)
                {
                    return ExtractFieldText(FieldKey.ARTIST);
                }

                return _artist;
            }
            set
            {
                if (HasFutherMetadataTag)
                {
                    _tag.setField(FieldKey.ARTIST, value);
                }
                else
                {
                    _artist = value;
                }
            }
        }

        public string Album 
        {
            get
            {
                if (HasFutherMetadataTag)
                {
                    return ExtractFieldText(FieldKey.ALBUM);
                }

                return _album;
            }
            set
            {
                if (HasFutherMetadataTag)
                {
                    _tag.setField(FieldKey.ALBUM, value);
                }
                else
                {
                    _artist = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasFutherMetadataTag
        {
            get
            {
                return _tag != null && _audioFile != null;
            }
        }

        public int Length
        {
            get
            {
                if (!HasFutherMetadataTag)
                {
                    throw new InvalidOperationException("There is no metadata tag available for this audio file!");
                }

                return _audioFile.getAudioHeader().getTrackLength();
            }
        }

        /// <summary>
        /// Is there any album art in the audio file?
        /// </summary>
        public bool HasAlbumArt
        {
            get
            {
                // Has a jaudiotagger object, contains artwork and the artwork binary data is not empty!
                return HasFutherMetadataTag && _tag.getArtworkList().size() > 0 && _tag.getFirstArtwork().getBinaryData() != null;
            }
        }

        /// <summary>
        /// Get the first album art image found in the audio tag.
        /// </summary>
        public Image AlbumArt
        {
            get
            {
                if (!HasFutherMetadataTag)
                {
                    throw new InvalidOperationException("There is no metadata tag available for this audio file!");
                }

                Logger.log("Fetching album artwork from " + _title + "...");
                return Image.FromStream(new MemoryStream(_tag.getFirstArtwork().getBinaryData()));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string ExtractFieldText(FieldKey key)
        {
            if (HasFutherMetadataTag)
            {
                if (_tag.getFirstField(key) != null)
                {
                    if (_tag.getFirstField(key).toString().Split('"').Length > 1)
                    {
                        return _tag.getFirstField(key).toString().Split('"')[1];
                    }
                    else
                    {
                        return _tag.getFirstField(key).toString();
                    }
                }
            }

            return string.Empty;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        public AudioFileMetaData(CdMetadata cdMetadata, int trackNumber)
        {
            _title = cdMetadata.Tracks[trackNumber];
            _artist = cdMetadata.Artist;
            _album = cdMetadata.Album;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public AudioFileMetaData(string fileName)
        {
            try
            {
                if ((new FileInfo(fileName)).Extension != ".cda")
                {
                    _audioFile = AudioFileIO.read(new java.io.File(fileName));
                    _tag = _audioFile.getTag();
                }
                else
                {
                    GetDetailsFromCddbServer(fileName);
                }
            }
            catch (Exception ex)
            {
                Logger.log("Error occured while parsing metadata from '" + fileName + "': " + ex.Message);
                
                _tag = null;
                GetDetailsFromUnsupportedFormat(fileName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private void GetDetailsFromUnsupportedFormat(string fileName)
        {
            string title = string.Empty;
            string artist = string.Empty;
            string album = string.Empty;

            fileName = Path.GetFileNameWithoutExtension(fileName);

            string[] metadata = null;
            if (fileName.Contains('-'))
            {
                metadata = fileName.Split('-');
            }
            else if (fileName.Contains('_'))
            {
                metadata = fileName.Split('\\').LastOrDefault().Split('_');
            } 
            else
            {
                metadata = fileName.Split('\\').LastOrDefault().Split(' ');
            }

            artist = metadata.FirstOrDefault();

            if (metadata.Length > 1)
            {
                for (int i = 1; i < metadata.Length; i++)
                {
                    title += ' ' + metadata[i];
                }
            }

            Logger.log("Parsed '" + fileName + "' as " + artist + " - " + title);
                        
            _title = title;
            _artist = artist;
            _album = album;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        private void GetDetailsFromCddbServer(string fileName)
        {
            CdMetadata cdData = GetCdMetadata(fileName[0]);
            int trackIdx = MediaPlayer.GetTrackNumberFromCdaFilename(fileName);

            _artist = cdData.Artist;
            _album = cdData.Album;
            _title = cdData.Tracks[trackIdx];
        }

        /// <summary>
        /// 
        /// </summary>
        public void CommitChanges()
        {
            if (_audioFile != null)
            {
                _audioFile.commit();
            }
            else
            {
                throw new InvalidOperationException("There is no metadata tag available for this audio file!");
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driveLetter">CD drive letter to use.</param>
        /// <returns></returns>
        public static List<AudioFileMetaData> GetAudioFileMetaDataForCd(char driveLetter)
        {
            CdMetadata metadata = GetCdMetadata(driveLetter);
            List<AudioFileMetaData> audioMetadata = new List<AudioFileMetaData>();

            for (int i = 0; i < metadata.Tracks.Count(); i++)
            { 
                audioMetadata.Add(new AudioFileMetaData(metadata, i));
            }
            
            return audioMetadata;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driveLetter">CD drive letter to use.</param>
        /// <returns></returns>
        private static CdMetadata GetCdMetadata(char driveLetter)
        {
            driveLetter = Char.ToLower(driveLetter);

            // Not a valid drive letter.
            if (driveLetter < 'a' || driveLetter > 'z')
            {
                throw new ArgumentException("'" + driveLetter + "' is not a valid drive letter! Drive letter should be a letter between a-z.");
            }

            CheckForCddbServerConnection();

            // Find drive details.
            int driveIndex = MediaPlayer.GetCdDriveIndex(driveLetter);

            string cddbResponse = BassCd.BASS_CD_GetID(driveIndex, BASSCDId.BASS_CDID_CDDB_QUERY);
            string entries = BassCd.BASS_CD_GetID(driveIndex, BASSCDId.BASS_CDID_CDDB_QUERY + 1);

            // Get album name from server response.
            string album = string.Empty;
            Match albumInfoMatch = Regex.Match(entries, "DTITLE=");

            if (albumInfoMatch != null)
            {
                int ablumIdx = albumInfoMatch.Index;
                album = entries.Substring(ablumIdx, entries.Substring(ablumIdx).IndexOf("\r")).Split('=').LastOrDefault().Split('/').LastOrDefault();
                album = album.Substring(1);
            }

            // Get artist name from server response.
            string artist = string.Empty;

            if (albumInfoMatch != null)
            {
                int artistIdx = albumInfoMatch.Index;
                artist = entries.Substring(artistIdx, entries.Substring(artistIdx).IndexOf("\r")).Split('=').LastOrDefault().Split('/').FirstOrDefault();
                artist = artist.Substring(0, artist.Length - 1);
            }

            // Get track names from server response.
            var matches = Regex.Matches(entries, "TTITLE");
            List<string> trackNames = new List<string>();

            foreach (Match m in matches)
            {
                trackNames.Add(entries.Substring(m.Index, entries.Substring(m.Index).IndexOf('\r')).Split('=')[1]);
            }

            return new CdMetadata { Artist = artist, Album = album, Tracks = trackNames };
        }

        /// <summary>
        /// This methods throws an exception if there is
        /// no connection to the CDDB_SERVER.
        /// </summary>
        public static void CheckForCddbServerConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead(CDDB_SERVER))
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot connect to CDDB information server @ '" + CDDB_SERVER + "'! (" + ex.Message + ")");
            }
        }
    }
}
