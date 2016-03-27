using Dukebox.Library.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library.Interfaces
{
    public interface ITrack : IEqualityComparer
    {
        album Album { get; set; }
        artist Artist { get; set; }
        AudioFileMetadata Metadata { get; set; }
        song Song { get; set; }
        string ToString();
    }
}
