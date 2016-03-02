using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Audio.Model
{
    public class TrackLoadedFromFileEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public MediaPlayerMetadata Metadata { get; set; }
    }
}
