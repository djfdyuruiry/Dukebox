using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Factories;

namespace Dukebox.Library.Services.MusicLibrary
{
    public class MusicLibrarySearchService : IMusicLibrarySearchService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMusicLibraryDbContextFactory _dbContextFactory;
        private readonly TrackFactory _trackFactory;
        private readonly IMusicLibraryCacheService _cacheService;

        public MusicLibrarySearchService(IMusicLibraryDbContextFactory dbContextFactory, TrackFactory trackFactory, IMusicLibraryCacheService cacheService)
        {
            _dbContextFactory = dbContextFactory;
            _trackFactory = trackFactory;

            _cacheService = cacheService;
        }

        public int GetSongCount()
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var songs = dukeboxData.Songs;
                return songs.Count();
            }
        }

        /// <summary>
        /// Get all search areas and add them to a given list.
        /// </summary>
        /// <param name="searchAreas">List to add search areas to.</param>
        private void AddSearchAreasToList(List<SearchAreas> searchAreas)
        {
            foreach (var area in Enum.GetValues(typeof(SearchAreas)))
            {
                searchAreas.Add((SearchAreas)area);
            }
        }

        public List<ITrack> GetTracksForRange(int start, int count)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var songs = dukeboxData.Songs;

                return songs.Where(s => s.Id >= start).Take(count).ToList()
                    .Select(s => _trackFactory.BuildTrackInstance(s)).ToList();
            }
        }

        public ITrack GetTrackFromLibraryOrFile(string filename)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var song = dukeboxData.Songs.FirstOrDefault(s => s.FileName.ToLower() == filename.ToLower());

                return song != null ? _trackFactory.BuildTrackInstance(song) : _trackFactory.BuildTrackInstance(filename);
            }
        }

        /// <summary>
        /// Search the library database for tracks matching
        /// the search term specified. All text returned by
        /// a call to 'ToString' on any given track object is
        /// searched for the the term provided.
        /// 
        /// This search is not case sensitive.
        /// </summary>
        /// <param name="searchTerm">The term to search for in track descriptions.</param>
        /// <returns>A list of tracks that match the given search criteria.</returns>
        public List<ITrack> SearchForTracks(string searchTerm, List<SearchAreas> searchAreas)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var stopwatch = Stopwatch.StartNew();
                var songs = dukeboxData.Songs;
                var matchingSongs = Enumerable.Empty<Song>();

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return songs.ToList().Select(s => _trackFactory.BuildTrackInstance(s)).ToList();
                }

                searchTerm = searchTerm.ToLower();
                searchAreas = searchAreas ?? new List<SearchAreas>();

                if (searchAreas.Contains(SearchAreas.All))
                {
                    AddSearchAreasToList(searchAreas);
                }

                if (searchAreas.Contains(SearchAreas.Album))
                {
                    var matchingAlbums = GetMatchingAttributeIds(SearchAreas.Album, searchTerm);
                    matchingSongs = matchingSongs.Concat(songs.Where(s => matchingAlbums.Contains(s.AlbumName)));
                }

                if (searchAreas.Contains(SearchAreas.Artist))
                {
                    var matchingArtists = GetMatchingAttributeIds(SearchAreas.Artist, searchTerm);
                    matchingSongs = matchingSongs.Concat(songs.Where(s => matchingArtists.Contains(s.ArtistName)));
                }

                if (searchAreas.Contains(SearchAreas.Song))
                {
                    matchingSongs = matchingSongs.Concat(songs.Where(s => s.Title.ToLower().Contains(searchTerm)));
                }

                if (searchAreas.Contains(SearchAreas.Filename))
                {
                    matchingSongs = matchingSongs.Concat(songs.Where(s => s.FileName.ToLower().Contains(searchTerm)));
                }

                var matchingSongsList = matchingSongs.Distinct().ToList();

                stopwatch.Stop();
                var searchAreasString = searchAreas.Select(sa => Enum.GetName(typeof(SearchAreas), sa)).Aggregate((c, n) => c + ", " + n);
                logger.DebugFormat("Getting tracks by attribute(s) '{0}' where name or title contain '{1}' took {2}ms and returned {3} results.",
                    searchAreasString, searchTerm, stopwatch.ElapsedMilliseconds, matchingSongsList.Count);


                return !matchingSongsList.Any() ? new List<ITrack>() : matchingSongsList.Select(s => _trackFactory.BuildTrackInstance(s)).ToList();
            }
        }

        public List<ITrack> SearchForTracksInArea(SearchAreas attribute, string nameOrTitle)
        {
            return SearchForTracks(nameOrTitle, new List<SearchAreas> { attribute });
        }

        public List<ITrack> GetTracksByAttributeValue(SearchAreas attribute, string attributeValue)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var lowerAttributeValue = attributeValue.ToLower();
                var stopwatch = Stopwatch.StartNew();
                var songs = dukeboxData.Songs;
                var matchingSongs = Enumerable.Empty<Song>();

                if (attribute == SearchAreas.Album)
                {
                    var matchingAlbums = GetMatchingAttributeIds(SearchAreas.Album, lowerAttributeValue, true);
                    matchingSongs = matchingSongs.Concat(songs.Where(s => matchingAlbums.Contains(s.AlbumName)));
                }

                if (attribute == SearchAreas.Artist)
                {
                    var matchingArtists = GetMatchingAttributeIds(SearchAreas.Artist, lowerAttributeValue, true);
                    matchingSongs = matchingSongs.Concat(songs.Where(s => matchingArtists.Contains(s.ArtistName)));
                }

                if (attribute == SearchAreas.Song)
                {
                    matchingSongs = matchingSongs.Concat(songs.Where(s => s.Title.ToLower().Equals(lowerAttributeValue)));
                }

                if (attribute == SearchAreas.Filename)
                {
                    matchingSongs = matchingSongs.Concat(songs.Where(s => s.FileName.ToLower().Equals(lowerAttributeValue)));
                }

                var matchingSongsList = matchingSongs.Distinct().ToList();

                stopwatch.Stop();
                logger.DebugFormat("Getting tracks by attribute(s) '{0}' where name or title equal '{1}' took {2}ms and returned {3} results.",
                    attribute, lowerAttributeValue, stopwatch.ElapsedMilliseconds, matchingSongsList.Count);

                return !matchingSongsList.Any() ? new List<ITrack>() : matchingSongsList.Select(s => _trackFactory.BuildTrackInstance(s)).ToList();
            }
        }

        private IEnumerable<string> GetMatchingAttributeIds(SearchAreas attribute, string searchTerm, bool exactMatch = false)
        {
            if (attribute == SearchAreas.Album)
            {
                if (exactMatch)
                {
                    return _cacheService.OrderedAlbums.Where(a => a.Name.ToLower().Equals(searchTerm)).Select(a => a.Name);
                }
                else
                {
                    return _cacheService.OrderedAlbums.Where(a => a.Name.ToLower().Contains(searchTerm)).Select(a => a.Name);
                }
            }
            else if (attribute == SearchAreas.Artist)
            {
                if (exactMatch)
                {
                    return _cacheService.OrderedArtists.Where(a => a.Name.ToLower().Equals(searchTerm)).Select(a => a.Name);
                }
                else
                {
                    return _cacheService.OrderedArtists.Where(a => a.Name.ToLower().Contains(searchTerm)).Select(a => a.Name);
                }
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Get a list of tracks who's attribute type equals the id specified. Filename 
        /// attribute is supported by this lookup method.
        /// </summary>
        /// <param name="attribute">The attribute type to select.</param>
        /// <param name="attributeId">The id of the attribute.</param>
        /// <returns>A list of tracks that match the given attribute keypair.</returns>
        public List<ITrack> GetTracksByAttributeId(SearchAreas attribute, string attributeId)
        {
            using (var dukeboxData = _dbContextFactory.GetInstance())
            {
                var searchAreas = new List<SearchAreas>();
                var stopwatch = Stopwatch.StartNew();
                var songs = dukeboxData.Songs;
                var matchingSongs = Enumerable.Empty<Song>();

                if (attribute == SearchAreas.Filename)
                {
                    throw new InvalidOperationException("The filename search attribute is not supported when looking up tracks by attribute id");
                }

                if (attribute == SearchAreas.All)
                {
                    AddSearchAreasToList(searchAreas);
                }
                else
                {
                    searchAreas.Add(attribute);
                }

                if (searchAreas.Contains(SearchAreas.Album))
                {
                    matchingSongs = matchingSongs.Concat(songs.Where(s => s.AlbumName == attributeId));
                }

                if (searchAreas.Contains(SearchAreas.Artist))
                {
                    matchingSongs = matchingSongs.Concat(songs.Where(s => s.ArtistName == attributeId));
                }

                if (searchAreas.Contains(SearchAreas.Song))
                {
                    matchingSongs = matchingSongs.Concat(songs.Where(s => s.Id.ToString() == attributeId));
                }

                var matchingSongsList = matchingSongs.Distinct().ToList();


                stopwatch.Stop();
                logger.DebugFormat("Getting tracks by attribute {0} and value {1} took {2}ms and returned {3} results.",
                    Enum.GetName(typeof(SearchAreas), attribute), attributeId, stopwatch.ElapsedMilliseconds, matchingSongsList.Count);

                return !matchingSongsList.Any() ? new List<ITrack>() : matchingSongsList.Select(s => _trackFactory.BuildTrackInstance(s)).ToList();
            }
        }
    }
}
