using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using TagLib;
using Dukebox.Audio.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Services;

namespace Dukebox.Library.Factories
{
    public class AudioFileMetadataFactory
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Regex truncateSpacesRegex = new Regex("\\s+");
        private static readonly Type audioTagType = typeof(Tag);

        private readonly ICdMetadataService _cdMetadataService;
        private readonly IAudioCdService _audioCdService;

        public AudioFileMetadataFactory(ICdMetadataService cdMetadataService, IAudioCdService audioCdService)
        {
            _cdMetadataService = cdMetadataService;
            _audioCdService = audioCdService;
        }

        public IAudioFileMetadata BuildAudioFileMetadataInstance(CdMetadata cdMetadata, int trackNumber)
        {
            var title = cdMetadata.Tracks[trackNumber];
            var artist = cdMetadata.Artist;
            var album = cdMetadata.Album;

            return new AudioFileMetadata(string.Empty, title, artist, album, 0, null, false, false, 0);
        }
        public IAudioFileMetadata BuildAudioFileMetadataInstance(string audioFilePath)
        {
            return BuildAudioFileMetadataInstance(audioFilePath, -1);
        }

        public IAudioFileMetadata BuildAudioFileMetadataInstance(string audioFilePath, long albumId)
        {
            var dbAlbumId = albumId;
            var trackLength = 0;
            var extendedMetadata = new Dictionary<string, List<string>>();
            var hasFutherMetadataTag = false;
            var hasAlbumArt = false;
            var title = string.Empty;
            var artist = string.Empty;
            var album = string.Empty;

            try
            {
                var fileInfo = new FileInfo(audioFilePath);

                if (string.Equals(fileInfo.Extension, ".cda", StringComparison.OrdinalIgnoreCase))
                {
                    LoadDetailsFromCddbServer(audioFilePath, ref title, ref artist, ref album);
                    LoadMissingTrackDetailsFromFileName(audioFilePath, ref title, ref artist, ref album);

                    hasFutherMetadataTag = false;
                    trackLength = 0;

                    return new AudioFileMetadata(audioFilePath, title, artist, album, trackLength, 
                        extendedMetadata, hasFutherMetadataTag, hasAlbumArt, dbAlbumId);
                }

                using (var fileStream = new FileStream(audioFilePath, FileMode.Open))
                {
                    InspectAudioFileTag(audioFilePath, fileStream, 
                        ref title, ref artist, ref album, ref trackLength, 
                        ref hasAlbumArt, extendedMetadata);
                }

                LoadMissingTrackDetailsFromFileName(audioFilePath, ref title, ref artist, ref album);
                hasFutherMetadataTag = true;
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("Error occured while parsing metadata from audio file '{0}'", audioFilePath), ex);

                hasFutherMetadataTag = false;
                trackLength = 0;
                LoadMissingTrackDetailsFromFileName(audioFilePath, ref title, ref artist, ref album);

                extendedMetadata.Clear();
            }

