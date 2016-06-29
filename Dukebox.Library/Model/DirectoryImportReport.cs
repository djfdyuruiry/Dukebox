using System;

namespace Dukebox.Library.Model
{
    public class DirectoryImportReport
    {
        public string DirectoryPath { get; set; }
        public int NumberOfFilesAdded { get; set; }
        public int NumberOfMissingFilesRemoved { get; set; }
        public DateTime ImportDateTime { get; set; }

        public DirectoryImportReport()
        {
            ImportDateTime = DateTime.UtcNow;
        }
    }
}
