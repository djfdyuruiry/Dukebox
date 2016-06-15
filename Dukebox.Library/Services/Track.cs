using System;
using System.Globalization;
using System.IO;
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
        private readonly IMusicLibraryQueueService _musicLibraryQueueService;
        private readonly AudioFileMetadataFactory _audioFileMetadataFactory;

        private readonly Album _album;
        private readonly Artist _artist;

        private IAudioFileMetadata _metadata;

        public event EventHandler MetadataChangesSaved;

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

        public Track(Song song, IDukeboxSettings settings, IMusicLibraryQueueService musicLibraryQueueService, AudioFileMetadataFactory audioFileMetadataFactory) : 
            this(song, settings, musicLibraryQueueService, audioFileMetadataFactory, null)
        {            
        }

        public Track(Song song, IDukeboxSettings settings, IMusicLibraryQueueService musicLibraryQueueService, AudioFileMetadataFactory audioFileMetadataFactory, IAudioFileMetadata audioFileMetadata)
        {
            if (song == null)
            {
                throw new ArgumentException("Cannot build track instance with a null song instance");
            }

            _settings = settings;
            _musicLibraryQueueService = musicLibraryQueueService;
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

            if (!Path.GetExtension(song.FileName).Equals(".cda", StringComparison.OrdinalIgnoreCase))
            {
                Song.TitleUpdated += (o, e) => SaveMetadataChanges();
                Artist.NameUpdated += (o, e) => SaveMetadataChanges();
                Album.NameUpdated += (o, e) => SaveMetadataChanges();
            }
        }

        private void SaveMetadataChanges()
        {
            Task.Run(() =>
            {
                CopyDetailsToAudioMetadata(Metadata);
                Metadata.SaveMetadataToFileTag();

                if (Song.Id != -1 || Artist.Id != -1 || Album.Id != -1)
                {
                    _musicLibraryQueueService.QueueMusicLibrarySaveChanges();
                }

                MetadataChangesSaved?.Invoke(this, EventArgs.Empty);
            });
        }

        public void CopyDetailsToAudioMetadata(IAudioFileMetadata metadata)
        {
            metadata.Album = Album.Name;
            metadata.Artist = Artist.Name;
            metadata.Title = Song.Title;
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
