using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Desktop.Services
{
    public class TrackWrapper : INotifyPropertyChanged
    {
        private static event EventHandler<Song> TrackInstanceChanged; 
        
        private readonly IMusicLibraryUpdateService _updateService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ITrack Data { get; private set; }

        public TrackWrapper(IMusicLibraryUpdateService updateService, ITrack track)
        {
            _updateService = updateService;

            Data = track;

            TrackInstanceChanged += (o, e) =>
            {
                if (o != this && e.FileName == Data.Song.FileName)
                {
                    Data.Song = e;

                    OnPropertyChanged("Title");
                    OnPropertyChanged("ArtistName");
                    OnPropertyChanged("AlbumName");
                    OnPropertyChanged("ExtendedMetadata");
                }
            };
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

            TrackInstanceChanged?.Invoke(this, Data.Song);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
