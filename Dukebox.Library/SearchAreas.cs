using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library
{
    /// <summary>
    /// Areas of the music library which can be
    /// targeted for a search. 
    /// 
    /// Used in 'MusicLibrary.SearchForTracks(..)'.
    /// </summary>
    public enum SearchAreas { All, Song, Artist, Album }
}
