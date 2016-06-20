using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.Services
{
    public class TrackWrapper : INotifyPropertyChanged
    {
        private static event EventHandler<string> TrackInstanceChanged; 
        
        private readonly IMusicLibrary _musicLibrary;

        public event PropertyChangedEventHandler PropertyChanged;

        public ITrack Data { get; private set; }

        public TrackWrapper(IMusicLibrary musicLibrary, ITrack track)
        {
            _musicLibrary = musicLibrary;

            Data = track;

            TrackInstanceChanged += (o, e) =>
            {
                if (o != this && e == Data.Song.FileName)
                {
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
                return Data.Song.Title;
            }
            set
            {
                Data.Song.Title = value;
                OnPropertyChanged("Title");

                PropagateTrackChanges();
            }
        }

        public string ArtistName
        {
            get
            {
                return Data.Artist.Name;
            }
            set
            {
                Data.Song.ArtistName = value;
                OnPropertyChanged("ArtistName");

                PropagateTrackChanges();
            }
        }

        public string AlbumName
        {
            get
            {
                return Data.Album.Name;
            }
            set
            {
                Data.Song.AlbumName = value;
                OnPropertyChanged("AlbumName");

                PropagateTrackChanges();
            }
        }

        public Dictionary<string, List<string>> ExtendedMetadata
        {
            get
            {
                return Data.Song.ExtendedMetadata;
            }
            set
            {
                Data.Song.ExtendedMetadata = value;
                OnPropertyChanged("ExtendedMetadata");

                PropagateTrackChanges();
            }
        }

        private void PropagateTrackChanges()
        {
            Data.SyncMetadata(_musicLibrary);

            TrackInstanceChanged?.Invoke(this, Data.Song.FileName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
