﻿using Dukebox.Library.Model;
using Dukebox.Library.Services;
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
        ObservableCollection<ITrack> Tracks { get; }
        ITrack CurrentlyLoadedTrack { get; }
        int LoadPlaylistFromFile(string filename);
        int LoadPlaylistFromFile(string filename, bool startPlayback);
        int LoadPlaylistFromList(List<ITrack> tracks);
        int LoadPlaylistFromList(List<ITrack> tracks, bool startPlayback);
        void ClearPlaylist();
        void SavePlaylistToFile(string filename);
        void SavePlaylistToFile(string filename, bool overwriteFile);
        void SkipToTrack(ITrack trackToPlay);
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
