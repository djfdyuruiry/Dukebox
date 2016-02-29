using System;
namespace Dukebox.Library.Interfaces
{
    public interface IDukeboxSettings
    {
        int AddDirectoryConcurrencyLimit { get; }
        string AlbumArtCachePath { get; }
        string TrackDisplayFormat { get; }
    }
}
