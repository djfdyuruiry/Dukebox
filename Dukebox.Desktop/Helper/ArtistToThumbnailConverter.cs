using Dukebox.Desktop.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Interop;
using Int32Rect = System.Windows.Int32Rect;

namespace Dukebox.Desktop.Helper
{
    public class ArtistToThumbnailConverter : IValueConverter
    {
        public readonly static Size DefaultThumbnailSize = new Size(128, 128);
        private readonly static PointF ArtistLetterPosition = new PointF(25, 25);
        private readonly static Font ArtistLetterFont = new Font(FontFamily.GenericSerif, 50);
        public readonly static string UnkownArtistLetter = "?";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var artist = value as Artist;
            // return a blank thumbnail if artist is null
            var artistLetter = string.Empty;

            if (artist != null)
            {
                artistLetter = string.IsNullOrEmpty(artist.Title) ? UnkownArtistLetter : artist.Title.Substring(0, 1).ToUpper(); 
            }

            var thumbnail = new Bitmap(DefaultThumbnailSize.Width, DefaultThumbnailSize.Height);

            using (var thumbnailGraphics = Graphics.FromImage(thumbnail))
            {
                thumbnailGraphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(new Point(0, 0), thumbnail.Size));

                thumbnailGraphics.DrawString(artistLetter, 
                    ArtistLetterFont, 
                    new SolidBrush(Color.Black),
                    ArtistLetterPosition);
            }

            return Imaging.CreateBitmapSourceFromHBitmap(thumbnail.GetHbitmap(System.Drawing.Color.Transparent),
                IntPtr.Zero,
                new Int32Rect(0, 0, thumbnail.Width, thumbnail.Height),
                null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
