using Dukebox.Desktop.Model;
using Dukebox.Desktop.Services;
using Dukebox.Library;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IAlbumListingViewModel : ISearchControlViewModel
    {
        List<Album> Albums { get; }
        ICommand LoadAlbum { get; }
    }
}
