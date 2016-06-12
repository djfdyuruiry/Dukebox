using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dukebox.Library.Model
{
    [Table("songs")]
    public class Song
    {
        private string _extendedMetadataJson;
        private Dictionary<string, List<string>> _extendedMetadata;

        [Column("id")]
        public long Id { get; set; }

        [Required]
        [StringLength(2147483647)]
        [Column("fileName")]
        public string FileName { get; set; }

        [Required]
        [StringLength(2147483647)]
        [Column("title")]
        public string Title { get; set; }

        [Column("lengthInSeconds")]
        public long LengthInSeconds { get; set; }

        [Column("albumId")]
        public long? AlbumId { get; set; }

        [Column("artistId")]
        public long? ArtistId { get; set; }

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

        [ForeignKey("AlbumId")]
        public virtual Album Album { get; set; }

        [ForeignKey("ArtistId")]
        public virtual Artist Artist { get; set; }

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
        }

        private void RefreshExtendedMetadata()
        {
            _extendedMetadata = string.IsNullOrEmpty(_extendedMetadataJson) ?
                new Dictionary<string, List<string>>() :
                JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(_extendedMetadataJson);
        }

        public override string ToString()
        {
            var displayTitle = string.IsNullOrEmpty(Title) ? "Unknown Title" : Title;
            var displayArtist = string.IsNullOrEmpty(Artist?.Name) ? "Unknown Artist" : Artist?.Name;

            return string.Format("{0} - {1}", displayArtist, displayTitle);
        }
    }
}
