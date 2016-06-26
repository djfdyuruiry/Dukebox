using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Services;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Desktop.ViewModel
{
    public class WatchFolderSettingsViewModel : ViewModelBase, IWatchFolderSettingsViewModel
    {
        private readonly IWatchFolderManagerService _watchFolderManager;
        private readonly IMusicLibraryUpdateService _updateService;
        private readonly FolderBrowserDialog _selectFolderDialog;

        private List<WatchFolderWrapper> _watchFolders;

        public List<WatchFolderWrapper> WatchFolders
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

        public WatchFolderSettingsViewModel(IWatchFolderManagerService watchFolderManager, IMusicLibraryUpdateService updateService)
        {
            _watchFolderManager = watchFolderManager;
            _updateService = updateService;
            _selectFolderDialog = new FolderBrowserDialog();

            _selectFolderDialog.Description = "Select a folder to watch";

            AddWatchFolder = new RelayCommand(DoAddWatchFolder);
            DeleteWatchFolder = new RelayCommand<WatchFolder>(DoDeleteWatchFolder);

            UpdateWatchFolders();
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
            UpdateWatchFolders();
        }

        private void DoDeleteWatchFolder(WatchFolder watchFolderToDelete)
        {
            _watchFolderManager.StopManagingWatchFolder(watchFolderToDelete);
            UpdateWatchFolders();
        }

        private void UpdateWatchFolders()
        {
            WatchFolders = _watchFolderManager
                .WatchFolders
                .Select(wfs => new WatchFolderWrapper(wfs.WatchFolder, _updateService))
                .ToList();
        }
    }
}
