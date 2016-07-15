using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library.Helper;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Desktop.ViewModel
{
    public class InitalImportWindowViewModel : ViewModelBase, IInitalImportWindowViewModel
    {
        private const string importFolderDialogDescription = "Select a folder to import into your music library";

        private readonly IMusicLibraryImportService _importService;
        private readonly IWatchFolderManagerService _watchFolderManager;
        private readonly IDukeboxUserSettings _userSettings;
        private readonly FolderBrowserDialog _selectMusicFolderDialog;

        private string _initalImportPath;
        private string _notificationText;
        private double _currentProgressValue;
        private double _maximumProgressValue;
        private bool _importStarted;
        private string _statusText;
        private int _filesImported;

        public string ImportPath
        {
            get
            {
                return _initalImportPath;
            }
            private set
            {
                _initalImportPath = value;

                OnPropertyChanged(nameof(ImportPath));
            }
        }

        public string NotificationText
        {
            get
            {
                return _notificationText;
            }
            private set
            {
                _notificationText = value;

                OnPropertyChanged(nameof(NotificationText));
            }
        }

        public double CurrentProgressValue
        {
            get
            {
                return _currentProgressValue;
            }
            private set
            {
                _currentProgressValue = value;

                OnPropertyChanged(nameof(CurrentProgressValue));
            }
        }

        public double MaximumProgressValue
        {
            get
            {
                return _maximumProgressValue;
            }
            private set
            {
                _maximumProgressValue = value;

                OnPropertyChanged(nameof(MaximumProgressValue));
            }
        }

        public bool ImportHasNotStarted
        {
            get
            {
                return !_importStarted;
            }
            private set
            {
                _importStarted = !value;

                OnPropertyChanged(nameof(ImportHasNotStarted));
            }
        }

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            private set
            {
                _statusText = value;

                OnPropertyChanged(nameof(StatusText));
            }
        }

        public ICommand SelectImportPath { get; private set; }

        public ICommand Import { get; private set; }

        public ICommand SkipImport { get; private set; }

        public InitalImportWindowViewModel(IMusicLibraryImportService importService, IWatchFolderManagerService watchFolderService, 
            IDukeboxUserSettings userSettings)
        {
            var userMusicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

            _importService = importService;
            _watchFolderManager = watchFolderService;
            _userSettings = userSettings;

            _selectMusicFolderDialog = new FolderBrowserDialog
            {
                Description = importFolderDialogDescription,
                SelectedPath = userMusicFolder
            };

            ImportPath = userMusicFolder;

            CurrentProgressValue = 0;
            MaximumProgressValue = 100;

            SelectImportPath = new RelayCommand(DoSelectImportPath);
            Import = new RelayCommand(DoInitalImport);
            SkipImport = new RelayCommand(FinishInitalImport);
        }

        private void DoSelectImportPath()
        {
            var dialogResult = _selectMusicFolderDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            ImportPath = _selectMusicFolderDialog.SelectedPath;
        }

        private void DoInitalImport()
        {
            ImportHasNotStarted = false;
            NotificationText = "0% - Preparing to Import...";

            _importService.AddSupportedFilesInDirectory(ImportPath, true, ImportProgressHandler, ImportCompleteHandler);
        }

        private void ImportProgressHandler(AudioFileImportedInfo importInfo)
        {
            MaximumProgressValue = importInfo.TotalFilesThisImport;


            var percentComplete = ((double)_filesImported / (MaximumProgressValue / 100));

            NotificationText = $"{percentComplete:G3}% - {importInfo.Status} {TruncatePathHelper.TruncatePath(importInfo.FileAdded)}";

            Interlocked.Increment(ref _filesImported);
            CurrentProgressValue = _filesImported;

            if (percentComplete > 99.9)
            {
                NotificationText = "100% - Processing Album Art...";
            }
        }

        private void ImportCompleteHandler(DirectoryImportReport importReport)
        {
            var watchFolder = new WatchFolder
            {
                FolderPath = ImportPath,
                LastScanDateTime = DateTime.UtcNow
            };

            _watchFolderManager.ManageWatchFolder(watchFolder);
            FinishInitalImport();
        }

        private void FinishInitalImport()
        {
            _userSettings.InitalImportHasBeenShown = true;

            SendNotificationMessage(NotificationMessages.LoadingFinished);
            SendNotificationMessage(NotificationMessages.IntialImportWindowShouldClose);
        }
    }
}
