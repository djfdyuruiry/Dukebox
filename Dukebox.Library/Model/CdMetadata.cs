using Dukebox.Audio;
using Dukebox.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Un4seen.Bass.AddOn.Cd;

namespace Dukebox.Library.Model
{
    /// <summary>
    /// Holds metadata for a single audio CD. Use GetMetatadataFormCd(..) to
    /// generate an instance of this class.
    /// </summary>
    public class CdMetadata
    {
        public static readonly Uri CDDB_SERVER = new Uri("http://www.freedb.org/");

        public string Artist { get; set; }
        public string Album { get; set; }
        public List<string> Tracks { get; set; }

        /// <summary>
        /// Prevent external instantiation.
        /// </summary>
        private CdMetadata()
        {
        }

        public static CdMetadata GetMetadataForCd(char driveLetter)
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

        public static List<AudioFileMetaData> GetAudioFileMetaDataForCd(char driveLetter)
        {
            CdMetadata metadata = GetMetadataForCd(driveLetter);
            List<AudioFileMetaData> audioMetadata = new List<AudioFileMetaData>();

            for (int i = 0; i < metadata.Tracks.Count(); i++)
            {
                audioMetadata.Add(new AudioFileMetaData(metadata, i));
            }

            return audioMetadata;
        }

    }
}
