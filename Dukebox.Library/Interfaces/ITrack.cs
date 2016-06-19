using System;
using System.ComponentModel;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface ITrack : INotifyPropertyChanged
    {
        event EventHandler MetadataChangesSaved;
        Album Album { get; }
        Artist Artist { get; }
        IAudioFileMetadata Metadata { get; }
        Song Song { get; }
        void CopyDetailsToAudioMetadata(IAudioFileMetadata metadata);
        string ToString();
    }
}
