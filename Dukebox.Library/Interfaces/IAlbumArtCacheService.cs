using Dukebox.Model.Services;
using System;
using System.Drawing;

namespace Dukebox.Library.Interfaces
{
    public interface IAlbumArtCacheService
    {
        void AddSongToCache(song songToProcess, AudioFileMetaData metadata, album albumObj);
        bool CheckCacheForAlbum(long albumId);
        Image GetAlbumArtFromCache(long albumId);
    }
}
