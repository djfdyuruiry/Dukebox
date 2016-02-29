﻿using Dukebox.Library.Config;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Model.Services;
using log4net;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Dukebox.Library.Services
{
    public class AlbumArtCacheService : IAlbumArtCacheService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDukeboxSettings _settings;
        private bool _errorBuildingCachePath;
        private string _cachePath;

        public AlbumArtCacheService(IDukeboxSettings settings)
        {
            _settings = settings;
            BuildCachePath();
        }

        private void BuildCachePath()
        {
            _errorBuildingCachePath = false;

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
                _errorBuildingCachePath = true;
                _logger.Error("Error building album art cache path.", ex);
            }
        }

        private bool LoggedCachePathCheck(long albumId, string cacheOperation)
        {
            if (_errorBuildingCachePath)
            {
                _logger.WarnFormat("Aborting cache operation '{0}' for album with id {1} as a previous error occurred when building cache path.", cacheOperation, albumId);
            }

            return !_errorBuildingCachePath;
        }

        public void AddSongToCache(song songToProcess, AudioFileMetaData metadata, album albumObj)
        {
            var stopwatch = Stopwatch.StartNew();

            if (albumObj == null)
            {
                _logger.InfoFormat("Not processing song with id {0} into album art cache as it has no associated album.", songToProcess.id);
                return;
            }
            else if (!metadata.HasFutherMetadataTag || !metadata.HasAlbumArt)
            {
                _logger.InfoFormat("Not processing song with id {0} into album art cache as the song does not contain album art data.", songToProcess.id);
                return;
            }

            var albumId = albumObj.id;

            if (!LoggedCachePathCheck(albumId, AlbumArtCacheOperations.Add))
            {
                return;
            }

            if (CheckCacheForAlbum(albumId))
            {
                _logger.InfoFormat("Not processing song with id {0} into album art cache as it is already in the album art cache.", songToProcess.id);
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
                _logger.InfoFormat("Added album with id {0} into album art cache.", albumIdString);
                _logger.DebugFormat("Adding album into album art cache took {0}ms. Album id: {1}", stopwatch.ElapsedMilliseconds, albumIdString);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error adding album with id {0} into album art cache.", albumId), ex);
            }
        }

        public bool CheckCacheForAlbum(long albumId)
        {
            if (!LoggedCachePathCheck(albumId, AlbumArtCacheOperations.Check))
            {
                return false;
            }

            try
            {
                var path = Path.Combine(_cachePath, albumId.ToString());
                return File.Exists(path);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error checking if album with id {0} is in album art cache.", albumId), ex);
                return false;
            }
        }

        public Image GetAlbumArtFromCache(long albumId)
        {
            try
            {
                if (!LoggedCachePathCheck(albumId, AlbumArtCacheOperations.Get))
                {
                    throw new Exception("Failed to build path to cache.");
                }

                if (!CheckCacheForAlbum(albumId))
                {
                    throw new Exception(string.Format("Album with id {0} is not in the album art cache.", albumId));
                }

                _logger.InfoFormat("Fetching album artwork for album with id {0} from ablum art file cache.", albumId);
                var path = Path.Combine(_cachePath, albumId.ToString());

                return Image.FromFile(path);
            }
            catch (Exception ex)
            {            
                var msg = string.Format("Error getting album with id {0} from album art cache.", albumId);
                _logger.Error(msg, ex);

                throw new Exception(string.Format("{0}: {1}", msg, ex.Message)); 
            }
        }
    }
}
