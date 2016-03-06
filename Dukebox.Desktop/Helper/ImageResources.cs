using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Dukebox.Desktop.Helper
{
    public static class ImageResources
    {
        private const string defaultAlbumArtUri = @"pack://application:,,,/Graphics/black_7_music_node.png";
        private const string pauseImageUri = @"pack://application:,,,/Graphics/black_4_audio_pause.png";
        private const string playImageUri = @"pack://application:,,,/Graphics/black_4_audio_play.png";
        public static readonly ImageSource DefaultAlbumArtImage = ImageResourceLoader.LoadImageFromResourceUri(defaultAlbumArtUri);
        public static readonly ImageSource PlayImage = ImageResourceLoader.LoadImageFromResourceUri(playImageUri);
        public static readonly ImageSource PauseImage = ImageResourceLoader.LoadImageFromResourceUri(pauseImageUri);    
    }
}
