using System.Collections.Generic;
using System.Windows.Input;
using Dukebox.Desktop.Services;

namespace Dukebox.Desktop.Interfaces
{
    public interface IPlaylistListingViewModel
    {
        ICommand LoadPlaylist { get; }
        ICommand RemovePlaylist { get; }
        List<PlaylistWrapper> Playlists { get; }
    }
}
