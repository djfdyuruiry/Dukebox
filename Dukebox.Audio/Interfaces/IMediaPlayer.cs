using Dukebox.Audio.Model;
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
        event EventHandler StartPlayingTrack;
        event EventHandler TrackPaused;
        event EventHandler TrackResumed;
        event EventHandler FinishedPlayingTrack;
        event EventHandler<TrackLoadedFromFileEventArgs> LoadedTrackFromFile;
        event EventHandler AudioPositionChanged;
        void LoadFile(string fileName, MediaPlayerMetadata metadata = null);
        void PausePlayAudio();
        void StopAudio();
        void ChangeAudioPosition(double newPositionInSeconds);
    }
}
