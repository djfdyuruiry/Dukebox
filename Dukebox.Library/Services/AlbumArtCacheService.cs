using Dukebox.Library.Config;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Dukebox.Library.Services
{
    public class AlbumArtCacheService : IAlbumArtCacheService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDukeboxSettings _settings;
        private string _cachePath;

        public event EventHandler AlbumAdded;

        public AlbumArtCacheService(IDukeboxSettings settings)
        {
            _settings = settings;
            BuildCachePath();
        }

        private void BuildCachePath()
        {
            try
            {
                var relativePath = _settings.AlbumArtCachePath;
                var absolutePath = Path.GetFullPath(relativePath);

                if (!Directory.Exists(absolutePath))
                {
                    Directory.CreateDirectory(absolutePath);
                }

                _cachePath = absolutePath;                
            }
            catch (Exception ex)
            {
                logger.Error("Error building album art cache path.", ex);
                throw ex;
            }
        }

        public void AddSongToCache(Song songToProcess, IAudioFileMetaData metadata, Album albumObj)
        {
            var stopwatch = Stopwatch.StartNew();

            if (albumObj == null)
            {
                logger.InfoFormat("Not processing song with id {0} into album art cache as it has no associated album.", songToProcess.id);
                return;
            }
            else if (!metadata.HasFutherMetadataTag || !metadata.HasAlbumArt)
            {
                logger.InfoFormat("Not processing song with id {0} into album art cache as the song does not contain album art data.", songToProcess.id);
                return;
            }

            var albumId = albumObj.id;

            if (CheckCacheForAlbum(albumId))
            {
                logger.InfoFormat("Not processing song with id {0} into album art cache as it is already in the album art cache.", songToProcess.id);
                return;
            }
                                       
            try
            {
                var img = metadata.AlbumArt;
                var albumIdString = albumObj.id.ToString();
                var path = Path.Combine(_cachePath, albumIdString);

                if (img != null && !File.Exists(path))
                {
                    img.Save(path);
                }

                stopwatch.Stop();
                logger.InfoFormat("Added album with id {0} into album art cache.", albumIdString);
                logger.DebugFormat("Adding album into album art cache took {0}ms. Album id: {1}", stopwatch.ElapsedMilliseconds, albumIdString);

                if (AlbumAdded != null)
                {
                    AlbumAdded(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error adding album with id {0} into album art cache.", albumId), ex);
            }
        }

        public bool CheckCacheForAlbum(long albumId)
        {
            try
            {
                var path = Path.Combine(_cachePath, albumId.ToString());
                return File.Exists(path);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error checking if album with id {0} is in album art cache.", albumId), ex);
                return false;
            }
        }

        public Image GetAlbumArtFromCache(long albumId)
        {
            try
            {
                if (!CheckCacheForAlbum(albumId))
                {
                    throw new Exception(string.Format("Album with id {0} is not in the album art cache.", albumId));
                }

                logger.InfoFormat("Fetching album artwork for album with id {0} from ablum art file cache.", albumId);
                var path = Path.Combine(_cachePath, albumId.ToString());

                return Image.FromFile(path);
            }
            catch (Exception ex)
            {            
                var msg = string.Format("Error getting album with id {0} from album art cache.", albumId);
                logger.Error(msg, ex);

                throw new Exception(string.Format("{0}: {1}", msg, ex.Message)); 
            }
        }

        public List<long> GetAlbumIdsFromCache()
        {
            var ids = Directory.EnumerateFiles(_cachePath)
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Select(f => long.Parse(f))
                .ToList();

            return ids;
        }
    }
}
