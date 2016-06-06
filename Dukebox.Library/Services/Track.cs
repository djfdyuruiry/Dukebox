using System;
using System.Reflection;
using log4net;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services
{
    /// <summary>
    /// An audio track, with source information and
    /// metadata properties.
    /// </summary>
    public class Track : ITrack
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IDukeboxSettings _settings;
        private IMusicLibrary _musicLibrary;
        private IAudioFileMetadata _metadata;
        private Album _album;
        private Artist _artist;
        
        public Song Song { get; private set; }
        
        public Artist Artist
        {
            get
            {
                return Song.Artist;
            }
        }
        
        public Album Album
        {
            get
            {
                return Song.Album;
            }
        }
        
        public IAudioFileMetadata Metadata 
        {
            get
            {
                if (_metadata == null)
                {
                    _metadata = AudioFileMetadata.BuildAudioFileMetaData(Song.FileName);
                }

                return _metadata;
            }
        }

        public Track(Song song, IDukeboxSettings settings, IAudioFileMetadata audioFileMetadata = null)
        {
            if (song == null)
            {
                throw new ArgumentException("Cannot build track instance with a null song instance");
            }

            _settings = settings;

            Song = song;
            _album = Song.Album ?? new Album { Id = -1 };
            _artist = Song.Artist ?? new Artist { Id = -1 };

            _metadata = audioFileMetadata;

            if (song.Id == -1)
            {
                Song.Title = Metadata.Title;
            }

            if (_album.Id == -1)
            {
                _album.Name = Metadata.Album;
            }

            if (_artist.Id == -1)
            {
                _artist.Name = Metadata.Artist;
            }
        }

        public override string ToString()
        {
            var trackFormat = _settings.TrackDisplayFormat.ToLower();

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
