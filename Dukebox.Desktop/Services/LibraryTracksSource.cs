using System;
using System.Collections.Generic;
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

        public int Count
        {
            get
            {
                return _searchService.GetSongCount();
            }
        }

        public LibraryTracksSource(IMusicLibrarySearchService cacheService, TrackFactory trackFactory)
        {
            _searchService = cacheService;
            _trackFactory = trackFactory;
        }

        public LibraryTracksSource(IMusicLibrarySearchService cacheService, TrackFactory trackFactory, IdLibrarySourceFilter idFilter) 
            : this(cacheService, trackFactory)
        {
            _idFilter = idFilter;
        }

        public LibraryTracksSource(IMusicLibrarySearchService cacheService, TrackFactory trackFactory, ValueLibrarySourceFilter valueFilter)
            : this(cacheService, trackFactory)
        {
            _valueFilter = valueFilter;
        }

        public PagedSourceItemsPacket<ITrack> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {
            List<ITrack> tracks;

            if (_idFilter == null && _valueFilter == null)
            {
                tracks = _searchService.GetTracksForRange(pageoffset, count);
            }
            else if (_idFilter != null)
            {
                tracks = _searchService.GetTracksByAttributeId(_idFilter.SearchAreas, _idFilter.Id);
            }
            else
            {
                tracks = _searchService.GetTracksByAttributeValue(_valueFilter.SearchAreas, _valueFilter.Value, pageoffset, count);
            }


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
                return -1;
            }

            return (int)item.Song.Id;
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
