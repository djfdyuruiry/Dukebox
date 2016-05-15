using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using Newtonsoft.Json;
using Dukebox.Audio.Interfaces;
using Dukebox.Audio.Model;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using System.Diagnostics;

namespace Dukebox.Library.Services
{
    /// <summary>
    /// Provides facilities to create a playlist of
    /// audio tracks and manipulate it's flow of playback.
    /// </summary>
    public class AudioPlaylist : IAudioPlaylist
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMusicLibrary _musicLibrary;
        private readonly IMediaPlayer _mediaPlayer;
        
        #region Playback management fields

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

        #region Playback management fields

        private Stack<int> _previousTracks;
        private int _lastTrack;

        #endregion

        #region Playback state properties

        /// <summary>
        /// The list of tracks in the playlist.
        /// </summary>
        public ObservableCollection<ITrack> Tracks { get; private set; }
        
        /// <summary>
        /// The track currently loaded into memory for playback.
        /// </summary>
        public ITrack CurrentlyLoadedTrack
        {
            get
            {
                try
                {
                    return Tracks[GetCurrentTrackIndex()];
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Is the playlist currently loaded in memory for playback?
        /// </summary>
        public bool StreamingPlaylist { get { return Tracks.Count > 0 && _playlistManagerThread != null; } }
        /// <summary>
        /// Is an audio file being played?
        /// </summary>
        public bool PlayingAudio { get { return _mediaPlayer.Playing; } }
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

        #region Event handlers

        public event EventHandler<NewTrackLoadedEventArgs> NewTrackLoaded;
        public event EventHandler<TrackListAccessEventArgs> TrackListModified;
        
        /// <summary>
        /// Get the current index in the playlist that is loaded into memory for playback.
        /// </summary>
        public int GetCurrentTrackIndex()
        {
            _currentTrackIndexMutex.WaitOne();

            var v = _currentTrackIndex;

            _currentTrackIndexMutex.ReleaseMutex();

            return v;
        }

        /// <summary>
        /// Get the current index in the playlist that is loaded into memory for playback.
        /// </summary>
        /// <param name="value">The new value to assign</param>
        private void SetCurrentTrackIndex(int value, bool skippingTrack = false)
        {
            _currentTrackIndexMutex.WaitOne();

            _currentTrackIndex = value;

            _currentTrackIndexMutex.ReleaseMutex();

            CallNewTrackEventHandlers(skippingTrack);
        }

        /// <summary>
        /// 
        /// </summary>
        private void CallNewTrackEventHandlers(bool skippingTrack)
        {
            if (!skippingTrack && Tracks.Count > 0 && (_currentTrackIndex > -1 && _currentTrackIndex < Tracks.Count))
            {
                // Register track as recently played with music library and
                // load current track into media player.
                var currentTrack = Tracks[_currentTrackIndex];

                _musicLibrary.RecentlyPlayed.Add(currentTrack);

                if (NewTrackLoaded != null)
                {
                    var newTrackArgs = new NewTrackLoadedEventArgs
                    {
                        Track = Tracks[_currentTrackIndex],
                        TrackIndex = _currentTrackIndex
                    };

                    NewTrackLoaded(this, newTrackArgs);
                }
            }
        }

        #endregion

        /// <summary>
        /// Create a new playlist instance. All boolean
        /// playback flow control options default to false.
        /// </summary>
        public AudioPlaylist(IMusicLibrary musicLibrary, IMediaPlayer mediaPlayer)
        {
            _musicLibrary = musicLibrary;
            _mediaPlayer = mediaPlayer;

            Tracks = new ObservableCollection<ITrack>();
            Tracks.CollectionChanged += (o, e) => CallTrackModifiedHandler();

            _previousTracks = new Stack<int>();
            _currentTrackIndexMutex = new Mutex();

            SetCurrentTrackIndex(-1);
            _forward = false;
            _back = false;

            Shuffle = false;
            RepeatAll = false;
            Repeat = false;
        }

        private void CallTrackModifiedHandler()
        {
            TrackListModified?.Invoke(this, new TrackListAccessEventArgs { TrackListSize = Tracks.Count });
        }

        /// <summary>
        /// Start playlist playback if the StreamingPlaylist
        /// property is false.
        /// </summary>
        public void StartPlaylistPlayback()
        {
            if (!StreamingPlaylist)
            {
                SetCurrentTrackIndex(0);
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
                _mediaPlayer.StopAudio();

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
                _mediaPlayer.PausePlayAudio();
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
                _mediaPlayer.StopAudio();
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

                // WTF does this even do...
                while (GetCurrentTrackIndex() != 0)
                {
                    Thread.Sleep(10);
                }
            }

            if (trackIndex == 0)
            {
                StopPlaylistPlayback();
                StartPlaylistPlayback();
            }
            else if (trackIndex == GetCurrentTrackIndex())
            {
                SetCurrentTrackIndex(GetCurrentTrackIndex() - 1);
                _forward = true;
            }
            else
            {
                SetCurrentTrackIndex(trackIndex - 1);
                _forward = true;
            }

        }

        /// <summary>
        /// Skip to the given track in the playlist collection
        /// and start playback if not already begun. No action
        /// occurs if track is not in the current playlist.
        /// </summary>
        /// <param name="trackIndex">The track to skip to.</param>
        public void SkipToTrack(ITrack trackToPlay)
        {
            int idx = Tracks.IndexOf(trackToPlay);

            // Skip only if track is found in collection.
            if (idx != -1)
            {
                SkipToTrack(idx);
            }
        }

        /// <summary>
        /// Preform an iteration of the play all tracks loop.
        /// </summary>
        /// <param name="random">Random number generator to be used if shuffle is on.</param>
        private void PlayAllTracksIteration(Random random)
        {
            if (!_forward && !_back)
            {
                // Load current track into media player.
                var currentTrack = Tracks[GetCurrentTrackIndex()];
                var mediaPlayMetadata = new MediaPlayerMetadata
                {
                    AlbumName = currentTrack.Album?.name,
                    ArtistName = currentTrack.Artist?.name,
                    TrackName = currentTrack.Song.title
                };

                _mediaPlayer.LoadFile(currentTrack.Song.filename, mediaPlayMetadata);

                // Wait until media player thread has started playback.
                while (!_mediaPlayer.Playing)
                {
                    Thread.Sleep(10);
                }

                if (GetCurrentTrackIndex() != _lastTrack)
                {
                    _previousTracks.Push(GetCurrentTrackIndex());
                }

                _lastTrack = GetCurrentTrackIndex();
            }

            // While next and back are not pressed and the user is not finished with the song.
            while (!_forward && !_back && !_mediaPlayer.Finished)
            {
                Thread.Sleep(10);
            }

            // Move forward if forward motion has been indicated and shuffle is off.
            if (_forward && !Shuffle)
            {
                SetCurrentTrackIndex(GetCurrentTrackIndex() + 1);

                _forward = false;
            }
            else if (_back && !Shuffle) // Move back one song if indicated and shuffle is off.
            {
                SetCurrentTrackIndex(GetCurrentTrackIndex() - 1);

                _back = false;
            }
            else if (Shuffle && (!_back || _previousTracks.Count < 2)) // Pick random track for playback.
            {
                int nextTrack = random.Next(Tracks.Count);

                while (nextTrack == GetCurrentTrackIndex())
                {
                    nextTrack = random.Next(Tracks.Count);
                }

                SetCurrentTrackIndex(nextTrack);

                _forward = false;
                _back = false;
            }
            else if (Shuffle && _back) // Play last played track.
            {
                int nextTrack = _previousTracks.Pop();
                nextTrack = _previousTracks.Pop();

                SetCurrentTrackIndex(nextTrack);

                _back = false;
            }
            else if (!Repeat) // Go forward if no actions have been indicated and repeat is off.
            {
                SetCurrentTrackIndex(GetCurrentTrackIndex() + 1);
            }

            // Keep rolling over to start of playlist if repeat all is on.
            if (RepeatAll)
            {
                if (GetCurrentTrackIndex() >= Tracks.Count)
                {
                    // Roll on to start of playlist.
                    SetCurrentTrackIndex(0);
                }
                else if (GetCurrentTrackIndex() < 0) // Roll off to end of playlist.
                {
                    SetCurrentTrackIndex(Tracks.Count - 1);
                }
            }
        }


        /// <summary>
        /// Start playlist playback of all tracks, following
        /// the boolean flags for shuffle, repeat and repeat
        /// all while traversing the Tracks collection.
        /// </summary>
        private void PlayAllTracks()
        {
            var random = new Random();

            // Set playback flow controls to default values.            
            _back = false;
            _forward = false;

            while (GetCurrentTrackIndex() < Tracks.Count)
            {
                PlayAllTracksIteration(random);
            }

            SetCurrentTrackIndex(0);
        }

        /// <summary>
        /// Replace the current playlist tracks with those
        /// found in the given playlist file.
        /// </summary>
        public int LoadPlaylistFromFile(string filename, bool startPlayback = true)
        {
            var playlist = _musicLibrary.GetPlaylistFromFile(filename);
            var tracks = playlist.GetTracksForPlaylist();

            return LoadPlaylistFromList(tracks, startPlayback);
        }

        public int LoadPlaylistFromList(List<ITrack> tracks, bool startPlayback = true)
        {
            StopPlaylistPlayback();

            Tracks.Clear();
            tracks.ForEach(t => Tracks.Add(t));

            if (startPlayback)
            {
                StartPlaylistPlayback();
            }

            return Tracks.Count;
        }

        public void ClearPlaylist()
        {
            StopPlaylistPlayback();
            Tracks.Clear();
        }

        /// <summary>
        /// Export a JSON playlist file ('.jpl') that represents
        /// the current list of tracks loaded.
        /// </summary>
        public void SavePlaylistToFile(string filename, bool overwriteFile = false)
        {
            if (File.Exists(filename) && !overwriteFile)
            {
                return;
            }

            var jsonTracks = JsonConvert.SerializeObject(Tracks.Select(t => t.Song.filename).ToList());

            File.WriteAllText(filename, jsonTracks);             
        }

        public void WaitForPlaybackToFinish()
        {
            _playlistManagerThread?.Join();
        }

        /// <summary>
        /// Dispose of event handlers and current
        /// track index mutex.
        /// </summary>
        public void Dispose()
        {
            _currentTrackIndexMutex.Dispose();
        }
    }
}
