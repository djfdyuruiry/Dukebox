using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library.Model
{
    public class NewTrackLoadedEventArgs : EventArgs
    {
        public ITrack Track { get; set; }
        public int TrackIndex { get; set; }
    }
}
