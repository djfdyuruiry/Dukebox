using System.Collections.Generic;
using Dukebox.Library.Interfaces;

namespace Dukebox.Library.Model
{
    public class RangedTrackSearchResults
    {
        public List<ITrack> RangedResults { get; set; }
        public List<int> FullResultSetIds { get; set; }
    }
}
