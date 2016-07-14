using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using Dukebox.Desktop.Model;

namespace Dukebox.Desktop.Views
{
    /// <summary>
    /// Interaction logic for InitalImportWindow.xaml
    /// </summary>
    public partial class InitalImportWindow : Window
    {
        public InitalImportWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<NotificationMessage>(this, (nm) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (nm.Notification == NotificationMessages.IntialImportWindowShouldClose)
                    {
                        Close();
                    }
                });
            });
        }
    }
}
