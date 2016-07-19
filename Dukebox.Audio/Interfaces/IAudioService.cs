namespace Dukebox.Audio.Interfaces
{
    public interface IAudioService
    {
        int CreateStreamFromFile(string file, long offset, long length);
        int CreateStreamFromCd(int drive, int track);
        bool PlayChannel(int handle, bool restart);
        bool PauseChannel(int handle);
        bool StopChannel(int handle);
        long GetChannelLength(int handle);
        long GetChannelPosition(int handle);
        bool SetChannelPosition(int handle, double seconds);
        double GetSecondsForChannelPosition(int handle, long pos);
        bool IsChannelActive(int handle);
        bool FreeStream(int handle);
        string GetLastError();
    }
}
