using Dukebox.Desktop.Helper;
using Dukebox.Library.Interfaces;
using LibraryAlbum = Dukebox.Library.Model.Album;
using System.Windows.Media;

namespace Dukebox.Desktop.Services
{
    public class Album
    {
        private readonly IAlbumArtCacheService _albumArtCacheService;
        private string _albumArtPath;

        public LibraryAlbum Data { get; private set; }

        public string AlbumArtPath
        {
            get
            {
                if (_albumArtPath == null)
                {
                    _albumArtPath = _albumArtCacheService.CheckCacheForAlbum(Data.id) ? 
                        _albumArtCacheService.GetAlbumArtPathFromCache(Data.id) : 
                        ImageResources.DefaultAlbumArtUri;
                }

                return _albumArtPath;
            }
        }

        public static Album BuildAlbumInstance(LibraryAlbum data)
        {
            var instance = DesktopContainer.GetInstance<Album>();
            instance.Data = data;

            return instance;
        }

        public Album(IAlbumArtCacheService albumArtCacheService)
        {
            _albumArtCacheService = albumArtCacheService;
        }

        public static bool ContainsString(Album album, string stringToFind)
        {
            var name = album.Data.name;

            return string.IsNullOrEmpty(name) ? false : name.ToLower().Contains(stringToFind.ToLower());
        }        
    }
}
