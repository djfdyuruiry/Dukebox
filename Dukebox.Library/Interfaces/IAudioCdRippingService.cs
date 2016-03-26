using System;
using System.Threading.Tasks;

namespace Dukebox.Library.Interfaces
{
    public interface IAudioCdRippingService
    {
        Task RipCdToFolder(string inPath, string outPath, ICdRipViewUpdater viewUpdater);
    }
}
