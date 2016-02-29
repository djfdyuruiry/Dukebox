using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Audio
{
    /// <summary>
    /// 
    /// </summary>
    public class AudioFileFormats
    {
        /// <summary>
        /// A string list of all supported audio
        /// formats specified by file extension. (e.g. '.mp3')
        /// </summary>
        public List<string> SupportedFormats = new List<string>();

        /// <summary>
        /// A file filter string for use with a file dialog
        /// class which represents all supported formats.
        /// </summary>
        public string FileDialogFilter
        {
            get
            {
                string formatList = SupportedFormats.Aggregate((a, b) => a + b).Replace(".", ";*.");
                string filter = "Audio Files |" + formatList.Substring(1);

                return filter;
            }
        }
    } 
}
