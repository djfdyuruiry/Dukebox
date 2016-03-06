using Dukebox.Model.Services;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Dukebox.Library.Interfaces
{
    public interface IAlbumArtCacheService
    {
        event EventHandler AlbumAdded;
        void AddSongToCache(song songToProcess, AudioFileMetaData metadata, album albumObj);
        bool CheckCacheForAlbum(long albumId);
        Image GetAlbumArtFromCache(long albumId);
        List<long> GetAlbumIdsFromCache();
    }
}
