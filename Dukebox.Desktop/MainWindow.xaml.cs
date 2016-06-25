using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;

namespace Dukebox.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            (DataContext as IMainWindowViewModel)?.ShowLoadingScreen?.Execute(null);

            Messenger.Default.Register<NotificationMessage>(this, (nm) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (nm.Notification == NotificationMessages.LoadingFinished)
                    {
                        Show();
                    }
                });
            });
        }
    }
}
