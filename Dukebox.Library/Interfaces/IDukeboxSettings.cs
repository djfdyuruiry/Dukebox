using System;
namespace Dukebox.Library.Interfaces
{
    public interface IDukeboxSettings
    {
        int AddDirectoryConcurrencyLimit { get; }
        string AlbumArtCachePath { get; }
        int BassPluginCount { get; }
        string BassLicenseEmail { get; }
        string BassLicenseKey { get; }
        string BassAddOnsPath { get; }
        string TrackDisplayFormat { get; }
    }
}
