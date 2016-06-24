using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface ISettingsMenuViewModel
    {
        ICommand TrackColumnsSettings { get; }
    }
}
