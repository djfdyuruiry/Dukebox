using System;
using System.Drawing;
using System.IO;
using System.Linq;
using log4net;
using org.jaudiotagger.audio;
using org.jaudiotagger.tag;
using Dukebox.Audio.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services
{
    /// <summary>
    /// Holds metadata on a single audio file, including track information
    /// and album art, if available.
    /// </summary>
    public class AudioFileMetadata : IAudioFileMetadata
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private ICdMetadataService _cdMetadataService;
        private IAudioCdService _audioCdService;

        #region Metadata properties
                
        private long _dbAlbumId;
        private int _trackLength;
        private bool _hasAlbumArt;

        public string AudioFilePath { get; private set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public bool HasFutherMetadataTag { get; private set;}

        public bool HasAlbumArt { get; private set; }

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Artist) && string.IsNullOrEmpty(Album);
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

                return _trackLength;
            }
        }

        /// <summary>
        /// Get the first album art image found in the audio tag.
        /// </summary>
        public Image AlbumArt
        {
            get
            {
                return GetAlbumArt();
            }
        }

        #endregion

        public static IAudioFileMetadata BuildAudioFileMetaData(CdMetadata cdMetadata, int trackNumber)
        {
            var AudioFileMetadata = LibraryPackage.GetInstance<IAudioFileMetadata>() as AudioFileMetadata;

            AudioFileMetadata.Title = cdMetadata.Tracks[trackNumber];
            AudioFileMetadata.Artist = cdMetadata.Artist;
            AudioFileMetadata.Album = cdMetadata.Album;

            return AudioFileMetadata as IAudioFileMetadata;
        }

        public static IAudioFileMetadata BuildAudioFileMetaData(string fileName, long albumId = -1)
        {
            var audioFileMetadata = LibraryPackage.GetInstance<IAudioFileMetadata>() as AudioFileMetadata;
            audioFileMetadata.AudioFilePath = fileName;

            try
            {
                var fileInfo = new FileInfo(audioFileMetadata.AudioFilePath);

                if (fileInfo.Extension != ".cda")
                {
                    var audioFile = OpenAudioFile(audioFileMetadata.AudioFilePath);
                    var tag = audioFile.getTag();

                    audioFileMetadata.Title = ExtractFieldText(tag, FieldKey.TITLE);
                    audioFileMetadata.Artist = ExtractFieldText(tag, FieldKey.ARTIST);
                    audioFileMetadata.Album = ExtractFieldText(tag, FieldKey.ALBUM);

                    var audioHeader = audioFile.getAudioHeader();
                    var artworkList = tag.getArtworkList();

                    audioFileMetadata._trackLength = audioHeader.getTrackLength();
                    audioFileMetadata.HasAlbumArt = artworkList.size() > 0;

                    if (string.IsNullOrEmpty(audioFileMetadata.Title))
                    {
                        audioFileMetadata.LoadMissingTrackDetailsFromFileName();
                    }

                    audioFileMetadata.HasFutherMetadataTag = true;
                }
                else
                {
                    audioFileMetadata.LoadDetailsFromCddbServer();
                }                
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error occured while parsing metadata from audio file '{0}'", audioFileMetadata.AudioFilePath), ex);

                audioFileMetadata.HasFutherMetadataTag = false;
                audioFileMetadata.LoadMissingTrackDetailsFromFileName();
            }

            audioFileMetadata._dbAlbumId = albumId;

            return audioFileMetadata as IAudioFileMetadata;
        }

        public AudioFileMetadata(ICdMetadataService cdMetadataService, IAudioCdService audioCdService)
        {
            _cdMetadataService = cdMetadataService;
            _audioCdService = audioCdService;
        }

        public Image GetAlbumArt(Action<Image> beforeStreamClosedCallback = null)
        {
            if (!HasAlbumArt)
            {
                throw new InvalidOperationException("There is no metadata tag available for this audio file!");
            }

            Image albumArt = null;

            logger.InfoFormat("Fetching album artwork from file: {0}", AudioFilePath);

            try
            {
                var audioFile = OpenAudioFile(AudioFilePath);
                var tag = audioFile.getTag();
                var artWork = tag.getFirstArtwork();

                var albumArtBytes = artWork.getBinaryData();

                if (albumArtBytes.Length == 0)
                {
                    throw new InvalidDataException("Album art binary data was empty");
                }

                using (var albumArtStream = new MemoryStream(albumArtBytes))
                {
                    albumArt = Image.FromStream(albumArtStream);
                    beforeStreamClosedCallback?.Invoke(albumArt);
                }

                if (albumArt == null)
                {
                    throw new Exception("Error opening memory stream to save album art image to disk");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error getting album art from metadata in file {0}", AudioFilePath), ex);
            }

            return albumArt;
        }

        public string SaveAlbumArtToTempFile()
        {
            try
            {
                var path = Path.GetTempFileName();

                GetAlbumArt(i => i.Save(path));

                return path;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error saving album art to temporary file from audio file at path '{0}'", AudioFilePath), ex);
            }
        }

        private void LoadMissingTrackDetailsFromFileName()
        {
            var titleExists = !string.IsNullOrEmpty(Title);
            var artistExists = !string.IsNullOrEmpty(Artist);
            var albumExists = !string.IsNullOrEmpty(Album);

            if (titleExists && artistExists)
            {
                return;
            }

            string title = string.Empty;
            string artist = string.Empty;

            AudioFilePath = Path.GetFileNameWithoutExtension(AudioFilePath);

            string[] metadata = null;
            if (AudioFilePath.Contains('-'))
            {
                metadata = AudioFilePath.Split('-');
            }
            else if (AudioFilePath.Contains('_'))
            {
                metadata = AudioFilePath.Split('\\').LastOrDefault().Split('_');
            }
            else
            {
                metadata = AudioFilePath.Split('\\').LastOrDefault().Split(' ');
            }

            artist = metadata.FirstOrDefault();

            if (metadata.Length > 1)
            {
                for (int i = 1; i < metadata.Length; i++)
                {
                    title += ' ' + metadata[i];
                }
            }

            if (!titleExists)
            {
                Title = string.IsNullOrWhiteSpace(title) ? "Unknown Title" : title;
            }

            if (!artistExists)
            {
                Artist = string.IsNullOrWhiteSpace(artist) ? "Unknown Artist" : artist;
            }

            if(!albumExists)
            {
                Album = "Unknown Alum";
            }

            logger.InfoFormat("Updated metadata for file ('{0}') to '{1}' - '{2}'", AudioFilePath, Artist, Title);
        }

        private void LoadDetailsFromCddbServer()
        {
            CdMetadata cdData = _cdMetadataService.GetMetadataForCd(AudioFilePath[0]);
            int trackIdx = _audioCdService.GetTrackNumberFromCdaFilename(AudioFilePath);

            Artist = cdData.Artist;
            Album = cdData.Album;
            Title = cdData.Tracks[trackIdx];
        }

        private static AudioFile OpenAudioFile(string audioFilePath)
        {
            var file = new java.io.File(audioFilePath);
            var audioFile = AudioFileIO.read(file);

            return audioFile;
        }

        private static string ExtractFieldText(Tag tag, FieldKey key)
        {
            if (tag == null || tag.getFirstField(key) == null)
            {
                return string.Empty;
            }

            if (tag.getFirstField(key).toString().Split('"').Length > 1)
            {
                return tag.getFirstField(key).toString().Split('"')[1];
            }

            return tag.getFirstField(key).toString();
        }
    }
}
