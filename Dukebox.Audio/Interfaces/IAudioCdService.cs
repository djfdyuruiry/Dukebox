namespace Dukebox.Audio.Interfaces
{
    public interface IAudioCdService
    {
        int GetCdDriveIndex(char driveLetter);
        int GetTrackNumberFromCdaFilename(string fileName);
        bool IsAudioCd(char driveLetter);
    }
}
