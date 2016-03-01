using System;
using System.Windows.Input;
using System.Windows.Media;

namespace Dukebox.Desktop.Interfaces
{
    public interface IPlaybackMonitorViewModel
    {
        string Album { get; }
        ImageSource AlbumArt { get; }
        string Artist { get; }
        ICommand BackCommand { get; }
        ICommand ForwardCommand { get; }
        ICommand PlayPauseCommand { get; }
        ImageSource PlayPauseImage { get; }
        ICommand StopCommand { get; }
        string Track { get; }
        string TrackMinutesPassed { get; }
        string TrackMinutesTotal { get; }
    }
}
