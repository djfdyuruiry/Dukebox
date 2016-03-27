﻿using Dukebox.Library;
using Dukebox.Library.Config;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Repositories;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        private AudioFileMetadata _metadata;
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

        /// <summary>
        /// Metadata information and accessor.
        /// </summary>
        public AudioFileMetadata Metadata 
        {
            get
            {
                if (_metadata == null)
                {
                    _metadata = AudioFileMetadata.BuildAudioFileMetaData(Song.filename, Album.id);
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

        public static ITrack BuildTrackInstance(song song)
        {
            if (song == null)
            {
                throw new ArgumentException("Cannot build track instance with a null song instance");
            }

            var track = LibraryPackage.GetInstance<ITrack>();

            track.Album = song.album ?? new album { id = -1 };
            track.Artist = song.artist ?? new artist { id = -1 };
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
            ITrack a = (ITrack)x;
            ITrack b = (ITrack)y;

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
            ITrack trackObj = (ITrack)obj;

            return (int)trackObj.Song.id;
        }
    }
}
