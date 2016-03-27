using Dukebox.Audio;
using Dukebox.Audio.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Un4seen.Bass.AddOn.Cd;

namespace Dukebox.Library.Services
{
    /// <summary>
    /// Holds metadata for a single audio CD. Use GetMetatadataFormCd(..) to
    /// generate an instance of this class.
    /// </summary>
    public class CdMetadataService : ICdMetadataService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly Uri cddbServer = new Uri("http://www.freedb.org/");

        private IAudioCdService _audioCdService;

        public CdMetadataService(IAudioCdService audioCdService)
        {
            _audioCdService = audioCdService;
        }

        public CdMetadata GetMetadataForCd(char driveLetter)
        {
            driveLetter = Char.ToLower(driveLetter);

            // Not a valid drive letter.
            if (driveLetter < 'a' || driveLetter > 'z')
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid drive letter! Drive letter should be a letter between a-z.", driveLetter));
            }

            CheckForCddbServerConnection();

            // Find drive details.
            int driveIndex = _audioCdService.GetCdDriveIndex(driveLetter);

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

        public List<IAudioFileMetadata> GetAudioFileMetaDataForCd(char driveLetter)
        {
            CdMetadata metadata = GetMetadataForCd(driveLetter);
            List<IAudioFileMetadata> audioMetadata = new List<IAudioFileMetadata>();

            for (int i = 0; i < metadata.Tracks.Count(); i++)
            {
                audioMetadata.Add(AudioFileMetadata.BuildAudioFileMetaData(metadata, i));
            }

            return audioMetadata;
        }

        /// <summary>
        /// This methods throws an exception if there is
        /// no connection to the CDDB_SERVER.
        /// </summary>
        private void CheckForCddbServerConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead(cddbServer))
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                var errMsg = string.Format("Cannot connect to CDDB information server @ '{0}'", cddbServer);

                logger.Error(errMsg, ex);
                throw new Exception(errMsg, ex);
            }
        }
    }
}
