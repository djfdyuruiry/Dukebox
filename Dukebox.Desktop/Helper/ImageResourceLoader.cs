using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dukebox.Desktop.Helper
{
    public static class ImageResourceLoader
    {
        public static ImageSource LoadImageFromResourceUri(string resourceUri)
        {
            var uri = new Uri(resourceUri, UriKind.RelativeOrAbsolute);
            return LoadImageFromResourceUri(uri);
        }

        public static ImageSource LoadImageFromResourceUri(Uri resourceUri)
        {
            var image = new BitmapImage();

            image.BeginInit();
            image.StreamSource = Application.GetResourceStream(resourceUri).Stream;
            image.EndInit();

            return image;
        }
    }
}
