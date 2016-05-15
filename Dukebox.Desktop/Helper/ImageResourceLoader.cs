using System;
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
            var image = new BitmapImage(resourceUri);
            image.CacheOption = BitmapCacheOption.None;

            return image;
        }
    }
}