            return new AudioFileMetadata(audioFilePath, title, artist, album, trackLength,
                        extendedMetadata, hasFutherMetadataTag, hasAlbumArt, dbAlbumId);
        }

        private void LoadDetailsFromCddbServer(string audioFilePath, ref string title, 
            ref string artist, ref string album)
        {
            CdMetadata cdData = _cdMetadataService.GetMetadataForCd(audioFilePath[0]);
            int trackIdx = _audioCdService.GetTrackNumberFromCdaFilename(audioFilePath);

            artist = cdData.Artist;
            album = cdData.Album;
            title = cdData.Tracks[trackIdx];
        }

        private void LoadMissingTrackDetailsFromFileName(string audioFilePath, ref string currentTitle,
             ref string currentArtist, ref string currentAlbum)
        {
            // validate existing metadata
            var titleExists = !string.IsNullOrWhiteSpace(currentTitle);
            var artistExists = !string.IsNullOrWhiteSpace(currentArtist);
            var albumExists = !string.IsNullOrWhiteSpace(currentAlbum);

            if (titleExists && artistExists && albumExists)
            {
                return;
            }

            // extract metadata from filename
            var audioFileName = Path.GetFileNameWithoutExtension(audioFilePath);
            audioFileName = truncateSpacesRegex.Replace(audioFileName, " ");

            string[] metadata = null;

            if (audioFileName.Contains('-'))
            {
                metadata = audioFileName.Split('-');
            }
            else if (audioFileName.Contains('_'))
            {
                metadata = audioFileName.Split('_');
            }
            else if (audioFileName.Contains(' '))
            {
                metadata = audioFileName.Split(' ');
            }

            if (metadata == null || metadata.Length < 1)
            {
                // metdata extraction failed, just use filename
                currentTitle = audioFileName;
            }
            else
            {
                // assign artist and build title
                currentArtist = metadata[0];
                var stringBuilder = new StringBuilder();

                for (var i = 1; i < metadata.Length; i++)
                {
                    stringBuilder.AppendFormat(" {0}", metadata[i]);
                }

                currentTitle = stringBuilder.ToString();
            }

            // assign found metadata or placeholders if metadata is invalid
            if (!titleExists)
            {
                currentTitle = string.IsNullOrWhiteSpace(currentTitle) ? "Unknown Title" : currentTitle;
            }

            if (!artistExists)
            {
                currentArtist = string.IsNullOrWhiteSpace(currentArtist) ? "Unknown Artist" : currentArtist;
            }

            if (!albumExists)
            {
                currentAlbum = "Unknown Album";
            }

            // clean strings of NUL terminator rubbish data
            currentTitle = currentTitle.Trim().Replace("\0", string.Empty);
            currentArtist = currentArtist.Trim().Replace("\0", string.Empty);
            currentAlbum = currentAlbum.Trim().Replace("\0", string.Empty);

            logger.InfoFormat("Updated metadata for file ('{0}') to '{1}' - '{2}' [{3}]", 
                audioFilePath, currentArtist, currentTitle, currentAlbum);
        }

        private void InspectAudioFileTag(string audioFilePath, FileStream fileStream,
            ref string title, ref string artist, ref string album, ref int trackLength,
            ref bool hasAlbumArt, Dictionary<string, List<string>> extendedMetadata)
        {
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(audioFilePath, fileStream, fileStream));
            var tag = tagFile.Tag;

            if (tag == null)
            {
                throw new Exception("Audio file does not contain a valid tag");
            }

            title = tag.Title;
            artist = tag.FirstPerformer;
            album = tag.Album;

            trackLength = (int)tagFile.Properties.Duration.TotalSeconds;

            try
            {
                var artwork = tag.Pictures;
                hasAlbumArt = artwork?[0] != null;
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("Failed to extract album art info from the tag in audio file '{0}'", audioFilePath), ex);
                hasAlbumArt = false;
            }

            try
            {
                PopulateExtendedMetadata(tag, extendedMetadata);
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("Failed to extract extended metadata from the tag in audio file '{0}'", audioFilePath), ex);
                extendedMetadata.Clear();
            }
        }

        private void PopulateExtendedMetadata(Tag tag, Dictionary<string, List<string>> extendedMetadata)
        {
            foreach (var property in audioTagType.GetProperties())
            {
                if (property.CanRead)
                {
                    var values = property.PropertyType.IsArray ?
                          ((object[])property.GetValue(tag))?.Select(o => o.ToString())?.ToList() ?? new List<string>()
                        : new List<string> { property.GetValue(tag)?.ToString() ?? string.Empty };

                    var validValues = values.Where(s => !string.IsNullOrEmpty(s)).ToList();

                    if (validValues.Any())
                    {
                        extendedMetadata[property.Name] = validValues;
                    }
                }
            }
        }
    }
}
