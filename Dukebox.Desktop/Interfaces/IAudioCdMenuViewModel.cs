using System;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IAudioCdMenuViewModel
    {
        ICommand PlayAudioCd { get; }
        ICommand RipCdToFolder { get; }
    }
}
