using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using log4net;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using System.Threading;
using Dukebox.Desktop.Model;
using Dukebox.Library.Helper;

namespace Dukebox.Desktop.ViewModel
{
    public class LoadingScreenViewModel : ViewModelBase, ILoadingScreenViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DukeboxInitialisationHelper _initHelper;

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

        public LoadingScreenViewModel(DukeboxInitialisationHelper DukeboxInitialisationHelper)
        {
            _initHelper = DukeboxInitialisationHelper;
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

            SendNotificationMessage(NotificationMessages.LoadingScreenShouldHide);
            SendNotificationMessage(NotificationMessages.LoadingFinished);
            SendNotificationMessage(NotificationMessages.LoadingScreenShouldClose);
        }
    }
}
