using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Views;

namespace Dukebox.Desktop.ViewModel
{
    public class SettingsMenuViewModel : ViewModelBase, ISettingsMenuViewModel
    {
        public ICommand TrackColumnsSettings { get; private set; }

        public SettingsMenuViewModel()
        {
            TrackColumnsSettings = new RelayCommand(ShowTrackColumnsSettingsWindow);
        }

        private void ShowTrackColumnsSettingsWindow()
        {
            var metadataSettingsWindow = new MetadataColumnsSettings();
            metadataSettingsWindow.Show();            
        }
    }
}
