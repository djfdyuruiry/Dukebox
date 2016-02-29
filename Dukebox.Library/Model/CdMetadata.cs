using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library.Model
{
    public class CdMetadata
    {
        public string Artist { get; set; }
        public string Album { get; set; }
        public List<string> Tracks { get; set; }
    }
}
