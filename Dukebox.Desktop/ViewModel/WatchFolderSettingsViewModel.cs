using Dukebox.Desktop.Interfaces;
using System.Collections.Generic;
using Dukebox.Library.Model;
using System.Windows.Input;
using Dukebox.Library.Interfaces;
using GalaSoft.MvvmLight.Command;
using System.Windows.Forms;

namespace Dukebox.Desktop.ViewModel
{
    public class WatchFolderSettingsViewModel : ViewModelBase, IWatchFolderSettingsViewModel
    {
        private readonly IWatchFolderManagerService _watchFolderManager;
        private readonly FolderBrowserDialog _selectFolderDialog;

        private List<WatchFolder> _watchFolders;

        public List<WatchFolder> WatchFolders
        {
            get
            {
                return _watchFolders;
            }
            set
            {
                _watchFolders = value;
                OnPropertyChanged("WatchFolders");
            }
        }

        public ICommand AddWatchFolder { get; private set; }

        public ICommand DeleteWatchFolder { get; private set; }

        public WatchFolderSettingsViewModel(IWatchFolderManagerService watchFolderManager)
        {
            _watchFolderManager = watchFolderManager;
            _selectFolderDialog = new FolderBrowserDialog();

            _selectFolderDialog.Description = "Select a folder to watch";

            AddWatchFolder = new RelayCommand(DoAddWatchFolder);
            DeleteWatchFolder = new RelayCommand<WatchFolder>(DoDeleteWatchFolder);
        }

        private void DoAddWatchFolder()
        {
            var result = _selectFolderDialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            var watchFolder = new WatchFolder
            { 
                FolderPath = _selectFolderDialog.SelectedPath
            };

            _watchFolderManager.ManageWatchFolder(watchFolder);
        }

        private void DoDeleteWatchFolder(WatchFolder watchFolderToDelete)
        {
            _watchFolderManager.StopManagingWatchFolder(watchFolderToDelete);
        }
    }
}
