using System;

namespace Dukebox.Library.Model
{
    public class WatchFolderEvent
    {
        private const int trailingReportCharsToPrint = 50;

        public WatchFolderEventType EventType { get; set; }
        public DirectoryImportReport ImportReport { get; set; }
        public AudioFileImportedInfo AudioFileImportInfo { get; set; }

        public string GetReportString()
        {
            string msg;

            if (EventType == WatchFolderEventType.DirectoryImport)
            {
                msg = $"Imported {ImportReport.NumberOfFilesAdded} songs from directory {TruncatePath(ImportReport.DirectoryPath)}";
            }
            else
            {
                msg = $"{AudioFileImportInfo.Status} file {TruncatePath(AudioFileImportInfo.FileAdded)}";
            }

            return $"{ImportReport?.ImportDateTime ?? DateTime.UtcNow} - {msg}";
        }


        private string TruncatePath(string path)
        { 
            var truncatedpath = path.Substring(Math.Max(0, path.Length - trailingReportCharsToPrint));

            return path.Length > truncatedpath.Length ? $"...{truncatedpath}" : path;
        }
    }
}
