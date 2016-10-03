using System.Collections.Generic;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibrarySearchService
    {
        int GetSongCount();
        List<ITrack> GetTracksForRange(int start, int count);
        ITrack GetTrackFromLibraryOrFile(string filename);
        List<ITrack> GetTracksByAttributeId(SearchAreas attribute, string attributeId);
        RangedTrackSearchResults GetTracksByAttributeId(SearchAreas attribute, string attributeId, int numberToSkip, int maxResults); 
        List<ITrack> GetTracksByAttributeValue(SearchAreas attribute, string attributeValue);
        RangedTrackSearchResults GetTracksByAttributeValue(SearchAreas attribute, string attributeValue, int numberToSkip, int maxResults);
        List<ITrack> SearchForTracks(string searchTerm, List<SearchAreas> searchAreas);
        List<ITrack> SearchForTracksInArea(SearchAreas attribute, string nameOrTitle);
    }
}