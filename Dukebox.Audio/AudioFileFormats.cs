﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        public const string FileFilterPrefixFormat = "Audio Files |{0}";

        /// <summary>
        /// A string list of all supported audio
        /// formats specified by file extension. (e.g. '.mp3')
        /// </summary>
        public List<string> SupportedFormats { get; private set; }

        /// <summary>
        /// A file filter string for use with a file dialog
        /// class which represents all supported formats.
        /// </summary>
        public string FileDialogFilter
        {
            get
            {
                if (!SupportedFormats.Any())
                {
                    return string.Format(FileFilterPrefixFormat, "*.*");
                }

                string formatList = SupportedFormats.Aggregate((a, b) => a + b).Replace(".", ";*.");
                string filter = string.Format(FileFilterPrefixFormat, formatList.Substring(1));

                return filter;
            }
        }

        public event EventHandler FormatsLoaded;

        public void SignalFormatsHaveBeenLoaded()
        {
            if (FormatsLoaded != null)
            {
                FormatsLoaded(this, EventArgs.Empty);
            }
        }

        public AudioFileFormats()
        {
            SupportedFormats = new List<string>();
        }

        public bool FileSupported(string filename)
        {
            var fileExtension = Path.GetExtension(filename);
            return FileFormatSupported(fileExtension);
        }

        public bool FileFormatSupported(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
            {
                return false;
            }

            return SupportedFormats.Any(f => f.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }
    } 
}
