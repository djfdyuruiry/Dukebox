using Dukebox.Library.Model;
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
        Album Album { get; set; }
        Artist Artist { get; set; }
        IAudioFileMetadata Metadata { get; set; }
        Song Song { get; set; }
        string ToString();
    }
}
