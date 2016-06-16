using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace Dukebox.Library.Model
{
    public static class TagTypeMap
    {
        public static readonly Dictionary<string, TagTypes> FileTypeMap;

        static TagTypeMap()
        {
            FileTypeMap = new Dictionary<string, TagTypes>
            {
                { ".mp3", TagTypes.Id3v2 },
                { ".mp4", TagTypes.Id3v2 },
                { ".ogg", TagTypes.Xiph },
                { ".oga", TagTypes.Xiph },
                { ".ape", TagTypes.Ape },
                { ".m4a", TagTypes.Apple },
                { ".asf", TagTypes.Asf },
                { ".wma", TagTypes.Asf },
                { ".wmv", TagTypes.Asf },
                { ".wav", TagTypes.RiffInfo },
                { ".flac", TagTypes.FlacMetadata },
                { ".aa", TagTypes.AudibleMetadata }
            };
        }
    }
}
