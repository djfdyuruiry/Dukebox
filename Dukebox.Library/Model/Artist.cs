using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dukebox.Library.Model
{
    [Table("artists")]
    public class Artist
    {
        public Artist()
        {
            Songs = new HashSet<Song>();
        }

        [Column("id")]
        public long Id { get; set; }

        [Required]
        [StringLength(2147483647)]
        [Column("name")]
        public string Name { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
        public override string ToString()
        {
            return Name;
        }        
    }
}
