using Dukebox.Library;
using Dukebox.Library.Config;
using Dukebox.Library.Repositories;
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
        /// <summary>
        /// Details specific to track file.
        /// </summary>
        public song Song { get; set; }

        /// <summary>
        /// Associated artist information and accessor.
        /// </summary>
        private artist _artist;
        public artist Artist
        {
            get
            {
                if (Song.artistId.HasValue && Song.artistId != -1 && _artist == null)
                {
                    _artist = MusicLibrary.GetInstance().GetArtistById(Song.artistId.Value); 
                }

                return _artist;
            }
            set
            {
                if (value.id < 1 || value.id > MusicLibrary.GetInstance().GetArtistCount())
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
        private album _album;
        public album Album
        {
            get
            {
                if (Song.albumId.HasValue && Song.albumId != -1 && _album == null)
                {
                    _album = MusicLibrary.GetInstance().GetAlbumById(Song.albumId.Value);
                }

                return _album;
            }
            set
            {
                if (value.id < 1 || value.id > MusicLibrary.GetInstance().GetAlbumCount())
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
        private AudioFileMetaData _metadata;     
        public AudioFileMetaData Metadata 
        {
            get
            {
                if (_metadata == null)
                {
                    _metadata = new AudioFileMetaData(Song.filename, Album.id);
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

        /// <summary>
        /// Audio track information in "$artist - $title" format.
        /// </summary>
        /// <returns>A string describing this audio track.</returns>
        public override string ToString()
        {
            var trackFormat = DukeboxSettings.Settings["trackDisplayFromat"].ToString().ToLower();

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
