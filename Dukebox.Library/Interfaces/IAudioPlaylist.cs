using Dukebox.Library.Model;
using Dukebox.Model.Services;
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
        ObservableCollection<Track> Tracks { get; }
        Track CurrentlyLoadedTrack { get; }
        int LoadPlaylistFromFile(string filename);
        int LoadPlaylistFromList(List<Track> tracks);
        void SavePlaylistToFile(string filename);
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
