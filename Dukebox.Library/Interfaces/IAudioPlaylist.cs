using Dukebox.Library.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dukebox.Library.Interfaces
{
    public interface IAudioPlaylist : IDisposable
    {
        event EventHandler<NewTrackLoadedEventArgs> NewTrackLoaded;
        event EventHandler<TrackListAccessEventArgs> TrackListModified;
        bool PlayingAudio { get; }
        bool Repeat { get; set; }
        bool RepeatAll { get; set; }
        bool Shuffle { get; set; }
        bool StreamingPlaylist { get; }
        bool TrackLoaded { get; }
        ObservableCollection<string> Tracks { get; }
        string CurrentlyLoadedTrack { get; }
        int LoadPlaylistFromFile(string filename);
        int LoadPlaylistFromFile(string filename, bool startPlayback);
        int LoadPlaylistFromList(List<string> tracks);
        int LoadPlaylistFromList(List<string> tracks, bool startPlayback);
        void ClearPlaylist();
        void SavePlaylistToFile(string filename);
        void SavePlaylistToFile(string filename, bool overwriteFile);
        void SkipToTrack(string trackToPlay);
        void SkipToTrack(int trackIndex);
        void StartPlaylistPlayback();
        void PausePlay();
        void Stop();
        void Back();
        void Forward();
        int GetCurrentTrackIndex();
        void StopPlaylistPlayback();
        void WaitForPlaybackToFinish();
    }
}
