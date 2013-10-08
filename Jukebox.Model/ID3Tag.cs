using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jukebox.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ID3Tag
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }

        // Prevent empty instantiaton.
        private ID3Tag()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mp3FileName"></param>
        public static ID3Tag ID3TagFromMP3File(string mp3FileName)
        {
            return ID3TagFromNonMP3(mp3FileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ID3Tag ID3TagFromNonMP3(string fileName)
        {
            string title = string.Empty;
            string artist = string.Empty;
            string album = string.Empty;

            return new ID3Tag() { Title = title, Artist = artist, Album = album };
        }
    }
}
