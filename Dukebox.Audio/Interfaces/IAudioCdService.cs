using System;

namespace Dukebox.Audio.Interfaces
{
    public interface IAudioCdService
    {
        int GetCdDriveIndex(char driveLetter);
        int GetTrackNumberFromCdaFilename(string fileName);
    }
}
