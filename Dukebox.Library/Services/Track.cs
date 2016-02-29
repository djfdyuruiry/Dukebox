using Dukebox.Library;
using Dukebox.Library.Config;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Repositories;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Model.Services
{
    /// <summary>
    /// An audio track, with source information and
    /// metadata properties.
    /// </summary>
    public class Track : IEqualityComparer
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IDukeboxSettings _settings;
        private IMusicLibrary _musicLibrary;
        private AudioFileMetaData _metadata;
        private album _album;
        private artist _artist;

        /// <summary>
        /// Details specific to track file.
        /// </summary>
        public song Song { get; set; }

        /// <summary>
        /// Associated artist information and accessor.
        /// </summary>
        public artist Artist
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
                if (value.id < 1 || value.id > _musicLibrary.GetArtistCount())
                {
                    _artist = value;
                }
                else if (value.id != -1)
                {
                    throw new ArgumentOutOfRangeException("The artist id '" + value.id + "' is invalid!");
                }
            }
        }

        /// <summary>
        /// Associated album information and accessor.
        /// </summary>
        public album Album
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
                if (value.id < 1 || value.id > _musicLibrary.GetAlbumCount())
                {
                    _album = value;
                }
                else if (value.id != -1)
                {
                    throw new ArgumentOutOfRangeException("The album id '" + value.id + "' is invalid!");
                }
            }
        }

        /// <summary>
        /// Metadata information and accessor.
        /// </summary>
        public AudioFileMetaData Metadata 
        {
            get
            {
                if (_metadata == null)
                {
                    _metadata = AudioFileMetaData.BuildAudioFileMetaData(Song.filename, Album.id);
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
                    throw new InvalidOperationException("Can only manually set the property 'Metadata' if the song is not stored in the database!");
                }
            }
        }

        public static Track BuildTrackInstance(album album, artist artist, song song)
        {
            var track = LibraryPackage.GetInstance<Track>();
            
            track.Album = album;
            track.Artist = artist;
            track.Song = song;

            return track;
        }

        public Track(IDukeboxSettings settings, IMusicLibrary musicLibrary)
        {
            _settings = settings;
            _musicLibrary = musicLibrary;
        }

        /// <summary>
        /// Audio track information in "$artist - $title" format.
        /// </summary>
        /// <returns>A string describing this audio track.</returns>
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

        /// <summary>
        /// Whether the tracks have the database song id.
        /// </summary>
        /// <param name="x">Track one.</param>
        /// <param name="y">Track two.</param>
        /// <returns>Are the two parameter tracks equal?</returns>
        bool IEqualityComparer.Equals(object x, object y)
        {
            Track a = (Track)x;
            Track b = (Track)y;

            return a.Song.id == b.Song.id;
        }

        /// <summary>
        /// Return the id of the song id in the database
        ///  for a track object as the hashcode.
        /// </summary>
        /// <param name="obj">The track object to extract id from.</param>
        /// <returns>The hashcode for this track.</returns>
        public int GetHashCode(object obj)
        {
            Track trackObj = (Track)obj;

            return (int)trackObj.Song.id;
        }
    }
}
