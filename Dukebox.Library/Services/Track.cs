using System;
using System.Collections;
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
        
        public Song Song { get; set; }
        
        public Artist Artist
        {
            get
            {
                if (Song.ArtistId.HasValue && Song.ArtistId != -1 && _artist == null)
                {
                    _artist = _musicLibrary.GetArtistById(Song.ArtistId.Value); 
                }

                return _artist;
            }
            set
            {
                if (value.Id <= _musicLibrary.GetArtistCount())
                {
                    _artist = value;
                }
                else if (value.Id != -1)
                {
                    throw new ArgumentOutOfRangeException(string.Format("The artist id '{0}' is invalid!", value));
                }
            }
        }
        
        public Album Album
        {
            get
            {
                if (Song.AlbumId.HasValue && Song.AlbumId != -1 && _album == null)
                {
                    _album = _musicLibrary.GetAlbumById(Song.AlbumId.Value);
                }

                return _album;
            }
            set
            {
                if (value.Id <= _musicLibrary.GetAlbumCount())
                {
                    _album = value;
                }
                else if (value.Id != -1)
                {
                    throw new ArgumentOutOfRangeException(string.Format("The album id '{0}' is invalid!", value.Id));
                }
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
            set
            {
                if (Song.Id == -1)
                {
                    _metadata = value;
                }
                else
                {
                    throw new InvalidOperationException("Can't manually set the property 'Metadata' because this song's metadata is maintained in the database");
                }
            }
        }

        public static ITrack BuildTrackInstance(Song song)
        {
            if (song == null)
            {
                throw new ArgumentException("Cannot build track instance with a null song instance");
            }

            var track = LibraryPackage.GetInstance<ITrack>() as Track;

            track._album = song.Album ?? new Album { Id = -1 };
            track._artist = song.Artist ?? new Artist { Id = -1 };
            track.Song = song;

            return track as ITrack;
        }

        public static ITrack BuildTrackInstance(string fileName, IAudioFileMetadata audioFileMetadata = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Cannot build track instance with a null or empty file name");
            }

            var song = new Song() { Id = -1, AlbumId = -1, ArtistId = -1, FileName = fileName };             
            var track = BuildTrackInstance(song) as Track;
            
            track.Song.Title = track.Metadata.Title;
            track._album.Name = track.Metadata.Album;
            track._artist.Name = track.Metadata.Artist;

            return track as ITrack;
        }

        public Track(IDukeboxSettings settings, IMusicLibrary musicLibrary)
        {
            _settings = settings;
            _musicLibrary = musicLibrary;
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
