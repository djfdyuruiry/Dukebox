using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Dukebox.Library.Model
{
    [Table("playlists")]
    public class Playlist
    {
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [StringLength(2147483647)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [StringLength(2147483647)]
        [Column("filenamesCsv")]
        public string FilenamesCsv { get; set; }
        /// <summary>
        /// List of files associated with this playlist.
        /// </summary>
        public List<string> Files
        {
            get
            {
                return FilenamesCsv.Split(',').ToList();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
