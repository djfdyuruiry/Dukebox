using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;
using Un4seen.Bass.AddOn.Cd;
using Dukebox.Audio.Interfaces;
using Dukebox.Library.Factories;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services
{
    public class CdMetadataService : ICdMetadataService
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly Uri cddbServer = new Uri("http://www.freedb.org/");

        private readonly AudioFileMetadataFactory _audioFileMetadataFactory;
        private readonly IAudioCdService _audioCdService;

        public CdMetadataService(IAudioCdService audioCdService)
        {
            _audioFileMetadataFactory = new AudioFileMetadataFactory(this, audioCdService);
            _audioCdService = audioCdService;
        }

        public CdMetadata GetMetadataForCd(char driveLetter)
        {
            driveLetter = char.ToLower(driveLetter);

            // Not a valid drive letter.
            if (driveLetter < 'a' || driveLetter > 'z')
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid drive letter! Drive letter should be a letter between a-z.", driveLetter));
            }

            CheckForCddbServerConnection();

            // Find drive details.
            var driveIndex = _audioCdService.GetCdDriveIndex(driveLetter);
            var entries = BassCd.BASS_CD_GetID(driveIndex, BASSCDId.BASS_CDID_CDDB_QUERY + 1);

            // Get album name from server response.
            var album = string.Empty;
            var albumInfoMatch = Regex.Match(entries, "DTITLE=");

            if (albumInfoMatch != null)
            {
                var ablumIdx = albumInfoMatch.Index;
                album = entries.Substring(ablumIdx, entries.Substring(ablumIdx).IndexOf("\r", StringComparison.Ordinal)).Split('=').LastOrDefault().Split('/').LastOrDefault();
                album = album.Substring(1);
            }

            // Get artist name from server response.
            var artist = string.Empty;

            if (albumInfoMatch != null)
            {
                var artistIdx = albumInfoMatch.Index;
                artist = entries.Substring(artistIdx, entries.Substring(artistIdx).IndexOf("\r", StringComparison.Ordinal)).Split('=').LastOrDefault().Split('/').FirstOrDefault();
                artist = artist.Substring(0, artist.Length - 1);
            }

            // Get track names from server response.
            var matches = Regex.Matches(entries, "TTITLE");
            var trackNames = new List<string>();

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

            for (int i = 0; i < metadata.Tracks.Count; i++)
            {
                audioMetadata.Add(_audioFileMetadataFactory.BuildAudioFileMetadataInstance(metadata, i));
            }

            return audioMetadata;
        }

        /// <exception cref="Exception"> If there is no connection to the CDDB_SERVER.</exception>
        private void CheckForCddbServerConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead(cddbServer))
                    {
                        logger.DebugFormat("Succesfully connected to CDDB server @ '{0}'", cddbServer);
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
