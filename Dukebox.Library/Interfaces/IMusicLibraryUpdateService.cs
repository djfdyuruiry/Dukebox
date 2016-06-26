using System.Threading.Tasks;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryUpdateService
    {
        Task RemoveTrack(ITrack track);
        Task SaveSongChanges(Song song);
    }
}
