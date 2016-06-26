using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dukebox.Library.Model
{
    [Table("watchFolders")]
    public class WatchFolder
    {
        [Required]
        [Index(IsUnique = true)]
        [Column("folderPath")]
        public string FolderPath { get; set; }
        
        [Column("lastScanTimestamp")]
        public int LastScanTimestamp { get; set; }

        public DateTime LastScanDateTime
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds(LastScanTimestamp).DateTime;
            }
        }
    }
}
