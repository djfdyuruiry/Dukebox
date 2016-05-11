using System;
using System.Collections.Generic;
using System.Drawing;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IAlbumArtCacheService
    {
        event EventHandler AlbumAdded;
        List<long> GetAlbumIdsFromCache();
        bool CheckCacheForAlbum(long albumId);
        void AddAlbumToCache(Album album, IAudioFileMetadata tag);
        Image GetAlbumArtFromCache(long albumId);
        string GetAlbumArtPathFromCache(long albumId);
    }
}
