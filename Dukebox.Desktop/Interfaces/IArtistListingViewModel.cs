using Dukebox.Desktop.Model;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IArtistListingViewModel : ISearchControlViewModel
    {
        List<Artist> Artists { get; }
    }
}
