using System;
using System.Windows.Input;
using System.Windows.Media;

namespace Dukebox.Desktop.Interfaces
{
    public interface IPlaybackMonitorViewModel
    {
        string Album { get; }
        string Artist { get; }
        string Track { get; }
        string TrackMinutesPassed { get; }
        string TrackMinutesTotal { get; }
        ImageSource AlbumArt { get; }
        ImageSource PlayPauseImage { get; }
        ICommand BackCommand { get; }
        ICommand ForwardCommand { get; }
        ICommand PlayPauseCommand { get; }
        ICommand StopCommand { get; }
    }
}
