using System;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IHelpMenuViewModel
    {
        ICommand About { get; }
    }
}
