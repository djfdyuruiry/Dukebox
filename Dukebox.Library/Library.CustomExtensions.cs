using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Repositories;
using Dukebox.Model.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library
{
    public partial class song
    {
        public override string ToString()
        {
            var musicLibrary = LibraryPackage.GetInstance<IMusicLibrary>();
            var displayTitle = title == string.Empty ? "Unknown Title" : title;
            var displayArtist = "Unknown Artist";

            if (artistId != null && artistId != -1)
            {
                displayArtist = musicLibrary.GetArtistById((long)artistId).ToString();
            }

            return string.Format("{0} - {1}", displayArtist, displayTitle);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(string))
            {
                return (string)obj == ToString();
            }

            return base.Equals(obj);
        }
    }

    public partial class artist
    {
        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(string))
            {
                return (string)obj == ToString();
            }

            return base.Equals(obj);
        }
    }

    public partial class album
    {
        public bool HasAlbumArt 
        {
            get
            {
                return hasAlbumArt == 0 ? false : true;
            }
        }

        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(string))
            {
                return (string)obj == ToString();
            }

            return base.Equals(obj);
        }
    }

    public partial class playlist
    {
        /// <summary>
        /// List of files associated with this playlist.
        /// </summary>
        public List<string> Files
        {
            get
            {
                return filenamesCsv.Split(',').ToList();
            }
        }

        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(string))
            {
                return (string)obj == ToString();
            }

            return base.Equals(obj);
        }

        public List<Track> GetTracksForPlaylist()
        {
            var musicLibrary = LibraryPackage.GetInstance<IMusicLibrary>();
            var tracks = Enumerable.Empty<Track>();
            var nonLibraryFiles = new List<string>();

            foreach (var file in Files)
            {
                var foundTracks = musicLibrary.GetTracksByAttribute(SearchAreas.Filename, file);

                if (foundTracks.Count > 0)
                {
                    tracks = tracks.Concat(foundTracks);
                }
                else
                {
                    nonLibraryFiles.Add(file);
                }
            }

            tracks = tracks.Concat(nonLibraryFiles.Select(file => 
            {
                try
                {
                    return musicLibrary.GetTrackFromFile(file);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error while loading track from file '{0}' for playlist {1}", file, ToString()), ex);
                }
            }));

            return tracks.ToList();
        }
    }
}
