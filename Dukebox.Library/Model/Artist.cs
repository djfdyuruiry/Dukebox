using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dukebox.Library.Model
{
    public class Artist
    {
        public Artist()
        {
            songs = new HashSet<Song>();
        }

        public long id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string name { get; set; }

        public virtual ICollection<Song> songs { get; set; }
        public override string ToString()
        {
            return name;
        }        
    }
}
