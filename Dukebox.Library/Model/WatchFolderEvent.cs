using System;
using Dukebox.Library.Helper;

namespace Dukebox.Library.Model
{
    public class WatchFolderEvent
    {
        public WatchFolderEventType EventType { get; set; }
        public DirectoryImportReport ImportReport { get; set; }
        public AudioFileImportedInfo AudioFileImportInfo { get; set; }

        public string GetReportString()
        {
            string msg;

            if (EventType == WatchFolderEventType.DirectoryImport)
            {
                msg = $"Imported {ImportReport.NumberOfFilesAdded} songs from directory {TruncatePathHelper.TruncatePath(ImportReport.DirectoryPath)}";
            }
            else
            {
                msg = $"{AudioFileImportInfo.Status} file {TruncatePathHelper.TruncatePath(AudioFileImportInfo.FileAdded)}";
            }

            return $"{ImportReport?.ImportDateTime ?? DateTime.UtcNow} - {msg}";
        }
    }
}
