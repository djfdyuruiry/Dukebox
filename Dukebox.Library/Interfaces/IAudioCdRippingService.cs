using System;

namespace Dukebox.Library.Interfaces
{
    public interface IAudioCdRippingService
    {
        void RipCdToFolder(string inPath, string outPath, ICdRipViewUpdater viewUpdater);
    }
}
