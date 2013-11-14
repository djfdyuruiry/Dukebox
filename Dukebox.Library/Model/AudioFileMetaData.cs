﻿using Dukebox.Audio;
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
    /// Holds metadata on a single audio file, including track information
    /// and album art, if available.
    /// </summary>
    public class AudioFileMetaData
    {
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

                if (File.Exists(@".\albumArtCache\" + _dbAlbumId))
                {
                    // Fetch artwork from cache instead of file.
                    Logger.log("Fetching album artwork for " + _title + " from cache...");
                    return Image.FromFile(".\\albumArtCache\\" + _dbAlbumId);
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

        public AudioFileMetaData(CdMetadata cdMetadata, int trackNumber)
        {
            _title = cdMetadata.Tracks[trackNumber];
            _artist = cdMetadata.Artist;
            _album = cdMetadata.Album;
        }

        public AudioFileMetaData(string fileName, long albumId = -1)
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
                GetTrackDetailsFromUnsupportedFormat(fileName);
            }

            _dbAlbumId = albumId;
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

            Logger.log("Parsed '" + fileName + "' as " + artist + " - " + title);
                        
            _title = title;
            _artist = artist;
            _album = album;
        }

        private void GetDetailsFromCddbServer(string fileName)
        {
            CdMetadata cdData = CdMetadata.GetMetadataForCd(fileName[0]);
            int trackIdx = MediaPlayer.GetTrackNumberFromCdaFilename(fileName);

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
