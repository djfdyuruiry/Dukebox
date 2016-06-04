using Dukebox.Library.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dukebox.Library.Model
{
    [Table("songs")]
    public class Song
    {
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

        [ForeignKey("AlbumId")]
        public virtual Album Album { get; set; }

        [ForeignKey("ArtistId")]
        public virtual Artist Artist { get; set; }
        public override string ToString()
        {
            var displayTitle = string.IsNullOrEmpty(Title) ? "Unknown Title" : Title;
            var displayArtist = string.IsNullOrEmpty(Artist?.Name) ? "Unknown Artist" : Artist?.Name;

            return string.Format("{0} - {1}", displayArtist, displayTitle);
        }
    }
}
