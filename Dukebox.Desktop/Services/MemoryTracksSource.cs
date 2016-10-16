using System.Collections.Generic;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using FakeItEasy;
using Dukebox.Library.Interfaces;
using Dukebox.Desktop.Interfaces;

namespace Dukebox.Desktop.Services
{
    public class MemoryTracksSource : IMemoryTracksSource
    {
        private readonly List<ITrack> _tracks;

        public int Count
        {
            get
            {
                return _tracks?.Count ?? 0;
            }
        }        

        public MemoryTracksSource(List<ITrack> tracks)
        {
            _tracks = tracks;
        }
        
        public PagedSourceItemsPacket<ITrack> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {
            var itemsPacket = new PagedSourceItemsPacket<ITrack>
            {
                Items = _tracks.GetRange(pageoffset, count)
            };

            return itemsPacket;
        }

        public int IndexOf(ITrack item)
        {
            return _tracks.IndexOf(item);
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
