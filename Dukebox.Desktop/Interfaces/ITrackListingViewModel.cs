using Dukebox.Desktop.Model;
using Dukebox.Desktop.ViewModel;
using Dukebox.Model.Services;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface ITrackListingViewModel : ISearchControlViewModel
    {
        List<Track> Tracks { get; }
        bool EditingListingsDisabled { get; }
    }
}
