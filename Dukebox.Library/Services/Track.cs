using System;
using System.Globalization;
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

        private readonly Album _album;
        private readonly Artist _artist;

        private IAudioFileMetadata _metadata;

        public Song Song { get; private set; }
        
        public Artist Artist
        {
            get
            {
                return Song.Artist ?? _artist;
            }
        }
        
        public Album Album
        {
            get
            {
                return Song.Album ?? _album;
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
            _album = Song.Album ?? new Album { Id = -1 };
            _artist = Song.Artist ?? new Artist { Id = -1 };

            _metadata = audioFileMetadata;

            if (song.Id == -1)
            {
                Song.Title = Metadata.Title;
                Song.ExtendedMetadata = Metadata.ExtendedMetadata;
            }

            if (_album.Id == -1)
            {
                _album.Name = Metadata.Album;
            }

            if (_artist.Id == -1)
            {
                _artist.Name = Metadata.Artist;
            }

            Song.TitleUpdated += (o, e) => SaveMetadataChangesToDisk();
            Artist.NameUpdated += (o, e) => SaveMetadataChangesToDisk();
            Album.NameUpdated += (o, e) => SaveMetadataChangesToDisk();
        }

        private void SaveMetadataChangesToDisk()
        {
            Metadata.Album = Album.Name;
            Metadata.Artist = Artist.Name;
            Metadata.Title = Song.Title;

            Metadata.SaveMetadataToFileTag();
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
