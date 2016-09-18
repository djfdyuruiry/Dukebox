using System.Collections.Generic;
using System.ComponentModel;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.Services
{
    public class TrackWrapper : INotifyPropertyChanged
    {
        private readonly IMusicLibraryUpdateService _updateService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ITrack Data { get; private set; }
        

        public TrackWrapper(IMusicLibraryUpdateService updateService, IMusicLibraryEventService eventService, ITrack track)
        {
            _updateService = updateService;
            
            Data = track;
        }
        
        public string Title
        {
            get
            {
                return Data.Title;
            }
            set
            {
                Data.Title = value;
                OnPropertyChanged("Title");

                PropagateTrackChanges();
            }
        }

        public string ArtistName
        {
            get
            {
                return Data.ArtistName;
            }
            set
            {
                Data.ArtistName = value;
                OnPropertyChanged("ArtistName");

                PropagateTrackChanges();
            }
        }

        public string AlbumName
        {
            get
            {
                return Data.AlbumName;
            }
            set
            {
                Data.AlbumName = value;
                OnPropertyChanged("AlbumName");

                PropagateTrackChanges();
            }
        }

        public Dictionary<string, List<string>> ExtendedMetadata
        {
            get
            {
                return Data.ExtendedMetadata;
            }
            set
            {
                Data.ExtendedMetadata = value;
                OnPropertyChanged("ExtendedMetadata");

                PropagateTrackChanges();
            }
        }

        private void PropagateTrackChanges()
        {
            Data.SyncMetadata(_updateService);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
