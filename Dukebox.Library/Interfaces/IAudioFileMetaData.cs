using System.Drawing;

namespace Dukebox.Library.Interfaces
{
    public interface IAudioFileMetaData
    {
        string Album { get; set; }
        Image AlbumArt { get; }
        string Artist { get; set; }
        bool HasAlbumArt { get; }
        bool HasFutherMetadataTag { get; }
        int Length { get; }
        string Title { get; set; }
        void CommitChangesToFile();
    }
}