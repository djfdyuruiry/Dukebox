using System;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IPlaybackMenuViewModel
    {
        ICommand Repeat { get; }
        ICommand RepeatAll { get; }
        ICommand Shuffle { get; }
        bool ShuffleOn { get; set; }
        bool RepeatOn { get; set; }
        bool RepeatAllOn { get; set; }
    }
}
