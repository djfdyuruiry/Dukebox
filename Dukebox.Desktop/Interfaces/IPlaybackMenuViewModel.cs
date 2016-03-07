using System;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IPlaybackMenuViewModel
    {
        ICommand Repeat { get; }
        ICommand RepeatAll { get; }
        ICommand Shuffle { get; }
        bool ShuffleOn { get; }
        bool RepeatOn { get; }
        bool RepeatAllOn { get; }
    }
}
