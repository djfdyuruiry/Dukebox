using Dukebox.Desktop.Model;
using Dukebox.Library;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IAlbumListingViewModel : ISearchControlViewModel
    {
        List<album> Albums { get; }
    }
}
