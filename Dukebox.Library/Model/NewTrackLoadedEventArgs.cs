using Dukebox.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library.Model
{
    public class NewTrackLoadedEventArgs : EventArgs
    {
        public Track Track { get; set; }
        public int TrackIndex { get; set; }
    }
}
