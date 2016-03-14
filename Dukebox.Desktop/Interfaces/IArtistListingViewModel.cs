using Dukebox.Desktop.Model;
using Dukebox.Library;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IArtistListingViewModel : ISearchControlViewModel
    {
        List<artist> Artists { get; }
        ICommand LoadArtist { get; }
    }
}
