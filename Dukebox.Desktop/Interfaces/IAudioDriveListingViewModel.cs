using System.Collections.Generic;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IAudioDriveListingViewModel
    {
        List<string> AudioCdDrivePaths { get; }
        ICommand PlayCd { get; }
        ICommand RipCd { get; }
        string SelectedAudioCdDrivePath { set; }
    }
}
