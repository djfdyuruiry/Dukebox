using System;
using System.Collections.Generic;
using Dukebox.Library.Interfaces;

namespace Dukebox.Library.Model
{
    public class AudioCdDriveEventArgs : EventArgs
    {
        public string DriveDirectory { get; set; }

        public List<ITrack> CdTracks { get; set; }

        public AudioCdDriveEventArgs() : base()
        {

        }
    }
}
