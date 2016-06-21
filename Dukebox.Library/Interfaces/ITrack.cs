using System;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface ITrack
    {
        Album Album { get; }
        Artist Artist { get; }
        IAudioFileMetadata Metadata { get; }
        Song Song { get; }
        void CopyDetailsToAudioMetadata(IAudioFileMetadata metadata);
        void SyncMetadata(IMusicLibrary musicLibrayToUpdate);
        string ToString();
    }
}
