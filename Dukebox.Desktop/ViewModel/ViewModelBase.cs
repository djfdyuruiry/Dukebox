﻿using System.ComponentModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace Dukebox.Desktop.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            Task.Run(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        } 

        protected void SendNotificationMessage(string message)
        {
            var messageObj = new NotificationMessage(this, message);

            Task.Run(() => Messenger.Default.Send(messageObj));
        }
    }
}
