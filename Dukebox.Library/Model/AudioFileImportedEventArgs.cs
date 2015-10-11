using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library.Model
{
    /// <summary>
    /// Arguments in the event of an audio file being imported
    /// or being pre-processed.
    /// </summary>
    public class AudioFileImportedEventArgs : EventArgs
    {
        /// <summary>
        /// Is the track going through pre-processing?
        /// </summary>
        public bool JustProcessing { get; set; }

        /// <summary>
        /// The absolute filename that was added to the library.
        /// </summary>
        public string FileAdded { get; set; }

        /// <summary>
        /// Total files that are scheduled for this events parent operation.
        /// </summary>
        public int TotalFilesThisImport { get; set; }
    }
}
