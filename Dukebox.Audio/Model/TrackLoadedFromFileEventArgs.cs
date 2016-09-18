using System;

namespace Dukebox.Audio.Model
{
    public class TrackLoadedFromFileEventArgs : EventArgs
    {
        public string FileName { get; set; }
    }
}
