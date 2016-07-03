using System;
using Dukebox.Library.Interfaces;

namespace Dukebox.Library.Model
{
    public class NewTrackLoadedEventArgs : EventArgs
    {
        public ITrack Track { get; set; }
        public int TrackIndex { get; set; }
    }
}
