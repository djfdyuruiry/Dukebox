using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dukebox.Library.Helper;

namespace Dukebox.Library.Model
{
    [Table("watchFolders")]
    public class WatchFolder
    {

        [Key]
        [Index(IsUnique = true)]
        [Column("folderPath")]
        public string FolderPath { get; set; }
        
        [Column("lastScanTimestamp")]
        public long LastScanTimestamp { get; set; }

        [NotMapped]
        public DateTime LastScanDateTime
        {
            get
            {
                return UnixTimestampHelper.UnixTimestampToDateTime(LastScanTimestamp);
            }
            set
            {
                LastScanTimestamp = UnixTimestampHelper.DateTimeToUnixTimestamp(value);
            }
        }
    }
}
