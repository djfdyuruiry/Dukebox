using Dukebox.Desktop.Model;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IAlbumListingViewModel : ISearchControlViewModel
    {
        List<Album> Albums { get; }
    }
}
