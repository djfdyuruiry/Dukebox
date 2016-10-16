using System.Collections.Generic;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Services;
using Dukebox.Library.Factories;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.Factories
{
    public class TrackSourceFactory
    {
        private readonly IMusicLibrarySearchService _searchService;
        private readonly TrackFactory _trackFactory;

        public TrackSourceFactory(IMusicLibrarySearchService searchService, TrackFactory trackFactory)
        {
            _searchService = searchService;
            _trackFactory = trackFactory;
        }

        public ILibraryTracksSource BuildLibraryTracksSource()
        {
            return new LibraryTracksSource(_searchService, _trackFactory);
        }

        public ILibraryTracksSource BuildLibraryTracksSource(IdLibrarySourceFilter idFilter)
        {
            return new LibraryTracksSource(_searchService, _trackFactory, idFilter);
        }

        public ILibraryTracksSource BuildLibraryTracksSource(ValueLibrarySourceFilter valueFilter)
        {
            return new LibraryTracksSource(_searchService, _trackFactory, valueFilter);
        }

        public ILibraryOrFileTracksSource BuildLibraryOrFileTracksSource(List<string> tracks)
        {
            return new LibraryOrFileTracksSource(tracks, _searchService);
        }

        public IMemoryTracksSource BuildMemoryTracksSource(List<ITrack> tracks)
        {
            return new MemoryTracksSource(tracks);
        }
    }
}
