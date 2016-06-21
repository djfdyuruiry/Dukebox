using System.Collections.Generic;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibrarySearchService
    {
        List<ITrack> GetTracksByAttributeId(SearchAreas attribute, string attributeId);
        List<ITrack> GetTracksByAttributeValue(SearchAreas attribute, string attributeValue);
        List<ITrack> SearchForTracks(string searchTerm, List<SearchAreas> searchAreas);
        List<ITrack> SearchForTracksInArea(SearchAreas attribute, string nameOrTitle);
    }
}