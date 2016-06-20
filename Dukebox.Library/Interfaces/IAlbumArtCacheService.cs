using System;
using System.Collections.Generic;
using System.Drawing;
using Dukebox.Library.Model;
using System.Threading.Tasks;

namespace Dukebox.Library.Interfaces
{
    public interface IAlbumArtCacheService
    {
        event EventHandler AlbumAdded;
        List<string> GetAlbumIdsFromCache();
        bool CheckCacheForAlbum(string albumId);
        void AddAlbumToCache(Album album, IAudioFileMetadata tag);
        Image GetAlbumArtFromCache(string albumId);
        string GetAlbumArtPathFromCache(string albumId);
    }
}
