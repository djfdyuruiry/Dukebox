using System.Collections.Generic;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface ITrack
    {
        Album Album { get; }
        Artist Artist { get; }
        string ArtistName { get; set; }
        string AlbumName { get; set; }
        string Title { get; set; }
        Dictionary<string, List<string>> ExtendedMetadata { get; set; }
        IAudioFileMetadata Metadata { get; }
        Song Song { get; set; }
        void CopyDetailsToAudioMetadata(IAudioFileMetadata metadata);
        void SyncMetadata(IMusicLibraryUpdateService updateService);
        string ToString();
    }
}
