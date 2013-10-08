using Jukebox.Audio;
using Jukebox.Library.Model;
using Jukebox.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Un4seen.Bass.AddOn.Cd;
using Un4seen.Bass.AddOn.Tags;

namespace Jukebox.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class AudioFileMetaData
    {
        #region Metadata properties

        private string _title;
        private string _artist;
        private string _album;
        
        public string Title 
        {
            get
            {
                if (Tag != null)
                {
                    return Tag.title;
                }

                return _title;
            }
        }

        public string Artist 
        {
            get
            {
                if (Tag != null)
                {
                    return Tag.artist;
                }

                return _artist;
            }
        }

        public string Album 
        {
            get
            {
                if (Tag != null)
                {
                    return Tag.album;
                }

                return _album;
            }
        }

        public TAG_INFO Tag { get; set; }

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
                    Tag = BassTags.BASS_TAG_GetFromFile(fileName);
                }
                else
                {
                    GetDetailsFromCddbServer(fileName);
                }
            }
            catch (Exception ex)
            {
                Logger.log("Error occured while parsing metadata from '" + fileName + "': " + ex.InnerException.Message);
                
                Tag = null;
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

            if (fileName.Contains('_'))
            {
                metadata = fileName.Split('\\').LastOrDefault().Split('_');
            } 
            else if (fileName.Contains('-'))
            {
                metadata = fileName.Split('-');
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

            // Find drive details.
            int driveIndex = MediaPlayer.GetCdDriveIndex(driveLetter);

            string cddbResponse = BassCd.BASS_CD_GetID(driveIndex, BASSCDId.BASS_CDID_CDDB_QUERY);
            string entries = BassCd.BASS_CD_GetID(driveIndex, BASSCDId.BASS_CDID_CDDB_QUERY + 1);

            if (cddbResponse == null)
            {
                throw new Exception("Cannot connect to CDDB information server @ freedb.freedb.org!");
            }

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
    }
}
