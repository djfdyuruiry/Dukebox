using System;
using System.Windows.Threading;
using AlphaChiTech.Virtualization;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;

namespace Dukebox.Desktop
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            InitialiseCollectionVirtualizationManager();

            var viewModel = DataContext as IMainWindowViewModel;
            viewModel?.ShowLoadingScreen?.Execute(null);

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

        private void InitialiseCollectionVirtualizationManager()
        {
            if (!VirtualizationManager.IsInitialized)
            {
                VirtualizationManager.Instance.UIThreadExcecuteAction = a => Dispatcher.Invoke(a);

                new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, delegate
                {
                    VirtualizationManager.Instance.ProcessActions();
                }, Dispatcher).Start();
            }
        }
    }
}
