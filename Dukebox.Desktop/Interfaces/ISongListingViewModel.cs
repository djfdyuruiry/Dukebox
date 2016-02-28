using Dukebox.Desktop.Model;
using Dukebox.Desktop.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface ISongListingViewModel : ISearchControlViewModel
    {
        List<Song> Songs { get; }
        bool EditingListingsDisabled { get; }
    }
}
