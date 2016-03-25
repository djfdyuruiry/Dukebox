using System.ComponentModel.DataAnnotations;

namespace Dukebox.Library
{
    public partial class song
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

        public virtual album album { get; set; }

        public virtual artist artist { get; set; }
    }
}
