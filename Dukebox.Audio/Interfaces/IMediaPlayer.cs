using System;
using Dukebox.Audio.Model;

namespace Dukebox.Audio.Interfaces
{
    public interface IMediaPlayer : IDisposable
    {
        string AudioLengthInMins { get; }
        double AudioLengthInSecs { get; }
        bool AudioLoaded { get; }
        string MinutesPlayed { get; }
        double PercentagePlayed { get; }
        bool Playing { get; }
        double SecondsPlayed { get; }
        bool Stopped { get; }
        bool Finished { get; }
        Action<string, string> ErrorHandlingAction { get; set; }
        event EventHandler StartPlayingTrack;
        event EventHandler TrackPaused;
        event EventHandler TrackResumed;
        event EventHandler FinishedPlayingTrack;
        event EventHandler<TrackLoadedFromFileEventArgs> LoadedTrackFromFile;
        event EventHandler AudioPositionChanged;
        void LoadFile(string fileName);
        void LoadFile(string fileName, MediaPlayerMetadata metadata);
        void PausePlayAudio();
        void StopAudio();
        void ChangeAudioPosition(double newPositionInSeconds);
    }
}
