using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using log4net;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library.Helper;
using Dukebox.Desktop.Views;

namespace Dukebox.Desktop.ViewModel
{
    public class LoadingScreenViewModel : ViewModelBase, ILoadingScreenViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DukeboxInitialisationHelper _initHelper;
        private readonly IDukeboxUserSettings _userSettings;

        private string _notification;

        public string Notification
        {
            get
            {
                return _notification;
            }
            private set
            {
                _notification = value;

                logger.Info(Notification);
                OnPropertyChanged("Notification");
            }
        }

        public ICommand LoadComponents { get; }

        public LoadingScreenViewModel(DukeboxInitialisationHelper DukeboxInitialisationHelper, IDukeboxUserSettings userSettings)
        {
            _initHelper = DukeboxInitialisationHelper;
            _userSettings = userSettings;

            LoadComponents = new RelayCommand(() => Task.Run(() => DoLoadComponents()));
        }
        
        private void DoLoadComponents()
        {
            var loadOk = true;

            try
            {
                var programLoadStopwatch = Stopwatch.StartNew();

                Notification = "Loading BASS audio...";
                _initHelper.InitaliseAudioLibrary();

                Notification = "Registering supported audio formats...";
                _initHelper.RegisterSupportedAudioFileFormats();

                programLoadStopwatch.Stop();

                logger.InfoFormat("Dukebox loaded int {0}ms", programLoadStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                loadOk = false;

                logger.Error("Error loading Dukebox components", ex);

                MessageBox.Show(ex.ToString(), "Error loading Dukebox components",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if(!loadOk)
            {
                Application.Current.Shutdown();
            }

            if (!_userSettings.InitalImportHasBeenShown)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var initalImportWindow = new InitalImportWindow();
                    initalImportWindow.Show();
                });
            }

            SendNotificationMessage(NotificationMessages.LoadingScreenShouldHide);

            if (_userSettings.InitalImportHasBeenShown)
            {
                SendNotificationMessage(NotificationMessages.LoadingFinished);
            }

            SendNotificationMessage(NotificationMessages.LoadingScreenShouldClose);
        }
    }
}
