using Dukebox.Desktop.Model;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;

namespace Dukebox.Desktop
{
    /// <summary>
    /// Interaction logic for LoadingScreen.xaml
    /// </summary>
    public partial class LoadingScreen : Window
    {
        public LoadingScreen()
        {
            InitializeComponent();

            Messenger.Default.Register<NotificationMessage>(this, (nm) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (nm.Notification == NotificationMessages.LoadingScreenShouldHide)
                    {
                        Hide();
                    }
                    else if (nm.Notification == NotificationMessages.LoadingScreenShouldClose)
                    {
                        Close();
                    }
                });
            });
        }
    }
}
