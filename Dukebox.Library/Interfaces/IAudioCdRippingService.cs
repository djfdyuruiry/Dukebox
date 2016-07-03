using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dukebox.Library.Interfaces
{
    public interface IAudioCdRippingService
    {
        Task RipCdToFolder(string inPath, string outPath, ICdRipViewUpdater viewUpdater);
        Task RipCdToFolder(string inPath, string outPath, ICdRipViewUpdater viewUpdater, List<ITrack> customTracks);
    }
}
