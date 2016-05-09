using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services
{
    public class MusicLibrarySearchService : IMusicLibrarySearchService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMusicLibraryDbContext _dukeboxData;
        private readonly IMusicLibrary _musicLibrary;

        public MusicLibrarySearchService(IMusicLibraryDbContext dukeboxData, IMusicLibrary musicLibrary)
        {
            _dukeboxData = dukeboxData;
            _musicLibrary = musicLibrary;
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
            var stopwatch = Stopwatch.StartNew();

            var songs = _dukeboxData.Songs;
            var matchingSongs = Enumerable.Empty<Song>();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return songs.ToList().Select(Track.BuildTrackInstance).ToList();
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
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.albumId.HasValue ? matchingAlbums.Contains(s.albumId.Value) : false));
            }

            if (searchAreas.Contains(SearchAreas.Artist))
            {
                var matchingArtists = GetMatchingAttributeIds(SearchAreas.Artist, searchTerm);
                matchingSongs = matchingSongs.Concat(songs.Where(t => t.artistId.HasValue ? matchingArtists.Contains(t.artistId.Value) : false));
            }

            if (searchAreas.Contains(SearchAreas.Song))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.title.ToLower().Contains(searchTerm)));
            }

            if (searchAreas.Contains(SearchAreas.Filename))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.filename.ToLower().Contains(searchTerm)));
            }


            stopwatch.Stop();
            var searchAreasString = searchAreas.Select(sa => Enum.GetName(typeof(SearchAreas), sa)).Aggregate((c, n) => c + ", " + n);
            logger.DebugFormat("Getting tracks by attribute(s) '{0}' where name or title contain '{1}' took {2}ms and returned {3} results.",
                searchAreasString, searchTerm, stopwatch.ElapsedMilliseconds, matchingSongs.Count());

            return matchingSongs.Count() < 1 ? new List<ITrack>() : matchingSongs.ToList().Select(Track.BuildTrackInstance).ToList();
        }

        public List<ITrack> SearchForTracksInArea(SearchAreas attribute, string nameOrTitle)
        {
            return SearchForTracks(nameOrTitle, new List<SearchAreas> { attribute });
        }

        public List<ITrack> GetTracksByAttributeValue(SearchAreas attribute, string attributeValue)
        {
            var stopwatch = Stopwatch.StartNew();

            var songs = _dukeboxData.Songs;
            var matchingSongs = Enumerable.Empty<Song>();

            var lowerAttributeValue = attributeValue.ToLower();

            if (attribute == SearchAreas.Album)
            {
                var matchingAlbums = GetMatchingAttributeIds(SearchAreas.Album, lowerAttributeValue, true);
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.albumId.HasValue ? matchingAlbums.Contains(s.albumId.Value) : false));
            }

            if (attribute == SearchAreas.Artist)
            {
                var matchingArtists = GetMatchingAttributeIds(SearchAreas.Artist, lowerAttributeValue, true);
                matchingSongs = matchingSongs.Concat(songs.Where(t => t.artistId.HasValue ? matchingArtists.Contains(t.artistId.Value) : false));
            }

            if (attribute == SearchAreas.Song)
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.title.ToLower().Equals(lowerAttributeValue)));
            }

            if (attribute == SearchAreas.Filename)
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.filename.ToLower().Equals(lowerAttributeValue)));
            }


            stopwatch.Stop();
            logger.DebugFormat("Getting tracks by attribute(s) '{0}' where name or title equal '{1}' took {2}ms and returned {3} results.",
                attribute, lowerAttributeValue, stopwatch.ElapsedMilliseconds, matchingSongs.Count());

            return matchingSongs.Count() < 1 ? new List<ITrack>() : matchingSongs.ToList().Select(Track.BuildTrackInstance).ToList();
        }

        private IEnumerable<long> GetMatchingAttributeIds(SearchAreas attribute, string searchTerm, bool exactMatch = false)
        {
            if (attribute == SearchAreas.Album)
            {
                if (exactMatch)
                {
                    return _musicLibrary.OrderedAlbums.Where(a => a.ToString().ToLower().Equals(searchTerm)).Select(a => a.id);
                }
                else
                {
                    return _musicLibrary.OrderedAlbums.Where(a => a.ToString().ToLower().Contains(searchTerm)).Select(a => a.id);
                }
            }
            else if (attribute == SearchAreas.Artist)
            {
                if (exactMatch)
                {
                    return _musicLibrary.OrderedArtists.Where(a => a.ToString().ToLower().Equals(searchTerm)).Select(a => a.id);
                }
                else
                {
                    return _musicLibrary.OrderedArtists.Where(a => a.ToString().ToLower().Contains(searchTerm)).Select(a => a.id);
                }
            }

            return Enumerable.Empty<long>();
        }

        /// <summary>
        /// Get a list of tracks who's attribute type equals the id specified. Filename 
        /// attribute is supported by this lookup method.
        /// </summary>
        /// <param name="attribute">The attribute type to select.</param>
        /// <param name="attributeId">The id of the attribute.</param>
        /// <returns>A list of tracks that match the given attribute keypair.</returns>
        public List<ITrack> GetTracksByAttributeId(SearchAreas attribute, long attributeId)
        {
            var stopwatch = Stopwatch.StartNew();

            var searchAreas = new List<SearchAreas>();
            var songs = _dukeboxData.Songs;
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
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.albumId != null ? s.albumId == attributeId : false));
            }

            if (searchAreas.Contains(SearchAreas.Artist))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.artistId != null ? s.artistId == attributeId : false));
            }

            if (searchAreas.Contains(SearchAreas.Song))
            {
                matchingSongs = matchingSongs.Concat(songs.Where(s => s.id == attributeId));
            }

            stopwatch.Stop();
            logger.DebugFormat("Getting tracks by attribute {0} and value {1} took {2}ms and returned {3} results.",
                Enum.GetName(typeof(SearchAreas), attribute), attributeId, stopwatch.ElapsedMilliseconds, matchingSongs.Count());

            return matchingSongs.Count() < 1 ? new List<ITrack>() : matchingSongs.Select(Track.BuildTrackInstance).ToList();
        }
    }
}
