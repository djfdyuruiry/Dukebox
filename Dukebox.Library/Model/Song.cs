using Dukebox.Library.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Dukebox.Library.Model
{
    public class Song
    {
        public long id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string filename { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string title { get; set; }

        public long lengthInSeconds { get; set; }

        public long? albumId { get; set; }

        public long? artistId { get; set; }

        public virtual Album album { get; set; }

        public virtual Artist artist { get; set; }
        public override string ToString()
        {
            var musicLibrary = LibraryPackage.GetInstance<IMusicLibrary>();
            var displayTitle = title == string.Empty ? "Unknown Title" : title;
            var displayArtist = "Unknown Artist";

            if (artistId != null && artistId != -1)
            {
                displayArtist = musicLibrary.GetArtistById((long)artistId).ToString();
            }

            return string.Format("{0} - {1}", displayArtist, displayTitle);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(string))
            {
                return (string)obj == ToString();
            }

            return base.Equals(obj);
        }
    }
}
