using Dukebox.Desktop.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface ITrackListingViewModel : ISearchControlViewModel
    {
        List<TrackWrapper> Tracks { get; }
        bool EditingListingsDisabled { get; }
        ICommand LoadTrack { get; }
        Visibility ShowSearchControl { get; }
    }
}
