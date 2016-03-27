using Dukebox.Audio;
using Dukebox.Audio.Interfaces;
using Dukebox.Library;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using log4net;
using org.jaudiotagger.audio;
using org.jaudiotagger.tag;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Dukebox.Library.Services
{
    /// <summary>
    /// Holds metadata on a single audio file, including track information
    /// and album art, if available.
    /// </summary>
    public class AudioFileMetadata : IAudioFileMetadata
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IAlbumArtCacheService _albumArtCache;
        private ICdMetadataService _cdMetadataService;
        private IAudioCdService _audioCdService;

        #region Metadata properties

        private string _title = string.Empty;
        private string _artist = string.Empty;
        private string _album = string.Empty;

        private long _dbAlbumId;

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
                    _title = value;
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
                    _album = value;
                }
            }
        }

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

                if (_albumArtCache.CheckCacheForAlbum(_dbAlbumId))
                {
                    return _albumArtCache.GetAlbumArtFromCache(_dbAlbumId);
                }

                logger.InfoFormat("Fetching album artwork from file: {0}", _audioFile.getFile().getPath());
                return Image.FromStream(new MemoryStream(_tag.getFirstArtwork().getBinaryData()));
            }
        }

        #endregion

        public static IAudioFileMetadata BuildAudioFileMetaData(CdMetadata cdMetadata, int trackNumber)
        {
            var AudioFileMetadata = LibraryPackage.GetInstance<IAudioFileMetadata>() as AudioFileMetadata;

            AudioFileMetadata._title = cdMetadata.Tracks[trackNumber];
            AudioFileMetadata._artist = cdMetadata.Artist;
            AudioFileMetadata._album = cdMetadata.Album;

            return AudioFileMetadata as IAudioFileMetadata;
        }

        public static IAudioFileMetadata BuildAudioFileMetaData(string fileName, long albumId = -1)
        {
            var AudioFileMetadata = LibraryPackage.GetInstance<IAudioFileMetadata>() as AudioFileMetadata;

            try
            {
                var fileInfo = new FileInfo(fileName);

                if (fileInfo.Extension != ".cda")
                {
                    var javaFile = new java.io.File(fileInfo.FullName);

                    AudioFileMetadata._audioFile = AudioFileIO.read(javaFile);
                    AudioFileMetadata._tag = AudioFileMetadata._audioFile.getTag();
                }
                else
                {
                    AudioFileMetadata.GetDetailsFromCddbServer(fileName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error occured while parsing metadata from audio file '{0}'", fileName), ex);

                AudioFileMetadata._tag = null;
                AudioFileMetadata.GetTrackDetailsFromUnsupportedFormat(fileName);
            }

            AudioFileMetadata._dbAlbumId = albumId;

            return AudioFileMetadata as IAudioFileMetadata;
        }

        public AudioFileMetadata(IAlbumArtCacheService albumArtCache, ICdMetadataService cdMetadataService, IAudioCdService audioCdService)
        {
            _albumArtCache = albumArtCache;
            _cdMetadataService = cdMetadataService;
            _audioCdService = audioCdService;
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

        private void GetTrackDetailsFromUnsupportedFormat(string fileName)
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

            logger.InfoFormat("Parsed '{0}' as {1} - {2}", fileName, artist, title);

            _title = title;
            _artist = artist;
            _album = album;
        }

        private void GetDetailsFromCddbServer(string fileName)
        {
            CdMetadata cdData = _cdMetadataService.GetMetadataForCd(fileName[0]);
            int trackIdx = _audioCdService.GetTrackNumberFromCdaFilename(fileName);

            _artist = cdData.Artist;
            _album = cdData.Album;
            _title = cdData.Tracks[trackIdx];
        }

        public void CommitChangesToFile()
        {
            if (_audioFile == null)
            {
                throw new InvalidOperationException("There is no metadata tag available for this audio file!");
            }

            _audioFile.commit();
        }


    }
}
