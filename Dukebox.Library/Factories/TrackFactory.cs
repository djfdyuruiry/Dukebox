using System;
using Dukebox.Configuration.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Services;

namespace Dukebox.Library.Factories
{
    public class TrackFactory
    {
        public static event EventHandler TrackMetadataUpdated;

        private readonly IDukeboxSettings _settings;
        private readonly AudioFileMetadataFactory _audioFileMetadataFactory;

        public IMusicLibraryQueueService MusicLibraryQueueService { get; set; }

        public TrackFactory(IDukeboxSettings settings, AudioFileMetadataFactory audioFileMetadataFactory)
        {
            _settings = settings;
            _audioFileMetadataFactory = audioFileMetadataFactory;

            Track.TrackMetadataUpdated += (o, e) => TrackMetadataUpdated?.Invoke(o, e); 
        }

        public ITrack BuildTrackInstance(Song song)
        {
            return BuildTrackInstance(song, null);
        }

        public ITrack BuildTrackInstance(Song song, IAudioFileMetadata audioFileMetadata)
        {
            if (song == null)
            {
                throw new ArgumentException("Cannot build track instance with a null song instance");
            }

            return new Track(song, _settings, MusicLibraryQueueService, _audioFileMetadataFactory, audioFileMetadata);
        }

        public ITrack BuildTrackInstance(string fileName)
        {
            return BuildTrackInstance(fileName, null);
        }

        public ITrack BuildTrackInstance(string fileName, IAudioFileMetadata audioFileMetadata)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Cannot build track instance with a null or empty file name");
            }

            var song = new Song() { Id = -1, AlbumId = -1, ArtistId = -1, FileName = fileName };

            return BuildTrackInstance(song, audioFileMetadata);
        }
    }
}
