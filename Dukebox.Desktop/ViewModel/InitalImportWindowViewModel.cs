using System;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;

namespace Dukebox.Desktop.ViewModel
{
    public class InitalImportWindowViewModel : ViewModelBase, IInitalImportWindowViewModel
    {
        private const string importFolderDialogDescription = "Select a folder to import into your music library";

        private readonly IDukeboxUserSettings _userSettings;
        private readonly FolderBrowserDialog _selectMusicFolderDialog;

        private string _initalImportPath;
        private string _notificationText;
        private int _currentProgressValue;
        private int _maximumProgressValue;
        private bool _importStarted;
        private string _statusText;

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

        public int CurrentProgressValue
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

        public int MaximumProgressValue
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

        public InitalImportWindowViewModel(IDukeboxUserSettings userSettings)
        {
            var userMusicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

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
            // TODO: Import Logic

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
