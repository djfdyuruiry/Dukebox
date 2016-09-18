using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using FakeItEasy;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.Services
{
    public class LibraryOrFileTracksSource : ILibraryOrFileTracksSource
    {
        private readonly List<string> _tracks;
        private readonly IMusicLibrarySearchService _searchService;

        public int Count
        {
            get
            {
                return _tracks.Count;
            }
        }

        public LibraryOrFileTracksSource(List<string> tracks, IMusicLibrarySearchService searchService)
        {
            _tracks = tracks;
            _searchService = searchService;
        }

        public PagedSourceItemsPacket<ITrack> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {
            var tracks = _tracks.GetRange(pageoffset, count).Select(t => _searchService.GetTrackFromLibraryOrFile(t)).ToList();

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
            return _tracks.IndexOf(item.Song.FileName);
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
