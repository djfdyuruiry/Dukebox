﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Configuration.Interfaces;
using Dukebox.Library.Factories;

namespace Dukebox.Library.Services
{
    public class Track : ITrack
    {
        private readonly IDukeboxSettings _settings;
        private readonly AudioFileMetadataFactory _audioFileMetadataFactory;
        
        private IAudioFileMetadata _metadata;

        public Song Song { get; private set; }
        
        public Artist Artist
        {
            get
            {
                return Song.Artist;
            }
        }

        public string ArtistName
        {
            get
            {
                return Artist.Name;
            }
            set
            {
                Song.ArtistName = value;
            }
        }

        public Album Album
        {
            get
            {
                return Song.Album;
            }
        }

        public string AlbumName
        {
            get
            {
                return Album.Name;
            }
            set
            {
                Song.AlbumName = value;
            }
        }

        public IAudioFileMetadata Metadata 
        {
            get
            {
                if (_metadata == null)
                {
                    _metadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(Song.FileName);
                }

                return _metadata;
            }
        }

        public Track(Song song, IDukeboxSettings settings, AudioFileMetadataFactory audioFileMetadataFactory) : 
            this(song, settings, audioFileMetadataFactory, null)
        {            
        }

        public Track(Song song, IDukeboxSettings settings, AudioFileMetadataFactory audioFileMetadataFactory, IAudioFileMetadata audioFileMetadata)
        {
            if (song == null)
            {
                throw new ArgumentException("Cannot build track instance with a null song instance");
            }

            _settings = settings;
            _audioFileMetadataFactory = audioFileMetadataFactory;

            Song = song;

            _metadata = audioFileMetadata;

            if (song.Id == -1)
            {
                Song.Title = Metadata.Title;
                Song.ExtendedMetadata = Metadata.ExtendedMetadata;
            }

            if (string.IsNullOrEmpty(Song.AlbumName))
            {
                Song.AlbumName = Metadata.Album;
            }

            if (string.IsNullOrEmpty(Song.ArtistName))
            {
                Song.ArtistName = Metadata.Artist;
            }
        }
        

        public void CopyDetailsToAudioMetadata(IAudioFileMetadata metadata)
        {
            metadata.Album = Album.Name;
            metadata.Artist = Artist.Name;
            metadata.Title = Song.Title;
        }

        public void SyncMetadata(IMusicLibrary musicLibrayToUpdate)
        {
            if (Song.IsAudioCdTrack)
            {
                return;
            }

            Task.Run(() =>
            {
                CopyDetailsToAudioMetadata(Metadata);
                Metadata.SaveMetadataToFileTag();

                musicLibrayToUpdate.SaveDbChanges();
            });
        }

        public override string ToString()
        {
            var trackFormat = _settings.TrackDisplayFormat.ToLower(CultureInfo.InvariantCulture);

            if (Artist == null)
            {
                trackFormat = trackFormat.Replace("{artist}", "Unknown Artist");
            }
            else
            {
                trackFormat = trackFormat.Replace("{artist}", Artist.Name);
            }

            if (Album == null)
            {
                trackFormat = trackFormat.Replace("{album}", "Unknown Album");
            }
            else
            {
                trackFormat = trackFormat.Replace("{album}", Album.Name);
            }

            trackFormat = trackFormat.Replace("{filename}", Song.FileName);
            trackFormat = trackFormat.Replace("{title}", Song.Title);

            return trackFormat;
        }
    }
}
