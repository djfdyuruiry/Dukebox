using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IPlaylistGeneratorService
    {
        Playlist GetPlaylistFromFile(string playlistFile);
    }
}
