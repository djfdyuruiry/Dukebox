using Dukebox.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Track
    {
        public song Song { get; set; }

        private artist _artist;
        public artist Artist
        {
            get
            {
                if (Song.artistId.HasValue && _artist == null)
                {
                    _artist = MusicLibrary.GetInstance().Artists.OrderBy(a => a.id).ToArray()[Song.artistId.Value - 1]; 
                }

                return _artist;
            }
            set
            {
                if (value.id < 1 || value.id > MusicLibrary.GetInstance().Artists.Count())
                {
                    _artist = value;
                }

                throw new ArgumentOutOfRangeException("The artist id '" + value.id + "' is invalid!");
            }
        }

        private album _album;
        public album Album
        {
            get
            {
                if (Song.albumId.HasValue && _album == null)
                {
                    _album = MusicLibrary.GetInstance().Albums.OrderBy(a => a.id).ToArray()[Song.albumId.Value - 1];
                }

                return _album;
            }
            set
            {
                if (value.id < 1 || value.id > MusicLibrary.GetInstance().Albums.Count())
                {
                    _album = value;
                }

                throw new ArgumentOutOfRangeException("The album id '" + value.id + "' is invalid!");
            }
        }

        private AudioFileMetaData _metadata;     
        public AudioFileMetaData Metadata 
        {
            get
            {
                if (_metadata == null)
                {
                    _metadata = new AudioFileMetaData(Song.filename);
                }

                return _metadata;
            }
            set
            {
                if (Song.id == -1)
                {
                    _metadata = value;
                }

                throw new InvalidOperationException("Cannot manually set the property 'Metadata' if the song id is not '-1'!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Song.artistId != null && Song.artistId == -1)
            {
                return (Artist.name == string.Empty ? "Unknown Artist" : Artist.name) + " - " + Song.ToString();
            }

            return Song.ToString();
        }
    }
}
