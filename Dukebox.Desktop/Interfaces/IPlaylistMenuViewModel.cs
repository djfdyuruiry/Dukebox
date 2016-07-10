using System;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IPlaylistMenuViewModel
    {
        ICommand Clear { get; }
        ICommand SaveCurrentPlaylistToLibrary { get; }
        ICommand LoadFromFile { get; }
        ICommand SaveToFile { get; }
        ICommand ImportPlaylistToLibrary { get; }
        bool SaveToFileEnabled { get; }
    }
}
