namespace Dukebox.Library
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class playlist
    {
        [Key]
        public long id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string name { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string filenamesCsv { get; set; }
    }
}
