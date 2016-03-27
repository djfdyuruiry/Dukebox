using Dukebox.Library.Model;
using System.Collections;

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
