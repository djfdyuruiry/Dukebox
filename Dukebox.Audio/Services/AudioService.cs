using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;
using Dukebox.Audio.Interfaces;

namespace Dukebox.Audio.Services
{
    public class AudioService : IAudioService
    {
        public int CreateStreamFromCd(int drive, int track)
        {
            return BassCd.BASS_CD_StreamCreate(drive, track, BASSFlag.BASS_DEFAULT);
        }

        public double GetSecondsForChannelPosition(int handle, long pos)
        {
            return Bass.BASS_ChannelBytes2Seconds(handle, pos);
        }

        public long GetChannelLength(int handle)
        {
            return Bass.BASS_ChannelGetLength(handle);
        }

        public long GetChannelPosition(int handle)
        {
            return Bass.BASS_ChannelGetPosition(handle);
        }

        public bool IsChannelActive(int handle)
        {
            return Bass.BASS_ChannelIsActive(handle) == BASSActive.BASS_ACTIVE_PLAYING;
        }

        public bool PauseChannel(int handle)
        {
            return Bass.BASS_ChannelPause(handle);
        }

        public bool PlayChannel(int handle, bool restart)
        {
            return Bass.BASS_ChannelPlay(handle, restart);
        }

        public bool SetChannelPosition(int handle, double seconds)
        {
            return Bass.BASS_ChannelSetPosition(handle, seconds);
        }

        public bool StopChannel(int handle)
        {
            return Bass.BASS_ChannelStop(handle);
        }

        public string GetLastError()
        {
            return Bass.BASS_ErrorGetCode().ToString();
        }

        public int CreateStreamFromFile(string file, long offset, long length)
        {
            return Bass.BASS_StreamCreateFile(file, offset, length, BASSFlag.BASS_DEFAULT);
        }

        public bool FreeStream(int handle)
        {
            return Bass.BASS_StreamFree(handle);
        }
    }
}
