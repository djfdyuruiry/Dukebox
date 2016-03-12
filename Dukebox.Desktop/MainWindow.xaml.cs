using Dukebox.Desktop.Interfaces;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;

namespace Dukebox.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            (DataContext as IMainWindowViewModel).ShowLoadingScreen.Execute(null);

            Messenger.Default.Register<NotificationMessage>(this, (nm) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (nm.Notification == "LoadingFinished")
                    {
                        Show();
                    }
                });
            });
        }
    }
}
