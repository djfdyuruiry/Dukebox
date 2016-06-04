using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface ITrack
    {
        Album Album { get; }
        Artist Artist { get; }
        IAudioFileMetadata Metadata { get; }
        Song Song { get; }
        string ToString();
    }
}
