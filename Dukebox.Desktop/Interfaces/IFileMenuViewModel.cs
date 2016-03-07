using System;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IFileMenuViewModel
    {
        ICommand AddFilesToLibrary { get; }
        ICommand Exit { get; }
        ICommand ExportLibrary { get; }
        ICommand ImportLibrary { get; }
        ICommand PlayFile { get; }
        ICommand PlayFolder { get; }
    }
}
