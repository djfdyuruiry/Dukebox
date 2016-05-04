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
                if (Song.artistId.HasValue && Song.artistId != -1 && _artist == null)
                {
                    _artist = _musicLibrary.GetArtistById(Song.artistId.Value); 
                }

                return _artist;
            }
            set
            {
                if (value.id <= _musicLibrary.GetArtistCount())
                {
                    _artist = value;
                }
                else if (value.id != -1)
                {
                    throw new ArgumentOutOfRangeException(string.Format("The artist id '{0}' is invalid!", value));
                }
            }
        }
        
        public Album Album
        {
            get
            {
                if (Song.albumId.HasValue && Song.albumId != -1 && _album == null)
                {
                    _album = _musicLibrary.GetAlbumById(Song.albumId.Value);
                }

                return _album;
            }
            set
            {
                if (value.id <= _musicLibrary.GetAlbumCount())
                {
                    _album = value;
                }
                else if (value.id != -1)
                {
                    throw new ArgumentOutOfRangeException(string.Format("The album id '{0}' is invalid!", value.id));
                }
            }
        }
        
        public IAudioFileMetadata Metadata 
        {
            get
            {
                if (_metadata == null)
                {
                    _metadata = AudioFileMetadata.BuildAudioFileMetaData(Song.filename);
                }

                return _metadata;
            }
            set
            {
                if (Song.id == -1)
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

            track._album = song.album ?? new Album { id = -1 };
            track._artist = song.artist ?? new Artist { id = -1 };
            track.Song = song;

            return track as ITrack;
        }

        public static ITrack BuildTrackInstance(string fileName, IAudioFileMetadata audioFileMetadata = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Cannot build track instance with a null or empty file name");
            }

            var song = new Song() { id = -1, albumId = -1, artistId = -1, filename = fileName };             
            var track = BuildTrackInstance(song) as Track;
            
            track.Song.title = track.Metadata.Title;
            track._album.name = track.Metadata.Album;
            track._artist.name = track.Metadata.Artist;

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
                trackFormat = trackFormat.Replace("{artist}", Artist.name);
            }

            if (Album == null)
            {
                trackFormat = trackFormat.Replace("{album}", "Unknown Album");
            }
            else
            {
                trackFormat = trackFormat.Replace("{album}", Album.name);
            }

            trackFormat = trackFormat.Replace("{filename}", Song.filename);
            trackFormat = trackFormat.Replace("{title}", Song.title);

            return trackFormat;
        }
        
        public override bool Equals(object otherTrack)
        {
            var otherTrackObj = otherTrack as ITrack;

            if (otherTrackObj == null)
            {
                return false;
            }

            return GetHashCode() == otherTrack.GetHashCode();
        }

        public override int GetHashCode()
        {
            return (int)Song.id;
        }
    }
}
