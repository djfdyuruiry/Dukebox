using Dukebox.Library.Model;
using Dukebox.Model.Services;
using System;
using System.Collections.Generic;

namespace Dukebox.Library.Interfaces
{
    public interface IAudioPlaylist : IDisposable
    {
        bool PlayingAudio { get; }
        bool Repeat { get; set; }
        bool RepeatAll { get; set; }
        bool Shuffle { get; set; }
        List<Track> Tracks { get; set; }
        bool StreamingPlaylist { get; }
        Track CurrentlyLoadedTrack { get; }
        bool TrackLoaded { get; }
        event EventHandler<NewTrackLoadedEventArgs> NewTrackLoaded;
        event EventHandler<TrackListAccessEventArgs> TrackListAccess;
        void SavePlaylistToFile(string filename);
        int LoadPlaylistFromFile(string filename);
        void SkipToTrack(Track trackToPlay);
        void SkipToTrack(int trackIndex);
        void StartPlaylistPlayback();
        void PausePlay();
        void Stop();
        void Back();
        void Forward();
        int GetCurrentTrackIndex();
        void StopPlaylistPlayback();
    }
}
