using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dukebox.Library.Model
{
    [Table("albums")]
    public class Album
    {
        public Album()
        {
            Songs = new HashSet<Song>();
        }

        [Column("id")]
        public long Id { get; set; }

        [Required]
        [StringLength(2147483647)]
        [Column("name")]
        public string Name { get; set; }
        public int hasAlbumArt { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
        public bool HasAlbumArt
        {
            get
            {
                return hasAlbumArt == 0 ? false : true;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
