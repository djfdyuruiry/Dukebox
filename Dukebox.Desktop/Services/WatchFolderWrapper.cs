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
                OnPropertyChanged("FolderPath");

                PropagateWatchFolderChanges();
            }
        }

        public WatchFolderWrapper(WatchFolder data, IMusicLibraryUpdateService updateService)
        {
            Data = data;
            _updateService = updateService;
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
