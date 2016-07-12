using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using TagLib;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Helper;

namespace Dukebox.Library.Services
{
    /// <summary>
    /// Holds metadata on a single audio file, including track information
    /// and album art, if available.
    /// </summary>
    public class AudioFileMetadata : IAudioFileMetadata
    {
        private const string getLengthWarningFormat = 
            "Unable to get audio length: there is no metadata tag for audio file '{0}', defaulting to 0";

        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Type audioTagType = typeof(Tag);

        private readonly long _dbAlbumId;
        private readonly int _trackLength;
        private readonly bool _hasAlbumArt;
        private readonly SemaphoreSlim _saveMetadataSemaphore;

        public string AudioFilePath { get; private set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public bool HasFutherMetadataTag { get; private set;}

        public bool HasAlbumArt
        {
            get
            {
                return HasFutherMetadataTag && _hasAlbumArt;                
            }
        }

        public int Length
        {
            get
            {
                if (!HasFutherMetadataTag)
                {
                    logger.WarnFormat(getLengthWarningFormat, AudioFilePath);
                }

                return _trackLength;
            }
        }

        public Dictionary<string, List<string>> ExtendedMetadata { get; private set; }

        public bool IsEmpty
        {
            get
            {
                return Title.Equals("Unknown Title") && Artist.Equals("Unknown Artist") && Album.Equals("Unknown Album");
            }
        }

        public AudioFileMetadata(string audioFilePath, string title, string artist, string album, int trackLength, 
            Dictionary<string, List<string>> extendedMetadata, bool hasFutherMetadataTag, bool hasAlbumArt, long dbAlbumId)
        {
            AudioFilePath = audioFilePath;
            
            Title = title;
            Artist = artist;
            Album = album;

            ExtendedMetadata = extendedMetadata ?? new Dictionary<string, List<string>>();
            HasFutherMetadataTag = hasFutherMetadataTag;

            _trackLength = trackLength;

            _hasAlbumArt = hasAlbumArt;
            _dbAlbumId = dbAlbumId;

            _saveMetadataSemaphore = new SemaphoreSlim(1, 1);
        }

        public Image GetAlbumArt()
        {
            return GetAlbumArt(null);
        }

        public Image GetAlbumArt(Action<Image> beforeStreamClosedCallback)
        {
            if (!HasAlbumArt)
            {
                throw new InvalidOperationException("There is no metadata tag for this audio file");
            }

            Image albumArt = null;

            try
            {
                albumArt = ProcessAlbumArt(beforeStreamClosedCallback);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error getting album art from metadata in file {0}", AudioFilePath), ex);
            }

            return albumArt;
        }

        private Image ProcessAlbumArt(Action<Image> beforeStreamClosedCallback)
        {
            Image albumArt;

            logger.InfoFormat("Fetching album artwork from file: {0}", AudioFilePath);

            using (var fileStream = new FileStream(AudioFilePath, FileMode.Open))
            {
                var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFilePath, fileStream, fileStream));

                var tag = tagFile.Tag;
                var artwork = tag.Pictures[0];

                var albumArtBytes = artwork.Data;

                if (albumArtBytes.IsEmpty)
                {
                    throw new InvalidDataException("Album art binary data was empty");
                }

                using (var albumArtStream = new MemoryStream(albumArtBytes.Data))
                {
                    albumArt = Image.FromStream(albumArtStream);
                    beforeStreamClosedCallback?.Invoke(albumArt);
                }
            }

            if (albumArt == null)
            {
                throw new Exception("Error opening memory stream to save album art image to disk");
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
        
        public void SaveMetadataToFileTag()
        {
            SaveMetadataToFileTag(null);
        }

        public void SaveMetadataToFileTag(Action atomicBeforeTagCommitAction)
        {
            try
            {
                if (Path.GetExtension(AudioFilePath).Equals(".cda", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(string.Format("Attempted to create a tag for CD track '{0}'", AudioFilePath));
                }

                _saveMetadataSemaphore.Wait();

                atomicBeforeTagCommitAction?.Invoke();

                using (var fileStream = new FileStream(AudioFilePath, FileMode.Open))
                {
                    var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFilePath, fileStream, fileStream));

                    var tag = tagFile.Tag;

                    if (tag == null)
                    {
                        var fileType = Path.GetExtension(AudioFilePath).ToLower();

                        if (!TagTypeMap.FileTypeMap.ContainsKey(fileType))
                        {
                            throw new InvalidOperationException(string.Format("TagLib does not support creating tags for files of type '{0}'", fileType));
                        }

                        tag = tagFile.GetTag(TagTypeMap.FileTypeMap[fileType], true);
                    }
                    
                    tag.Title = Title;
                    tag.Performers = new string[] { Artist };
                    tag.Album = Album;

                    ExtendedMetadataHelper.WriteExtendedMetadata(tag, ExtendedMetadata);

                    tagFile.Save();

                    logger.InfoFormat("Successfully updated metadata tag in file '{0}'", AudioFilePath);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("Error while creating or updating metadata tag in file '{0}'", AudioFilePath), ex);
            }
            finally
            {
                _saveMetadataSemaphore.Release();
            }
        }
    }
}
