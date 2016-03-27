using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Linq;

namespace Dukebox.Library.Model
{
    public class Playlist
    {
        public long id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string name { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string filenamesCsv { get; set; }
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

        public List<ITrack> GetTracksForPlaylist()
        {
            var musicLibrary = LibraryPackage.GetInstance<IMusicLibrary>();
            var tracks = Enumerable.Empty<ITrack>();
            var nonLibraryFiles = new List<string>();

            foreach (var file in Files)
            {
                var foundTracks = musicLibrary.GetTracksByAttributeValue(SearchAreas.Filename, file);

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
