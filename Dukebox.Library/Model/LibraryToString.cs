using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library
{
    /// <summary>
    /// 
    /// </summary>
    public partial class song
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (artistId == null)
            {
                return "Unknown Artist" + " - " + title == string.Empty ? "Unknown" : title;
            }
            else if (artistId == -1)
            {
                return title == string.Empty ? "Unknown" : title;
            }

            return MusicLibrary.GetInstance().Artists.First(a => a.id == artistId) + " - " + title;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class artist
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return name;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class album
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return name;
        }
    }
}
