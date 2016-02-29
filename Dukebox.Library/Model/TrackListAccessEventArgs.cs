using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library.Model
{
    public class TrackListAccessEventArgs : EventArgs
    {
        public int TrackListSize { get; set; }
    }
}
