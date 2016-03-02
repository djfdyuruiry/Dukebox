using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dukebox.Desktop.Helper
{
    public class ImageToImageSourceConverter : IValueConverter
    {
        public ImageSource Convert(Image value)
        {
            return Convert(value, value.GetType(), null, CultureInfo.InvariantCulture) as ImageSource;
        }

        public Image Convert(ImageSource value)
        {
            return Convert(value, value.GetType(), null, CultureInfo.InvariantCulture) as Image;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var image = value as Image;
            var bitmapImage = new BitmapImage();
            var memoryStream = new MemoryStream();

            bitmapImage.BeginInit();

            image.Save(memoryStream, ImageFormat.Bmp);
            memoryStream.Seek(0, SeekOrigin.Begin);

            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imageSource = value as BitmapImage;
            var imageStream = imageSource.StreamSource;

            return Image.FromStream(imageStream);
        }
    }
}
