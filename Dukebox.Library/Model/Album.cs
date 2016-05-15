using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dukebox.Library.Model
{
    public class Album
    {
        public Album()
        {
            songs = new HashSet<Song>();
        }

        public long id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string name { get; set; }
        public int hasAlbumArt { get; set; }

        public virtual ICollection<Song> songs { get; set; }
        public bool HasAlbumArt
        {
            get
            {
                return hasAlbumArt == 0 ? false : true;
            }
        }

        public override string ToString()
        {
            return name;
        }
    }
}
