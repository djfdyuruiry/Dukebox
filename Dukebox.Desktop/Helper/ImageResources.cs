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
        public const string DefaultAlbumArtUri = @"pack://application:,,,/Graphics/black_7_music_node.png";
        public const string PauseImageUri = @"pack://application:,,,/Graphics/black_4_audio_pause.png";
        public const string PlayImageUri = @"pack://application:,,,/Graphics/black_4_audio_play.png";
        public static readonly ImageSource DefaultAlbumArtImage = ImageResourceLoader.LoadImageFromResourceUri(DefaultAlbumArtUri);
        public static readonly ImageSource PlayImage = ImageResourceLoader.LoadImageFromResourceUri(PlayImageUri);
        public static readonly ImageSource PauseImage = ImageResourceLoader.LoadImageFromResourceUri(PauseImageUri);    
    }
}
