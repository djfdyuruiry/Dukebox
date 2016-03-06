using Dukebox.Desktop.Helper;
using Dukebox.Library;
using Dukebox.Library.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Dukebox.Desktop.Services
{
    public class Album
    {
        private readonly IAlbumArtCacheService _albumArtCacheService;
        private readonly ImageToImageSourceConverter _imageToImageSourceConverter;
        private ImageSource _albumArt;

        public album Data { get; private set; }

        public ImageSource AlbumArt
        {
            get
            {
                if (_albumArt == null)
                {
                    _albumArt = Data.HasAlbumArt ? GetAlbumArt() : ImageResources.DefaultAlbumArtImage;
                }

                return _albumArt;
            }
        }


        public static Album BuildAlbumInstance(album data)
        {
            var instance = DesktopContainer.GetInstance<Album>();
            instance.Data = data;

            return instance;
        }

        public Album(IAlbumArtCacheService albumArtCacheService, ImageToImageSourceConverter imageToImageSourceConverter)
        {
            _albumArtCacheService = albumArtCacheService;
            _imageToImageSourceConverter = imageToImageSourceConverter;
        }

        public static bool ContainsString(Album album, string stringToFind)
        {
            return album.Data.name.ToLower().Contains(stringToFind.ToLower());
        }

        private ImageSource GetAlbumArt()
        {
            var albumArtImage = _albumArtCacheService.GetAlbumArtFromCache(Data.id);
            var albumArtImageSource = _imageToImageSourceConverter.Convert(albumArtImage);
            return albumArtImageSource;
        }
    }
}
