using Dukebox.Library.Model;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Dukebox.Desktop.Helper
{
    public class ArtistToCardPathConverter : IValueConverter
    {
        private const string artistCardDirectory = "artistCards";
        private const string unknownArtistCardFilename = "unknown";

        private static readonly Regex validArtistCardCharacterRegex = new Regex(@"[a-zA-Z0-9]");
        private static readonly string artistCardDirectoryPath = Path.Combine(Environment.CurrentDirectory, artistCardDirectory);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var artist = value as Artist;

            if (artist == null)
            {
                throw new InvalidOperationException("Parameter 'value' was not of type Dukebox.Library.Model.Artist");
            }
            else if (string.IsNullOrWhiteSpace(artist.Name))
            {
                throw new InvalidDataException("Name property of value parameter is null or whitespace");
            }

            var upperFirstLetter = artist.Name.Substring(0, 1).ToUpper();
            var filename = validArtistCardCharacterRegex.IsMatch(upperFirstLetter) ? upperFirstLetter : unknownArtistCardFilename;
            var file = string.Format("{0}.png", filename);

            var path = Path.Combine(artistCardDirectoryPath, file);

            return path;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
