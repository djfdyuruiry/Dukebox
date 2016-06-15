using System;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface ITrack
    {
        event EventHandler MetadataChangesSaved;
        Album Album { get; }
        Artist Artist { get; }
        IAudioFileMetadata Metadata { get; }
        Song Song { get; }
        string ToString();
    }
}
