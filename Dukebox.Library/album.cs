using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dukebox.Library
{
    public partial class album
    {
        public album()
        {
            songs = new HashSet<song>();
        }

        public long id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string name { get; set; }
        public int hasAlbumArt { get; set; }

        public virtual ICollection<song> songs { get; set; }
    }
}
