using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using Dukebox.Library.Factories;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Repositories
{
    public class MusicLibraryRepository : IMusicLibraryRepository
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMusicLibraryDbContextFactory _dbContextFactory;
        private readonly TrackFactory _trackFactory;
        private readonly IMusicLibraryCacheService _cacheService;

        public MusicLibraryRepository(IMusicLibraryDbContextFactory dbContextFactory, TrackFactory trackFactory, IMusicLibraryCacheService cacheService)
        {
            _dbContextFactory = dbContextFactory;
            _trackFactory = trackFactory;
            _cacheService = cacheService;
        }

        public int GetArtistCount()
        {
            return _cacheService.OrderedArtists.Count;
        }

        public int GetAlbumCount()
        {
            return _cacheService.OrderedAlbums.Count;
        }

        public Playlist GetPlaylistById(long? playlistId)
        {
            var stopwatch = Stopwatch.StartNew();
            Playlist playlist;

            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                playlist = dukeboxData.Playlists.Where(p => p.Id == playlistId).FirstOrDefault();
            }

            stopwatch.Stop();

            if (playlist == null)
            {
                throw new Exception(string.Format("No playlist with the id '{0}' was found in the database.", playlistId));
            }

            logger.DebugFormat("Look up for artist with id '{0}' took {1}ms.", playlistId, stopwatch.ElapsedMilliseconds);
            return playlist;
        }

        public int GetPlaylistCount()
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                return dukeboxData.Playlists.Count();
            }
        }

        public List<ITrack> GetTracksForArtist(string artistName)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                return dukeboxData.Songs
                    .Where(s => s.ArtistName.Equals(artistName))
                    .ToList()
                    .Select(s => _trackFactory.BuildTrackInstance(s))
                    .ToList();
            }
        }

        public List<ITrack> GetTracksForAlbum(string albumName)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                return dukeboxData.Songs
                    .Where(s => s.AlbumName.Equals(albumName))
                    .ToList()
                    .Select(s => _trackFactory.BuildTrackInstance(s))
                    .ToList();
            }
        }
    }
}
