namespace Dukebox.Library
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class album
    {
        public album()
        {
            songs = new HashSet<song>();
        }

        [Key]
        public long id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string name { get; set; }

        public virtual ICollection<song> songs { get; set; }
    }
}
