using System.Linq;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using FakeItEasy;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Factories;
using System;
using System.Diagnostics;

namespace Dukebox.Desktop.Services
{
    public class LibraryTrackSource : ILibraryTracksSource
    {
        private readonly IMusicLibrarySearchService _searchService;
        private readonly TrackFactory _trackFactory;

        public int Count
        {
            get
            {
                return _searchService.GetSongCount();
            }
        }

        public LibraryTrackSource(IMusicLibrarySearchService cacheService, TrackFactory trackFactory)
        {
            _searchService = cacheService;
            _trackFactory = trackFactory;
        }

        public PagedSourceItemsPacket<ITrack> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {
            var tracks = _searchService.GetTracksForRange(pageoffset, count);

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
