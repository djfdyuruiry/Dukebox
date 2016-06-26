using System.Collections.Generic;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface ITrackGeneratorService
    {
        List<ITrack> GetTracksForDirectory(string directory, bool subDirectories);
        List<ITrack> GetTracksForPlaylist(Playlist playlist);
    }
}
