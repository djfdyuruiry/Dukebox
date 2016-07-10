using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Desktop.Services
{
    public class PlaylistWrapper : INotifyPropertyChanged
    {
        private readonly IMusicLibraryRepository _musicRepo;
        private readonly IMusicLibraryUpdateService _updateService;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public Playlist Data { get; private set; }

        public string Name
        {
            get
            {
                return Data.Name;
            }
            set
            {
                Data.Name = value;
                OnPropertyChanged(nameof(Name));

                SavePlaylistChanges();
            }
        }

        public PlaylistWrapper(IMusicLibraryEventService eventService, IMusicLibraryRepository musicRepo, 
            IMusicLibraryUpdateService updateService, Playlist playlist)
        {
            Data = playlist;
            _musicRepo = musicRepo;
            _updateService = updateService;

            eventService.PlaylistCacheRefreshed += (o, e) => UpdatePlaylist();
        }

        private void UpdatePlaylist()
        {
            try
            {
                Data = _musicRepo.GetPlaylistById(Data.Id);
                OnPropertyChanged(nameof(Name));
            }
            catch (Exception)
            {
                // Playlist wrapper playlist has been detached from the DB
            }
        }

        private void SavePlaylistChanges()
        {
            Task.Run(() => _updateService.SavePlaylistChanges(Data));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
