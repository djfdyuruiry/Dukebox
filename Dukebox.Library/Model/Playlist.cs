using Dukebox.Audio;
using Dukebox.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Dukebox.Model
{
    /// <summary>
    /// Provides facilities to create a playlist of
    /// audio tracks and manipulate it's flow of playback.
    /// 
    /// This class acts as an abstraction layer on top of the
    /// MediaPlayer singleton class.
    /// </summary>
    sealed public class Playlist : IDisposable
    {
        /// <summary>
        /// The list of tracks in the playlist.
        /// </summary>
        private List<Track> _tracks;
        public List<Track> Tracks 
        { 
            get 
            { 
                CallTrackListAccessHandlers();  
                return _tracks; 
            } 
            set 
            { 
                _tracks = value; 
            } 
        }

        #region Playback state properties

        /// <summary>
        /// The track currently loaded into memory for playback.
        /// </summary>
        public Track CurrentlyLoadedTrack 
        { 
            get 
            { 
                try 
                { 
                    return Tracks[CurrentTrackIndex]; 
                } 
                catch (ArgumentOutOfRangeException ex) 
                { 
                    return null; 
                } 
            } 
        }

        /// <summary>
        /// The current index in the playlist that is loaded into memory for playback.
        /// </summary>
        public int CurrentTrackIndex 
        { 
            get 
            { 
                _currentTrackIndexMutex.WaitOne(); 

                var v = _currentTrackIndex;

                _currentTrackIndexMutex.ReleaseMutex(); 

                CallNewTrackEventHandlers();  
                return v; 
            } 
            set 
            { 
                _currentTrackIndexMutex.WaitOne();

                _currentTrackIndex = value; 

                _currentTrackIndexMutex.ReleaseMutex(); 
            } 
        
        }
        /// <summary>
        /// Is the playlist currently loaded in memory for playback?
        /// </summary>
        public bool StreamingPlaylist { get { return Tracks.Count > 0 && _playlistManagerThread != null; } }
        /// <summary>
        /// Is an audio file being played?
        /// </summary>
        public bool PlayingAudio { get { return MediaPlayer.GetInstance().Playing; } }
        /// <summary>
        /// Is a track currently loaded in memory?
        /// </summary>
        public bool TrackLoaded { get { return CurrentlyLoadedTrack != null; } }

        #endregion

        #region Playback flow control properties

        /// <summary>
        /// Shuffle playback so it is in random order?
        /// </summary>
        public bool Shuffle { get; set; }
        /// <summary>
        /// Repeat playlist?
        /// </summary>
        public bool RepeatAll { get; set; }
        /// <summary>
        /// Repeat currently playing track?
        /// </summary>
        public bool Repeat { get; set; }

        #endregion

        #region Playback management properties

        /// <summary>
        /// Main thread for playlist playback.
        /// </summary>
        private Thread _playlistManagerThread;
        /// <summary>
        /// The current index from the Tracks collection loaded into memory.
        /// </summary>
        private int _currentTrackIndex;
        /// <summary>
        /// Go back to the track immediately after current
        /// index in Tracks collection?
        /// </summary>
        private bool _forward;
        /// <summary>
        /// Go back to the track immediately before current
        /// index in Tracks collection?
        /// </summary>
        private bool _back;
        /// <summary>
        /// Mutex that controls access to the current track
        /// index.
        /// </summary>
        private Mutex _currentTrackIndexMutex;

        #endregion

        #region Event handlers

        /// <summary>
        /// 
        /// </summary>
        public List<EventHandler> NewTrackLoadedHandlers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private void CallNewTrackEventHandlers()
        {
            if (_tracks.Count > 0 && (_currentTrackIndex > -1 && _currentTrackIndex < _tracks.Count))
            {
                NewTrackLoadedHandlers.ForEach(a => a.Invoke(this, new NewTrackLoadedEventArgs() { Track = _tracks[_currentTrackIndex], TrackIndex = _currentTrackIndex }));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<EventHandler> TrackListAccessHandlers { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        private void CallTrackListAccessHandlers()
        {
            TrackListAccessHandlers.ForEach(a => a.Invoke(this, new TrackListAccessEventArgs() { TrackListSize = _tracks.Count }));
        }

        #endregion

        /// <summary>
        /// Create a new playlist instance. All boolean
        /// playback flow control options default to false.
        /// </summary>
        public Playlist()
        {
            Tracks = new List<Track>();
            _currentTrackIndexMutex = new Mutex();

            CurrentTrackIndex = -1;
            _forward = false;
            _back = false;

            Shuffle = false;
            RepeatAll = false;
            Repeat = false;

            NewTrackLoadedHandlers = new List<EventHandler>();
            TrackListAccessHandlers = new List<EventHandler>();
        }

        /// <summary>
        /// Start playlist playback if the StreamingPlaylist
        /// property is false.
        /// </summary>
        public void StartPlaylistPlayback()
        {
            if (!StreamingPlaylist)
            {
                _playlistManagerThread = new Thread(PlayAllTracks);
                _playlistManagerThread.Start();
            }
        }

        /// <summary>
        /// Stop playlist playback if the StreamingPlaylist
        /// property is true. 
        /// </summary>
        public void StopPlaylistPlayback()
        {
            if (StreamingPlaylist)
            {
                MediaPlayer.GetInstance().StopAudio();

                _playlistManagerThread.Abort();
                _playlistManagerThread = null;
            }
        }

        /// <summary>
        /// Pause/play currently loaded audio stream if the 
        /// StreamingPlaylist property is true. 
        /// </summary>
        public void PausePlay()
        {
            if (StreamingPlaylist)
            {
                MediaPlayer.GetInstance().PausePlayAudio();
            }
        }

        /// <summary>
        /// Stop currently loaded audio stream if the 
        /// StreamingPlaylist property is true. 
        /// </summary>
        public void Stop()
        {
            if (StreamingPlaylist)
            {
                MediaPlayer.GetInstance().StopAudio();
            }
        }

        /// <summary>
        /// Go to track immediately before current track in playlist
        /// collection if the StreamingPlaylist property is true. 
        /// </summary>
        public void Back()
        {
            if (StreamingPlaylist)
            {
                _back = true;
            }
        }

        /// <summary>
        /// Go to track immediately after current track in playlist
        /// collection if the StreamingPlaylist property is true. 
        /// </summary>
        public void Forward()
        {
            if (StreamingPlaylist)
            {
                _forward = true;
            }
        }

        /// <summary>
        /// Skip to the given track index in the playlist
        /// collection and start playback if not already
        /// begun. No action occurs if index is invalid.
        /// </summary>
        /// <param name="trackIndex">The index in the Tracks collection to skip to.</param>
        public void SkipToTrack(int trackIndex)
        {
            if (trackIndex > Tracks.Count - 1 || trackIndex < 0)
            {
                return;
            }

            if (!StreamingPlaylist)
            {
                StartPlaylistPlayback();

                if (trackIndex == 0)
                {
                    return;
                }

                while (CurrentTrackIndex != 0)
                {
                    Thread.Sleep(10);
                }
            }

            if (trackIndex == 0)
            {
                StopPlaylistPlayback();
                StartPlaylistPlayback();
            }
            else if (trackIndex == CurrentTrackIndex)
            {
                _forward = true;
                CurrentTrackIndex--;
            }
            else
            {
                _forward = true;
                CurrentTrackIndex = trackIndex - 1;
            }
            
        }

        /// <summary>
        /// Skip to the given track in the playlist collection
        /// and start playback if not already begun. No action
        /// occurs if track is not in the current playlist.
        /// </summary>
        /// <param name="trackIndex">The track to skip to.</param>
        public void SkipToTrack(Track trackToPlay)
        {
            int idx = Tracks.IndexOf(trackToPlay);

            // Skip only if track is found in collection.
            if (idx != -1)
            {
                SkipToTrack(idx);
            }
        }

        /// <summary>
        /// Start playlist playback of all tracks, following
        /// the boolean flags for shuffle, repeat and repeat
        /// all while traversing the Tracks collection.
        /// </summary>
        private void PlayAllTracks()
        {
            Random random = new Random();

            // Set playback flow controls to default values.            
            _back = false;
            _forward = false;
            CurrentTrackIndex = 0;

            while (CurrentTrackIndex < Tracks.Count)
            {
                if(!_forward && !_back)
                {
                    // Load current track into media player.
                    MediaPlayer.GetInstance().LoadFile(_tracks[_currentTrackIndex].Song.filename);

                    // Wait until media player thread has started playback.
                    while (!MediaPlayer.GetInstance().Playing)
                    {
                        Thread.Sleep(10);
                    }
                }
                
                // While next and back are not pressed and the user is not finished with the song.
                while (!_forward && !_back && !MediaPlayer.GetInstance().Finished)
                {
                    Thread.Sleep(10);
                }

                // Keep rolling over to start of playlist if repeat all is on.
                if (RepeatAll && !_back)
                {
                    if (CurrentTrackIndex + 1 == _tracks.Count) 
                    {
                        if (!Repeat)
                        {
                            // Roll on to start of playlist if repeat is off.
                            CurrentTrackIndex = -1;
                        }
                    }
                }

                // Move forward if repeat is off or forward
                // motion has been indicated.
                if (!Repeat || _forward)
                {
                    CurrentTrackIndex++;
                }

                // Move back one song.
                if (_back)
                {
                    CurrentTrackIndex -= 2;

                    if (_currentTrackIndex < 0) // Roll off to end of playlist.
                    {
                        CurrentTrackIndex = Tracks.Count - 1;
                    }

                    _back = false;
                }
                else if (Shuffle) // Pick random track for playback.
                {
                    CurrentTrackIndex = random.Next(_tracks.Count);
                }

                // Reset flags for next iteration.
                _back = false;
                _forward = false;               
            }

            CurrentTrackIndex = -1;
            Stop();
        }

        /// <summary>
        /// Replace the current playlist tracks with those
        /// found in the given playlist file.
        /// </summary>
        public int LoadPlaylistFromFile(string filename)
        {
            List<Track> tracksToAdd = MusicLibrary.GetInstance().GetTracksFromPlaylistFile(filename);

            StopPlaylistPlayback();
            Tracks = tracksToAdd;

            return tracksToAdd.Count;
        }

        /// <summary>
        /// Export a JSON playlist file ('.jpl') that represents
        /// the current list of tracks loaded.
        /// </summary>
        public void SavePlaylistToFile(string filename)
        {
            string jsonTracks = JsonConvert.SerializeObject(Tracks.Select(t => t.Song.filename).ToList());
            StreamWriter playlistFile = new StreamWriter(filename, false);

            playlistFile.Write(jsonTracks);
            playlistFile.Close();
        }
        
        /// <summary>
        /// Dispose of event handlers and current
        /// track index mutex.
        /// </summary>
        public void Dispose()
        {
            _currentTrackIndexMutex.Dispose();

            TrackListAccessHandlers.Clear();
            NewTrackLoadedHandlers.Clear();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class NewTrackLoadedEventArgs : EventArgs
    {
        public Track Track { get; set; }
        public int TrackIndex { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TrackListAccessEventArgs : EventArgs
    {
        public int TrackListSize { get; set; }
    }
}
