using System.Windows;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.Interfaces
{
    public interface ITrackListingViewModel : ISearchControlViewModel
    {
        VirtualizingObservableCollection<ITrack> Tracks { get; }
        bool EditingListingsDisabled { get; }
        ICommand LoadTrack { get; }
        ICommand EditTrack { get; }
        Visibility ShowSearchControl { get; }
    }
}
