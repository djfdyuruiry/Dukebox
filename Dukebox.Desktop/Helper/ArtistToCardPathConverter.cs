using Dukebox.Library.Model;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Dukebox.Desktop.Helper
{
    public class ArtistToCardPathConverter : IValueConverter
    {
        private const string artistCardDirectory = "artistCards";
        private const string unknownArtistCardFilename = "unknown.png";

        private static readonly string artistCardDirectoryPath = Path.Combine(Environment.CurrentDirectory, artistCardDirectory);
        private static readonly string unknownArtistCardPath = Path.Combine(artistCardDirectoryPath, unknownArtistCardFilename);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var artist = value as Artist;

            if (artist == null)
            {
                throw new InvalidOperationException("'value' parameter was not of type Dukebox.Library.Model.Artist");
            }

            var name = artist.Name;

            if(string.IsNullOrEmpty(name))
            {
                return unknownArtistCardPath;
            }

            var upperFirstLetter = name.Substring(0, 1).ToUpper();
            var file = string.Format("{0}.png", upperFirstLetter);
            var path = Path.Combine(artistCardDirectoryPath, file);

            return path;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
