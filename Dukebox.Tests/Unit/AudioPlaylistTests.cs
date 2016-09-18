using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
                
        [Fact]
        public void ClearPlaylist()
        {
            var audioPlaylistTuple = PrepareAudioPlaylistFakes();
            var audioPlaylist = audioPlaylistTuple.Item1;
            var mediaPlayer = audioPlaylistTuple.Item2;

            A.CallTo(() => mediaPlayer.Finished).Returns(true);

            audioPlaylist.StartPlaylistPlayback();
            audioPlaylist.ClearPlaylist();

            A.CallTo(() => mediaPlayer.StopAudio()).MustHaveHappened();
        }

        [Fact]
        public void GetCurrentTrackIndex()
        {
            var audioPlaylistTuple = PrepareAudioPlaylistFakes();
            var audioPlaylist = audioPlaylistTuple.Item1;
            var mediaPlayer = audioPlaylistTuple.Item2;

            A.CallTo(() => mediaPlayer.Finished).Returns(true);

            audioPlaylist.StartPlaylistPlayback();
            
            audioPlaylist.StopPlaylistPlayback();

            var currentIndex = audioPlaylist.GetCurrentTrackIndex();
            var currentIndexIsCorrect = currentIndex == 0;

            Assert.True(currentIndexIsCorrect, "Audio playlist reported an incorrect current track index");
        }
        
        [Fact]
        public void LoadPlaylistFromList()
        {
            var audioPlaylistTuple = PrepareAudioPlaylistFakes();
            var audioPlaylist = audioPlaylistTuple.Item1;
            var mediaPlayer = audioPlaylistTuple.Item2;
            var track = BuildTrackFake();

            A.CallTo(() => mediaPlayer.Finished).Returns(true);

            var signalEvent = new ManualResetEvent(false);

            mediaPlayer.LoadedTrackFromFile += (o, e) => signalEvent.Set();

            var count = audioPlaylist.LoadPlaylistFromList(new List<string> { track.Song.FileName });            

            signalEvent.WaitOne(100);

            var countCorrect = count == 1;

            Assert.True(countCorrect, "Audio playlist returned incorrect count after loading from track list");
            
            A.CallTo(() => mediaPlayer.LoadFile(A<string>.Ignored, A<MediaPlayerMetadata>.Ignored)).WithAnyArguments().MustHaveHappened();
        }
        
        [Fact]
        public void PausePlay()
        {
            var audioPlaylistTuple = PrepareAudioPlaylistFakes();
            var audioPlaylist = audioPlaylistTuple.Item1;
            var mediaPlayer = audioPlaylistTuple.Item2;

            A.CallTo(() => mediaPlayer.Finished).Returns(true);
            
            audioPlaylist.StartPlaylistPlayback();
            audioPlaylist.PausePlay();

            audioPlaylist.StopPlaylistPlayback();

            A.CallTo(() => mediaPlayer.PausePlayAudio()).MustHaveHappened();
        }

        [Fact]
        public void SavePlaylistToFile()
        {
            var audioPlaylistTuple = PrepareAudioPlaylistFakes();
            var audioPlaylist = audioPlaylistTuple.Item1;

            var playlistFile = "playlist.jpl";

            File.Delete(playlistFile);

            audioPlaylist.SavePlaylistToFile(playlistFile);

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
            var audioPlaylistTuple = PrepareAudioPlaylistFakes();
            var audioPlaylist = audioPlaylistTuple.Item1;
            var mediaPlayer = audioPlaylistTuple.Item2;

            A.CallTo(() => mediaPlayer.Finished).Returns(true);

            var signalEvent = new ManualResetEvent(false);
            var numLoads = 0;

            mediaPlayer.LoadedTrackFromFile += (o, e) =>
            {
                if (numLoads == 1)
                {
                    signalEvent.Set();
                }

                numLoads++;
            };

            audioPlaylist.SkipToTrack(0);

            signalEvent.WaitOne(100);

            A.CallTo(() => mediaPlayer.LoadFile(A<string>.Ignored, A<MediaPlayerMetadata>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void StartPlaylistPlayback()
        {
            var audioPlaylistTuple = PrepareAudioPlaylistFakes();
            var audioPlaylist = audioPlaylistTuple.Item1;
            var mediaPlayer = audioPlaylistTuple.Item2;

            A.CallTo(() => mediaPlayer.Finished).Returns(true);

            var signalEvent = new ManualResetEvent(false);

            mediaPlayer.LoadedTrackFromFile += (o, e) => signalEvent.Set();

            audioPlaylist.StartPlaylistPlayback();

            signalEvent.WaitOne(100);

            A.CallTo(() => mediaPlayer.LoadFile(A<string>.Ignored, A<MediaPlayerMetadata>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void Stop()
        {
            var audioPlaylistTuple = PrepareAudioPlaylistFakes();
            var audioPlaylist = audioPlaylistTuple.Item1;
            var mediaPlayer = audioPlaylistTuple.Item2;

            A.CallTo(() => mediaPlayer.StopAudio()).Invokes(() => mediaPlayer.Finished = true);
            
            audioPlaylist.StartPlaylistPlayback();
            audioPlaylist.Stop();

            audioPlaylist.StopPlaylistPlayback();

            A.CallTo(() => mediaPlayer.StopAudio()).MustHaveHappened();
        }

        [Fact]
        public void StopPlaylistPlayback()
        {
            var audioPlaylistTuple = PrepareAudioPlaylistFakes();
            var audioPlaylist = audioPlaylistTuple.Item1;
            var mediaPlayer = audioPlaylistTuple.Item2;

            A.CallTo(() => mediaPlayer.Finished).Returns(true);

            audioPlaylist.StartPlaylistPlayback();
            audioPlaylist.StopPlaylistPlayback();

            A.CallTo(() => mediaPlayer.StopAudio()).MustHaveHappened();
        }

        private Tuple<AudioPlaylist, IMediaPlayer> PrepareAudioPlaylistFakes()
        {
            var recentlyPlayedRepo = A.Fake<IRecentlyPlayedRepository>();
            var trackGenerator = A.Fake<ITrackGeneratorService>();
            var playlistGenerator = A.Fake<IPlaylistGeneratorService>();
            var mediaPlayer = A.Fake<IMediaPlayer>();
            var mediaPlayerIsPlaying = false;

            A.CallTo(() => mediaPlayer.LoadFile(A<string>.Ignored, A<MediaPlayerMetadata>.Ignored)).Invokes((o) => mediaPlayerIsPlaying = true);
            A.CallTo(() => mediaPlayer.Playing).ReturnsLazily(e => mediaPlayerIsPlaying);

            var audioPlaylist = new AudioPlaylist(recentlyPlayedRepo, trackGenerator, playlistGenerator, mediaPlayer);
            var track = BuildTrackFake();

            audioPlaylist.Tracks.Add(track.Song.FileName);

            return new Tuple<AudioPlaylist, IMediaPlayer> (audioPlaylist, mediaPlayer);
        }

        private ITrack BuildTrackFake()
        {
            var track = A.Fake<ITrack>();

            A.CallTo(() => track.Song).Returns(new Song { FileName = sampleMp3FileName });

            return track;
        }
    }
}
