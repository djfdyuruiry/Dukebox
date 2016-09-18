using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Views;

namespace Dukebox.Desktop.ViewModel
{
    public class SettingsMenuViewModel : ViewModelBase, ISettingsMenuViewModel
    {
        public ICommand TrackColumnsSettings { get; private set; }
        public ICommand WatchFolderSettings { get; private set; }

        public SettingsMenuViewModel()
        {
            TrackColumnsSettings = new RelayCommand(ShowTrackColumnsSettingsWindow);
            WatchFolderSettings = new RelayCommand(ShowWatchFolderSettings);
        }

        private void ShowTrackColumnsSettingsWindow()
        {
            var metadataSettingsWindow = new MetadataColumnsSettings();
            metadataSettingsWindow.ShowDialog();
        }

        private void ShowWatchFolderSettings()
        {
            var watchFoldersSettingsWindow = new WatchFolderSettings();
            watchFoldersSettingsWindow.ShowDialog();
        }
    }
}
