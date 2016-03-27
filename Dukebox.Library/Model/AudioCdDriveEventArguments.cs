using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library.Model
{
    public class AudioCdDriveEventArguments : EventArgs
    {
        public string DriveDirectory { get; set; }

        public List<ITrack> CdTracks { get; set; }

        public AudioCdDriveEventArguments() : base()
        {

        }
    }
}
