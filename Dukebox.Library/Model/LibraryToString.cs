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

            return MusicLibrary.GetInstance().GetArtistById((long)artistId) + " - " + title;
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

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(string))
            {
                return (string)obj == ToString();
            }

            return base.Equals(obj);
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

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(string))
            {
                return (string)obj == ToString();
            }

            return base.Equals(obj);
        }
    }
}
