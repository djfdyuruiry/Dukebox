﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using System;

namespace Dukebox.Library.Services.MusicLibrary
{
    public class MusicLibraryCacheService : IMusicLibraryCacheService
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMusicLibraryDbContextFactory _dbContextFactory;
        private readonly IMusicLibraryEventService _eventService;
        private readonly SemaphoreSlim _cacheSemaphore;
        private readonly BlockingCollection<string> _allFilesCache;
        private Dictionary<string, DateTime> _fileLastScannedMap;

        private List<Artist> _allArtistsCache;
        private List<Album> _allAlbumsCache;
        private List<Playlist> _allPlaylistsCache;
        private List<Song> _allSongsCache;

        public List<Artist> OrderedArtists
        {
            get
            {
                try
                {
                    _cacheSemaphore.Wait();
                    return _allArtistsCache;
                }
                finally
                {
                    _cacheSemaphore.Release();
                }
            }
        }

        public List<Album> OrderedAlbums
        {
            get
            {
                try
                {
                    _cacheSemaphore.Wait();
                    return _allAlbumsCache;
                }
                finally
                {
                    _cacheSemaphore.Release();
                }
            }
        }

        public List<Playlist> OrderedPlaylists
        {
            get
            {
                try
                {
                    _cacheSemaphore.Wait();
                    return _allPlaylistsCache;
                }
                finally
                {
                    _cacheSemaphore.Release();
                }
            }
        }

        public BlockingCollection<string> FilesCache
        {
            get
            {
                try
                {
                    _cacheSemaphore.Wait();
                    return _allFilesCache;
                }
                finally
                {
                    _cacheSemaphore.Release();
                }
            }
        }

        public List<Song> SongsCache
        {
            get
            {
                try
                {
                    _cacheSemaphore.Wait();
                    return _allSongsCache;
                }
                finally
                {
                    _cacheSemaphore.Release();
                }
            }
        }

        public MusicLibraryCacheService(IMusicLibraryDbContextFactory dbContextFactory, IMusicLibraryEventService eventService)
        {
            _dbContextFactory = dbContextFactory;
            _eventService = eventService;
            _cacheSemaphore = new SemaphoreSlim(1, 1);

            _eventService.DatabaseChangesSaved += (o, e) => RefreshCaches();
            _eventService.SongAdded += (o, e) => RefreshCaches();

            _allFilesCache = new BlockingCollection<string>();

            RefreshCaches();
        }

        public void RefreshCaches()
        {
            try
            {
                _cacheSemaphore.Wait();

                var stopwatch = Stopwatch.StartNew();
                List<string> files;
                
                using (var dukeboxData = _dbContextFactory.GetInstance())
                {
                    _allArtistsCache = dukeboxData.Artists.OrderBy(a => a.Name).ToList();
                    _allAlbumsCache = dukeboxData.Albums.OrderBy(a => a.Name).ToList();
                    _allPlaylistsCache = dukeboxData.Playlists.OrderBy(a => a.Name).ToList();
                    
                    _allSongsCache = dukeboxData.Songs.ToList();

                    files = _allSongsCache.Select(s => s.FileName).ToList();
                }

                while (_allFilesCache.Count > 0)
                {
                    string item;
                    _allFilesCache.TryTake(out item);
                }

                files.ForEach(f => _allFilesCache.Add(f));

                _fileLastScannedMap = _allSongsCache
                        .GroupBy(s => s.FileName)
                        .Select(sg => sg.First())
                        .ToDictionary(s => s.FileName, s => s.LastScanDateTime);

                stopwatch.Stop();

                logger.Info("Music library artist, album, playlist and file path caches were refreshed");
                logger.DebugFormat("Refreshing library artist and album caches took {0}ms.", stopwatch.ElapsedMilliseconds);
            }
            finally
            {
                _cacheSemaphore.Release();
            }

            _eventService.TriggerEvent(MusicLibraryEvent.ArtistCacheRefreshed);
            _eventService.TriggerEvent(MusicLibraryEvent.AlbumCacheRefreshed);
            _eventService.TriggerEvent(MusicLibraryEvent.PlaylistCacheRefreshed);
            _eventService.TriggerEvent(MusicLibraryEvent.FilesCacheRefreshed);
            _eventService.TriggerEvent(MusicLibraryEvent.CachesRefreshed);
        }

        public bool HasFileBeenUpdatedSinceLastScan(string file, DateTime lastWriteTime)
        {
            try
            {
                _cacheSemaphore.Wait();

                if (!_fileLastScannedMap.ContainsKey(file))
                {
                    return true;
                }

                return _fileLastScannedMap[file] < lastWriteTime;
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }
    }
}
