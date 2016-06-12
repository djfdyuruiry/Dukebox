using System.Collections.Generic;

namespace Dukebox.Library.Model
{
    public class CdMetadata
    {
        public string Artist { get; set; }
        public string Album { get; set; }
        public List<string> Tracks { get; set; }
    }
}
