using System;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Dukebox.Desktop.Interfaces;
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
                        try
                        {
                            Close();
                        }
                        catch (Exception)
                        {
                            // Window is closing already...
                        }
                    }
                });
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = DataContext as IInitalImportWindowViewModel;
            viewModel?.SkipImport?.Execute(null);
        }
    }
}
