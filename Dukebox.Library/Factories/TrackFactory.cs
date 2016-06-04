using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Services;
using System;

namespace Dukebox.Library.Factories
{
    public class TrackFactory
    {
        private readonly IDukeboxSettings _settings;

        public TrackFactory(IDukeboxSettings settings)
        {
            _settings = settings;
        }

        public ITrack BuildTrackInstance(Song song, IAudioFileMetadata audioFileMetadata = null)
        {
            if (song == null)
            {
                throw new ArgumentException("Cannot build track instance with a null song instance");
            }

            return new Track(song, _settings, audioFileMetadata);
        }

        public ITrack BuildTrackInstance(string fileName, IAudioFileMetadata audioFileMetadata = null)
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
