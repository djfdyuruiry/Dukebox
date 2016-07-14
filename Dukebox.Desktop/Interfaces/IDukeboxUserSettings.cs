using System.Collections.Generic;

namespace Dukebox.Desktop.Interfaces
{
    public interface IDukeboxUserSettings
    {
        bool Repeat { get; set; }
        bool RepeatAll { get; set; }
        bool Shuffle { get; set; }
        List<string> ExtendedMetadataColumnsToShow { get; set; }
        bool InitalImportHasBeenShown { get; set; }
    }
}
