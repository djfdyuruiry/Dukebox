using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using FakeItEasy;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Factories;
using Dukebox.Desktop.Model;

namespace Dukebox.Desktop.Services
{
    public class LibraryTracksSource : ILibraryTracksSource
    {
        private readonly IMusicLibrarySearchService _searchService;
        private readonly TrackFactory _trackFactory;
        private readonly IdLibrarySourceFilter _idFilter;
        private readonly ValueLibrarySourceFilter _valueFilter;

        private List<int> _fullSearchResultSetIds;

        public int Count
        {
            get
            {
                return TracksFiltersPresent ? _fullSearchResultSetIds?.Count ?? 0 : _searchService.GetSongCount();
            }
        }

        public bool TracksFiltersPresent => _idFilter != null || _valueFilter != null;

        public LibraryTracksSource(IMusicLibrarySearchService cacheService, TrackFactory trackFactory)
        {
            _searchService = cacheService;
            _trackFactory = trackFactory;
        }

        public LibraryTracksSource(IMusicLibrarySearchService cacheService, TrackFactory trackFactory, IdLibrarySourceFilter idFilter) 
            : this(cacheService, trackFactory)
        {
            _idFilter = idFilter;

            PopulateFullSearchResultIds();
        }

        public LibraryTracksSource(IMusicLibrarySearchService cacheService, TrackFactory trackFactory, ValueLibrarySourceFilter valueFilter)
            : this(cacheService, trackFactory)
        {
            _valueFilter = valueFilter;

            PopulateFullSearchResultIds();
        }

        private void PopulateFullSearchResultIds() => GetItemsAt(0, 0, false);

        public PagedSourceItemsPacket<ITrack> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {
            List<ITrack> tracks;

            Debug.WriteLine($"idFilter? {_idFilter != null} - valueFilter? {_valueFilter != null} -> Library page requested for tracks {pageoffset}-{pageoffset+count}");

            if (!TracksFiltersPresent)
            {
                tracks = _searchService.GetTracksForRange(pageoffset, count);
            }
            else if (_idFilter != null)
            {
                var results = _searchService.GetTracksByAttributeId(_idFilter.SearchAreas, _idFilter.Id, pageoffset, count);

                tracks = results.RangedResults;
                _fullSearchResultSetIds = results.FullResultSetIds;
            }
            else
            {
                var results = _searchService.GetTracksByAttributeValue(_valueFilter.SearchAreas, _valueFilter.Value, pageoffset, count);

                tracks = results.RangedResults;
                _fullSearchResultSetIds = results.FullResultSetIds;
            }

            Debug.WriteLine($"idFilter? {_idFilter != null} - valueFilter? {_valueFilter != null} -> Library returned {tracks?.Count} tracks");

            var itemsPacket = new PagedSourceItemsPacket<ITrack>
            {
                Items = tracks
            };

            GC.Collect();

            Console.WriteLine($"GetItemsAt called: {pageoffset} -> {count}");

            return itemsPacket;
        }

        public int IndexOf(ITrack item)
        {
            if (item == null || item.Song == null)
            {
                Debug.WriteLine($"null item passed for index check, returning -1");
                return -1;
            }

            var songId = (int)item.Song.Id;

            if (TracksFiltersPresent)
            {
                Debug.WriteLine($"item passed for index check, not TracksFiltersPresent so returning library id of {(int)item.Song.Id}");

                return songId;
            }

            var searchResultIndex = _fullSearchResultSetIds?.IndexOf(songId) ?? -1;

            Debug.WriteLine($"item passed for index check, TracksFiltersPresent so returning searchResultIndex of {(int)item.Song.Id}");

            return searchResultIndex;
        }

        public async Task<PagedSourceItemsPacket<ITrack>> GetItemsAtAsync(int pageoffset, int count, bool usePlaceholder)
        {
            return await Task.Run(() =>
            {
                return GetItemsAt(pageoffset, count, usePlaceholder);
            });
        }

        public ITrack GetPlaceHolder(int index, int page, int offset)
        {
            return A.Fake<ITrack>();
        }

        public async Task<int> GetCountAsync()
        {
            return await Task.Run(() =>
            {
                return Count;
            });
        }

        public async Task<int> IndexOfAsync(ITrack item)
        {
            return await Task.Run(() =>
            {
                return IndexOf(item);
            });
        }

        public void OnReset(int count)
        {
            // Do nothing
        }
    }
}
