using System;
using System.Windows;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IMainWindowViewModel
    {
        ICommand NavBarItemClickCommand { get; }
        Visibility ShowAlbumListing { get; }
        Visibility ShowLibraryListing { get; }
        Visibility ShowArtistListing { get; }
        Visibility ShowRecentlyPlayedListing { get; }
        Visibility ShowAudioCdListing { get; }
    }
}
