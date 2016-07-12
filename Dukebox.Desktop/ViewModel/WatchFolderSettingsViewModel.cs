using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
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
        private readonly IMusicLibraryEventService _eventService;
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
        public ICommand UpdateWatchFolder { get; private set; }
        public ICommand DeleteWatchFolder { get; private set; }

        public WatchFolderSettingsViewModel(IWatchFolderManagerService watchFolderManager, IMusicLibraryUpdateService updateService,
            IMusicLibraryEventService eventService)
        {
            _watchFolderManager = watchFolderManager;
            _updateService = updateService;
            _eventService = eventService;
            _selectFolderDialog = new FolderBrowserDialog();

            _selectFolderDialog.Description = "Select a folder to watch";

            AddWatchFolder = new RelayCommand(DoAddWatchFolder);
            DeleteWatchFolder = new RelayCommand<WatchFolderWrapper>(DoDeleteWatchFolder);
            UpdateWatchFolder = new RelayCommand<WatchFolderWrapper>(DoUpdateWatchFolder);

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

        private void DoUpdateWatchFolder(WatchFolderWrapper watchFolderToUpdate)
        {
            var result = _selectFolderDialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            watchFolderToUpdate.FolderPath = _selectFolderDialog.SelectedPath;

            UpdateWatchFolders();
        }

        private void DoDeleteWatchFolder(WatchFolderWrapper watchFolderToDelete)
        {
            _watchFolderManager.StopManagingWatchFolder(watchFolderToDelete.Data);
            UpdateWatchFolders();
        }

        private void UpdateWatchFolders()
        {
            WatchFolders = _watchFolderManager
                .WatchFolders
                .Select(wfs => new WatchFolderWrapper(wfs.WatchFolder, _updateService, _eventService))
                .ToList();
        }
    }
}
