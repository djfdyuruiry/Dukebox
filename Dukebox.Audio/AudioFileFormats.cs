using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dukebox.Audio
{
    public class AudioFileFormats
    {
        private const string fileFormatsNotLoadedErrorMessage = 
            "Please wait until FormatsLoaded has fired before using FileSupported or FileFormatSupported methods";

        public const string FileFilterPrefixFormat = "Audio Files |{0}";
        
        public List<string> SupportedFormats { get; private set; }
        
        public string FileFilter
        {
            get
            {
                if (!SupportedFormats.Any())
                {
                    return "*.*";
                }

                string formatList = SupportedFormats.Aggregate((a, b) => a + b).Replace(".", ";*.");
                string filter = formatList.Substring(1);

                return filter;
            }
        }
        
        public string FileDialogFilter
        {
            get
            {
                return string.Format(FileFilterPrefixFormat, FileFilter);
            }
        }

        public bool FileFormatsHaveBeenLoaded { get; private set; }

        public event EventHandler FormatsLoaded;

        public void SignalFormatsHaveBeenLoaded()
        {
            FileFormatsHaveBeenLoaded = true;
            FormatsLoaded?.Invoke(this, EventArgs.Empty);
        }

        public AudioFileFormats()
        {
            SupportedFormats = new List<string>();
        }

        public bool FileSupported(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return false;
            }

            if (!FileFormatsHaveBeenLoaded)
            {
                throw new InvalidOperationException(fileFormatsNotLoadedErrorMessage);
            }

            var fileExtension = Path.GetExtension(filename);
            return FileFormatSupported(fileExtension);
        }

        public bool FileFormatSupported(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
            {
                return false;
            }

            if (!FileFormatsHaveBeenLoaded)
            {
                throw new InvalidOperationException(fileFormatsNotLoadedErrorMessage);
            }

            return SupportedFormats.Any(f => f.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }
    } 
}
