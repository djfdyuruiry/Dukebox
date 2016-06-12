using System;
using System.Collections.Generic;
using System.Drawing;

namespace Dukebox.Library.Interfaces
{
    public interface IAudioFileMetadata
    {
        string AudioFilePath { get; }
        string Title { get; set; }
        string Album { get; set; }
        string Artist { get; set; }
        int Length { get; }
        bool HasAlbumArt { get; }
        bool HasFutherMetadataTag { get; }
        Dictionary<string, List<string>> ExtendedMetadata { get; }
        Image GetAlbumArt();
        Image GetAlbumArt(Action<Image> beforeStreamClosedCallback);
        string SaveAlbumArtToTempFile();
    }
}
