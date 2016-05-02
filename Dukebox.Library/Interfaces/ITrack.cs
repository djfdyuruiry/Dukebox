using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface ITrack
    {
        Album Album { get; set; }
        Artist Artist { get; set; }
        IAudioFileMetadata Metadata { get; set; }
        Song Song { get; set; }
        string ToString();
        bool Equals(object otherTrack);
        int GetHashCode();
    }
}
