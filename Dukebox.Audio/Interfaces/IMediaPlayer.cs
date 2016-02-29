using System;
using Un4seen.Bass.Misc;

namespace Dukebox.Audio.Interfaces
{
    public interface IMediaPlayer
    {
        string AudioLengthInMins { get; }
        double AudioLengthInSecs { get; }
        bool AudioLoaded { get; }
        string MinutesPlayed { get; }
        double PercentagePlayed { get; }
        bool Playing { get; set; }
        double SecondsPlayed { get; }
        bool Stopped { get; set; }
        bool Finished { get; set; }
        void LoadFile(string fileName);
        void PausePlayAudio();
        void StopAudio();
        void ChangeAudioPosition(double newPosition);
        string GetMinutesFromChannelPosition(int stream, long channelPosition);
        double GetSecondsFromChannelPosition(int stream, long channelPosition);
    }
}
