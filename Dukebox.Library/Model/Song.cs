using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Newtonsoft.Json;

namespace Dukebox.Library.Model
{
    [Table("songs")]
    public class Song
    {
        private string _extendedMetadataJson;
        private Dictionary<string, List<string>> _extendedMetadata;
        private Artist _artist;
        private Album _album;

        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("fileName")]
        public string FileName { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; }

        [Required]
        [Column("lengthInSeconds")]
        public long LengthInSeconds { get; set; }

        [Column("extendedMetadataJson")]
        public virtual string ExtendedMetadataJson
        {
            get
            {
                return _extendedMetadataJson;
            }
            set
            {
                _extendedMetadataJson = value;
                _extendedMetadata = null;

                RefreshExtendedMetadata();
            }
        }

        [Required]
        [Column("artistName")]
        public virtual string ArtistName { get; set; }

        [Required]
        [Column("albumName")]
        public virtual string AlbumName { get; set; }

        public Dictionary<string, List<string>> ExtendedMetadata
        {
            get
            {
                if (_extendedMetadata == null)
                {
                    RefreshExtendedMetadata();
                }

                return _extendedMetadata;
            }
            set
            {
                _extendedMetadata = value;
                ExtendedMetadataJson = JsonConvert.SerializeObject(_extendedMetadata);
            }
        }

        public Artist Artist
        {
            get
            {
                if (_artist == null)
                {
                    _artist = new Artist(ArtistName);
                }

                return _artist;
            }
        }

        public Album Album
        {
            get
            {
                if (_album == null)
                {
                    _album = new Album(AlbumName);
                }

                return _album;
            }
        }

        public bool IsAudioCdTrack
        {
            get
            {
                return Path.GetExtension(FileName).Equals(".cda", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void RefreshExtendedMetadata()
        {
            _extendedMetadata = (string.IsNullOrEmpty(_extendedMetadataJson) ?
                new Dictionary<string, List<string>>() :
                JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(_extendedMetadataJson));
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", ArtistName, Title);
        }
    }
}
