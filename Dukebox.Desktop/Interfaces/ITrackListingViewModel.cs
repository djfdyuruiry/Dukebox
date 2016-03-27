using Dukebox.Desktop.Model;
using Dukebox.Desktop.ViewModel;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface ITrackListingViewModel : ISearchControlViewModel
    {
        List<ITrack> Tracks { get; }
        bool EditingListingsDisabled { get; }
        ICommand LoadTrack { get; }
    }
}
