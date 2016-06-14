using System.Drawing;
using System.Globalization;

namespace Dukebox.ArtistCardGenerator
{
    public static class Program
    {
        private static readonly Size DefaultThumbnailSize = new Size(194, 194);
        private static readonly PointF ArtistLetterPosition = new PointF(60, 55);
        private static readonly Font ArtistLetterFont = new Font(FontFamily.GenericSerif, 60);

        public static void Main(string[] args)
        {
            string alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";

            foreach (var c in alphabet)
            {
                var artistLetter = c.ToString().ToUpper(CultureInfo.InvariantCulture);

                using (var thumbnail = new Bitmap(DefaultThumbnailSize.Width, DefaultThumbnailSize.Height))
                {
                    using (var thumbnailGraphics = Graphics.FromImage(thumbnail))
                    {
                        thumbnailGraphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(new Point(0, 0), thumbnail.Size));

                        thumbnailGraphics.DrawString(artistLetter,
                            ArtistLetterFont,
                            new SolidBrush(Color.Black),
                            ArtistLetterPosition);
                    }

                    var fileName = string.Format("{0}.png", artistLetter);

                    thumbnail.Save(fileName);
                }
            }
        }
    }
}
