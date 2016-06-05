using System.Collections.Generic;
using System.IO;
using FakeItEasy;
using Newtonsoft.Json;
using Xunit;
using Dukebox.Audio.Interfaces;
using Dukebox.Audio.Model;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Services;

namespace Dukebox.Tests.Unit
{
    public class AudioPlaylistTests
    {
        private const string sampleMp3FileName = "sample.mp3";

        private readonly AudioPlaylist _audioPlaylist;
        private readonly IMediaPlayer _mediaPlayer;
        private readonly IMusicLibrary _musicLibrary;
        private readonly ITrack _track;

        private bool _mediaPlayerIsPlaying = false;

        public AudioPlaylistTests()
        {
            _musicLibrary = A.Fake<IMusicLibrary>();
            _mediaPlayer = A.Fake<IMediaPlayer>();

            A.CallTo(() => _mediaPlayer.LoadFile(A<string>.Ignored, A<MediaPlayerMetadata>.Ignored)).Invokes((o) =>_mediaPlayerIsPlaying = true);
            A.CallTo(() => _mediaPlayer.Playing).ReturnsLazily(e => _mediaPlayerIsPlaying);

            _audioPlaylist = new AudioPlaylist(_musicLibrary, _mediaPlayer);
            _track = A.Fake<ITrack>();

            A.CallTo(() => _track.Song).Returns(new Song { FileName = sampleMp3FileName });

            _audioPlaylist.Tracks.Add(_track);
        }
        
        [Fact]
        public void ClearPlaylist()
        {
            A.CallTo(() => _mediaPlayer.Finished).Returns(true);

            _audioPlaylist.StartPlaylistPlayback();

            _audioPlaylist.ClearPlaylist();

            A.CallTo(() => _mediaPlayer.StopAudio()).MustHaveHappened();
        }

        [Fact]
        public void GetCurrentTrackIndex()
        {
            A.CallTo(() => _mediaPlayer.Finished).Returns(true);

            _audioPlaylist.StartPlaylistPlayback();
            
            _audioPlaylist.StopPlaylistPlayback();

            var currentIndex = _audioPlaylist.GetCurrentTrackIndex();
            var currentIndexIsCorrect = currentIndex == 0;

            Assert.True(currentIndexIsCorrect, "Audio playlist reported an incorrect current track index");
        }
        
        [Fact]
        public void LoadPlaylistFromList()
        {
            A.CallTo(() => _mediaPlayer.Finished).Returns(true);
            
            var count = _audioPlaylist.LoadPlaylistFromList(new List<ITrack> { _track });
            _audioPlaylist.WaitForPlaybackToFinish();

            var countCorrect = count == 1;

            Assert.True(countCorrect, "Audio playlist returned incorrect count after loading from track list");
            
            A.CallTo(() => _mediaPlayer.LoadFile(A<string>.Ignored, A<MediaPlayerMetadata>.Ignored)).WithAnyArguments().MustHaveHappened();
        }
        
        [Fact]
        public void PausePlay()
        {
            A.CallTo(() => _mediaPlayer.Finished).Returns(true);
            
            _audioPlaylist.StartPlaylistPlayback();
            _audioPlaylist.PausePlay();

            _audioPlaylist.StopPlaylistPlayback();

            A.CallTo(() => _mediaPlayer.PausePlayAudio()).MustHaveHappened();
        }

        [Fact]
        public void SavePlaylistToFile()
        {
            var playlistFile = "playlist.jpl";

            File.Delete(playlistFile);

            _audioPlaylist.SavePlaylistToFile(playlistFile);

            var playlistFileGenerated = File.Exists(playlistFile);

            Assert.True(playlistFileGenerated, "Audio playlist failed to generate playlist file");

            var playlistText = File.ReadAllText(playlistFile);
            var playlistJson = JsonConvert.SerializeObject(new List<string> { sampleMp3FileName });

            var playlistFileIsCorrect = playlistText == playlistJson;

            Assert.True(playlistFileIsCorrect, "Audio playlist generated an incorrect playlist file");
        }

        [Fact]
        public void SkipToTrack()
        {
            A.CallTo(() => _mediaPlayer.Finished).Returns(true);
            
            _audioPlaylist.SkipToTrack(0);

            _audioPlaylist.WaitForPlaybackToFinish();

            A.CallTo(() => _mediaPlayer.LoadFile(A<string>.Ignored, A<MediaPlayerMetadata>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void StartPlaylistPlayback()
        {
            A.CallTo(() => _mediaPlayer.Finished).Returns(true);
            
            _audioPlaylist.StartPlaylistPlayback();

            _audioPlaylist.WaitForPlaybackToFinish();

            A.CallTo(() => _mediaPlayer.LoadFile(A<string>.Ignored, A<MediaPlayerMetadata>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void Stop()
        {
            A.CallTo(() => _mediaPlayer.StopAudio()).Invokes(() => _mediaPlayer.Finished = true);
            
            _audioPlaylist.StartPlaylistPlayback();
            _audioPlaylist.Stop();

            _audioPlaylist.StopPlaylistPlayback();

            A.CallTo(() => _mediaPlayer.StopAudio()).MustHaveHappened();
        }

        [Fact]
        public void StopPlaylistPlayback()
        {
            A.CallTo(() => _mediaPlayer.Finished).Returns(true);

            _audioPlaylist.StartPlaylistPlayback();
            _audioPlaylist.StopPlaylistPlayback();

            A.CallTo(() => _mediaPlayer.StopAudio()).MustHaveHappened();
        }
    }
}
