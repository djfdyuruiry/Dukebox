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
        public artist Artist { get; set; }
        public album Album { get; set; }

        public AudioFileMetaData Metadata { get; set; }

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
