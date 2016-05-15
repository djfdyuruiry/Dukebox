using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library.Interfaces
{
    public interface IAudioFileMetadata
    {
        string AudioFilePath { get; }
        string Title { get; set; }
        string Album { get; set; }
        Image AlbumArt { get; }
        string Artist { get; set; }
        bool HasAlbumArt { get; }
        bool HasFutherMetadataTag { get; }
        bool IsEmpty { get; }
        int Length { get; }
        Image GetAlbumArt(Action<Image> beforeStreamClosedCallback = null);
        string SaveAlbumArtToTempFile();
    }
}
