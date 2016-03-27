using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Services;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Dukebox.Library.Interfaces
{
    public interface IAlbumArtCacheService
    {
        event EventHandler AlbumAdded;
        void AddSongToCache(Song songToProcess, IAudioFileMetaData metadata, Album albumObj);
        bool CheckCacheForAlbum(long albumId);
        Image GetAlbumArtFromCache(long albumId);
        List<long> GetAlbumIdsFromCache();
    }
}
