using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Dukebox.Library.Interfaces
{
    public interface IAlbumArtCacheService
    {
        event EventHandler AlbumAdded;
        void AddSongToCache(song songToProcess, IAudioFileMetaData metadata, album albumObj);
        bool CheckCacheForAlbum(long albumId);
        Image GetAlbumArtFromCache(long albumId);
        List<long> GetAlbumIdsFromCache();
    }
}
