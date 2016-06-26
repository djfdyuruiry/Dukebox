using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services.MusicLibrary
{
    public class MusicLibraryCacheService : IMusicLibraryCacheService
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMusicLibraryDbContextFactory _dbContextFactory;
        private readonly IMusicLibraryEventService _eventService;

        private List<Artist> _allArtistsCache;
        private List<Album> _allAlbumsCache;
        private List<Playlist> _allPlaylistsCache;
        private List<string> _allFilesCache;

        public List<Artist> OrderedArtists
        {
            get
            {
                if (_allArtistsCache == null)
                {
                    using (var dukeboxData = _dbContextFactory.GetInstance())
                    {
                        _allArtistsCache = dukeboxData.Artists.OrderBy(a => a.Name).ToList();
                    }

                    _eventService.TriggerEvent(MusicLibraryEvent.ArtistCacheRefreshed);
                }

                return _allArtistsCache;
            }
        }
        
        public List<Album> OrderedAlbums
        {
            get
            {
                if (_allAlbumsCache == null)
                {
                    using (var dukeboxData = _dbContextFactory.GetInstance())
                    {
                        _allAlbumsCache = dukeboxData.Albums.OrderBy(a => a.Name).ToList();
                    }

                    _eventService.TriggerEvent(MusicLibraryEvent.AlbumCacheRefreshed);
                }

                return _allAlbumsCache;
            }
        }

        public List<Playlist> OrderedPlaylists
        {
            get
            {
                if (_allPlaylistsCache == null)
                {
                    using (var dukeboxData = _dbContextFactory.GetInstance())
                    {
                        _allPlaylistsCache = dukeboxData.Playlists.OrderBy(a => a.Name).ToList();
                    }

                    _eventService.TriggerEvent(MusicLibraryEvent.PlaylistCacheRefreshed);
                }

                return _allPlaylistsCache;
            }
        }

        public List<string> FilesCache
        {
            get
            {
                if (_allFilesCache == null)
                {
                    using (var dukeboxData = _dbContextFactory.GetInstance())
                    {
                        _allFilesCache = dukeboxData.Songs.Select(s => s.FileName).ToList();
                    }

                    _eventService.TriggerEvent(MusicLibraryEvent.PlaylistCacheRefreshed);
                }

                return _allFilesCache;
            }
        }

        public MusicLibraryCacheService(IMusicLibraryDbContextFactory dbContextFactory, IMusicLibraryEventService eventService)
        {
            _dbContextFactory = dbContextFactory;
            _eventService = eventService;

            _eventService.DatabaseChangesSaved += (o, e) => RefreshCaches();
            
            RefreshCaches();
        }

        public void RefreshCaches()
        {
            var stopwatch = Stopwatch.StartNew();

            _allArtistsCache = null;
            _allAlbumsCache = null;
            _allPlaylistsCache = null;
            _allFilesCache = null;

            var albums = OrderedAlbums;
            var artists = OrderedArtists;
            var playlists = OrderedPlaylists;
            var files = _allFilesCache;

            stopwatch.Stop();
            logger.Info("Music library artist, album, playlist and file path caches were refreshed");
            logger.DebugFormat("Refreshing library artist and album caches took {0}ms.", stopwatch.ElapsedMilliseconds);

            _eventService.TriggerEvent(MusicLibraryEvent.CachesRefreshed);
        }
    }
}
