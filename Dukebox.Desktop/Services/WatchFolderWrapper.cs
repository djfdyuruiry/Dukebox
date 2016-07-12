using System;
using System.ComponentModel;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Desktop.Services
{
    public class WatchFolderWrapper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IMusicLibraryUpdateService _updateService;

        public WatchFolder Data { get; private set; }

        public string FolderPath
        {
            get
            {
                return Data.FolderPath;
            }
            set
            {
                Data.FolderPath = value;
                OnPropertyChanged(nameof(FolderPath));

                PropagateWatchFolderChanges();
            }
        }

        public DateTime LastScanDateTime
        {
            get
            {
                return Data.LastScanDateTime;
            }
        }

        public WatchFolderWrapper(WatchFolder data, IMusicLibraryUpdateService updateService, 
            IMusicLibraryEventService eventService)
        {
            Data = data;
            _updateService = updateService;

            eventService.WatchFolderUpdated += (o, e) => UpdateWatchFolder(e);
        }

        private void UpdateWatchFolder(WatchFolder watchFolderToUpdate)
        {
            if (!watchFolderToUpdate.FolderPath.Equals(Data.FolderPath))
            {
                return;
            }

            Data = watchFolderToUpdate;
            OnPropertyChanged(nameof(FolderPath));
            OnPropertyChanged(nameof(LastScanDateTime));
        }

        private void PropagateWatchFolderChanges()
        {
            _updateService.SaveWatchFolderChanges(Data);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
