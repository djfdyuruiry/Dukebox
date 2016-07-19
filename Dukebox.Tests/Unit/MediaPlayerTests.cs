using System;
using System.Linq;
using System.Threading;
using FakeItEasy;
using Xunit;
using Dukebox.Audio;
using Dukebox.Audio.Interfaces;

namespace Dukebox.Tests.Unit
{
    public class MediaPlayerTests
    {
        private const long channelLength = 6070;
        private const double channelCurrentSeconds = 121.4;
        private const double channelTotalSeconds = 607;
        private const double channelPercentPlayed = 20;
        private const string audioCurrentMinutes = "02:01";
        private const string audioTotalLengthInMinutes = "10:07";
        private const string errorString = "Error Occurred";

        private string _audioFilename;
        private int _stream;
        private long _channelPosition;
        private bool _isChannelActive;

        private readonly IAudioService _audioService;
        private readonly IAudioCdService _audioCdService;
        private readonly MediaPlayer _mediaPlayer;

        public MediaPlayerTests()
        {
            _audioFilename = "Sample.mp3";
            _stream = 1;
            _channelPosition = 10;
            _isChannelActive = false;

            _audioService = A.Fake<IAudioService>();
            _audioCdService = A.Fake<IAudioCdService>();

            A.CallTo(_audioService)
                .Where(c => c.Method.Name.Equals(nameof(IAudioService.CreateStreamFromFile)))
                .WithReturnType<int>()
                .WithAnyArguments()
                .ReturnsLazily(() =>
                {
                    _isChannelActive = _stream == -1 ? false : true;
                    return _stream;
                });

            A.CallTo(_audioService)
                .Where(c => c.Method.Name.Equals(nameof(IAudioService.CreateStreamFromCd)))
                .WithReturnType<int>()
                .WithAnyArguments()
                .ReturnsLazily(() =>
                {
                    _isChannelActive = _stream == -1 ? false : true;
                    return _stream;
                });

            A.CallTo(_audioService)
                .Where(c => c.Method.Name.Equals(nameof(IAudioService.GetChannelLength)))
                .WithReturnType<long>()
                .WithAnyArguments()
                .Returns(channelLength);

            A.CallTo(_audioService)
                .Where(c => c.Method.Name.Equals(nameof(IAudioService.GetChannelPosition)))
                .WithReturnType<long>()
                .WithAnyArguments()
                .ReturnsLazily(c =>
                {
                    return _channelPosition;
                });

            A.CallTo(_audioService)
                .Where(c => c.Method.Name.Equals(nameof(IAudioService.GetSecondsForChannelPosition)))
                .WithReturnType<double>()
                .WithAnyArguments()
                .ReturnsLazily(c =>
                {
                    var position = (long)c.Arguments[1];

                    return position == _channelPosition ? channelCurrentSeconds : channelTotalSeconds;
                });

            A.CallTo(_audioService)
                .Where(c => c.Method.Name.Equals(nameof(IAudioService.IsChannelActive)))
                .WithReturnType<bool>()
                .WithAnyArguments()
                .ReturnsLazily(() => _isChannelActive);

            A.CallTo(_audioService)
                .Where(c => c.Method.Name.Equals(nameof(IAudioService.GetLastError)))
                .WithReturnType<string>()
                .Returns(errorString);

            A.CallTo(_audioService)
                .Where(c => c.Method.Name.Equals(nameof(IAudioService.StopChannel)))
                .WithReturnType<bool>()
                .ReturnsLazily(() =>
                {
                    _isChannelActive = false;
                    return true;
                });

            A.CallTo(_audioService)
                .Where(c => c.Method.Name.Equals(nameof(IAudioService.SetChannelPosition)))
                .WithReturnType<bool>()
                .WithAnyArguments()
                .ReturnsLazily(c =>
                {
                    var stream = (int)c.Arguments[0];
                    var newPosition = (double)c.Arguments[1];

                    _channelPosition = (long)(newPosition * (channelLength / channelTotalSeconds));

                    return true;
                });

            _mediaPlayer = new MediaPlayer(_audioService, _audioCdService);
        }

        [Fact]
        public void AudioLengthInMins_Should_Return_EmptyString_When_File_Not_Loaded()
        {
            var audioLengthInMins = _mediaPlayer.AudioLengthInMins;

            var valueIsCorrect = audioLengthInMins.Equals(string.Empty);

            Assert.True(valueIsCorrect, "MediaPlayer failed to return correct value for AudioLengthInMins when file not loaded");
        }

        [Fact]
        public void AudioLengthInMins_Should_Return_Correct_Length_When_File_Loaded()
        {
            StartPlayback();

            var audioLengthInMins = _mediaPlayer.AudioLengthInMins;

            var valueIsCorrect = audioLengthInMins.Equals(audioTotalLengthInMinutes);

            Assert.True(valueIsCorrect, "MediaPlayer failed to return correct value for AudioLengthInMins when file loaded");
        }

        [Fact]
        public void AudioLengthInSecs_Should_Return_0_When_File_Not_Loaded()
        {
            var audioLengthInMins = _mediaPlayer.AudioLengthInSecs;

            var valueIsCorrect = audioLengthInMins.Equals(0);

            Assert.True(valueIsCorrect, "MediaPlayer failed to return correct value for AudioLengthInSecs when file not loaded");
        }

        [Fact]
        public void AudioLengthInSecs_Should_Return_Correct_Length_When_File_Loaded()
        {
            StartPlayback();

            var audioLengthInMins = _mediaPlayer.AudioLengthInSecs;

            var valueIsCorrect = audioLengthInMins.Equals(channelTotalSeconds);

            Assert.True(valueIsCorrect, "MediaPlayer failed to return correct value for AudioLengthInSecs when file loaded");
        }

        [Fact]
        public void AudioLoaded_Should_Return_False_When_File_Not_Loaded()
        {
            var audioLoaded = _mediaPlayer.AudioLoaded;

            Assert.False(audioLoaded, "MediaPlayer failed to return false for AudioLoaded when file not loaded");
        }

        [Fact]
        public void AudioLoaded_Should_Return_True_When_File_Loaded()
        {
            StartPlayback();

            var audioLoaded = _mediaPlayer.AudioLoaded;

            Assert.True(audioLoaded, "MediaPlayer failed to return true for AudioLoaded when file loaded");
        }
        [Fact]
        public void MinutesPlayed_Should_Return_EmptyString_When_File_Not_Loaded()
        {
            var minutesPlayed = _mediaPlayer.MinutesPlayed;

            var valueIsCorrect = minutesPlayed.Equals(string.Empty);

            Assert.True(valueIsCorrect, "MediaPlayer failed to return correct value for MinutesPlayed when file not loaded");
        }

        [Fact]
        public void MinutesPlayed_Should_Return_Correct_Minutes_When_File_Loaded()
        {
            StartPlayback();

            var minutesPlayed = _mediaPlayer.MinutesPlayed;

            var valueIsCorrect = minutesPlayed.Equals(audioCurrentMinutes);

            Assert.True(valueIsCorrect, "MediaPlayer failed to return correct value for MinutesPlayed when file loaded");
        }

        [Fact]
        public void PercentagePlayed_Should_Return_0_When_File_Not_Loaded()
        {
            var percentPlayed = _mediaPlayer.PercentagePlayed;

            var valueIsCorrect = percentPlayed.Equals(0);

            Assert.True(valueIsCorrect, "MediaPlayer failed to return correct value for PercentagePlayed when file not loaded");
        }

        [Fact]
        public void PercentagePlayed_Should_Return_Correct_Percentage_When_File_Loaded()
        {
            StartPlayback();

            var percentPlayed = _mediaPlayer.PercentagePlayed;

            var valueIsCorrect = percentPlayed.Equals(channelPercentPlayed);

            Assert.True(valueIsCorrect, "MediaPlayer failed to return correct value for PercentagePlayed when file loaded");
        }

        [Fact]
        public void Playing_Should_Return_False_When_File_Not_Loaded()
        {
            var playing = _mediaPlayer.Playing;

            Assert.False(playing, "MediaPlayer failed to return false for Playing when file not loaded");
        }

        [Fact]
        public void Playing_Should_Return_False_When_File_Loaded_And_Paused()
        {
            StartPlayback();
            PausePlayback();

            var playing = _mediaPlayer.Playing;

            Assert.False(playing, "MediaPlayer failed to return false for Playing when file loaded and paused");
        }

        [Fact]
        public void Playing_Should_Return_False_When_File_Loaded_And_Stopped()
        {
            StartPlayback();
            StopPlayback();

            var playing = _mediaPlayer.Playing;

            Assert.False(playing, "MediaPlayer failed to return false for Playing when file loaded and stopped");
        }

        [Fact]
        public void Playing_Should_Return_False_When_File_Loaded_And_Playback_Ends()
        {
            StartPlayback();
            EndPlayback();

            var playing = _mediaPlayer.Playing;

            Assert.False(playing, "MediaPlayer failed to return false for Playing when file loaded and playback ends");
        }

        [Fact]
        public void Playing_Should_Return_True_When_File_Loaded()
        {
            StartPlayback();

            var audioLoaded = _mediaPlayer.Playing;

            Assert.True(audioLoaded, "MediaPlayer failed to return true for Playing when file loaded");
        }

        [Fact]
        public void SecondsPlayed_Should_Return_0_When_File_Not_Loaded()
        {
            var secondsPlayed = _mediaPlayer.SecondsPlayed;

            var valueIsCorrect = secondsPlayed.Equals(0);

            Assert.True(valueIsCorrect, "MediaPlayer failed to return correct value for SecondsPlayed when file not loaded");
        }

        [Fact]
        public void SecondsPlayed_Should_Return_Correct_Seconds_When_File_Loaded()
        {
            StartPlayback();

            var secondsPlayed = _mediaPlayer.SecondsPlayed;

            var valueIsCorrect = secondsPlayed.Equals(channelCurrentSeconds);

            Assert.True(valueIsCorrect, "MediaPlayer failed to return correct value for SecondsPlayed when file loaded");
        }

        [Fact]
        public void Stopped_Should_Return_False_When_File_Not_Loaded()
        {
            var stopped = _mediaPlayer.Stopped;

            Assert.False(stopped, "MediaPlayer failed to return false for Stopped when file not loaded");
        }

        [Fact]
        public void Stopped_Should_Return_False_When_File_Loaded()
        {
            StartPlayback();

            var stopped = _mediaPlayer.Stopped;

            Assert.False(stopped, "MediaPlayer failed to return true for Stopped when file loaded");
        }

        [Fact]
        public void Stopped_Should_Return_True_When_File_Loaded_And_Stopped()
        {
            StartPlayback();
            StopPlayback();

            var stopped = _mediaPlayer.Stopped;

            Assert.True(stopped, "MediaPlayer failed to return true for Stopped when file loaded");
        }

        [Fact]
        public void Finished_Should_Return_False_When_File_Not_Loaded()
        {
            var finished = _mediaPlayer.Finished;

            Assert.False(finished, "MediaPlayer failed to return false for Finished when file not loaded");
        }

        [Fact]
        public void Finished_Should_Return_False_When_File_Loaded()
        {
            StartPlayback();

            var finished = _mediaPlayer.Finished;

            Assert.False(finished, "MediaPlayer failed to return false for Finished when file not loaded");
        }

        [Fact]
        public void Finished_Should_Return_True_When_File_Loaded_And_Stopped()
        {
            StartPlayback();
            StopPlayback();

            var finished = _mediaPlayer.Finished;

            Assert.True(finished, "MediaPlayer failed to return true for Finished when file loaded");
        }

        [Fact]
        public void Finished_Should_Return_True_When_File_Loaded_And_Playback_Ends()
        {
            StartPlayback();
            EndPlayback();

            var finished = _mediaPlayer.Finished;

            Assert.True(finished, "MediaPlayer failed to return true for Finished when file loaded");
        }

        [Fact]
        public void Error_Handler_Should_Be_Called_When_Audio_File_Fails_To_Load()
        {
            var errorHandlerCalled = false;
            var signalEvent = new ManualResetEvent(false);

            _mediaPlayer.ErrorHandlingAction = (m, e) =>
            {
                errorHandlerCalled = true;
                signalEvent.Set();
            };

            _stream = 0;

            _mediaPlayer.LoadFile(_audioFilename);

            signalEvent.WaitOne(500);

            Assert.True(errorHandlerCalled, "MediaPlayer failed to call error handler when file failed to load");
        }

        [Fact]
        public void Error_Handler_Should_Provide_Audio_Error_String()
        {
            var errorStringProvided = string.Empty;
            var signalEvent = new ManualResetEvent(false);

            _mediaPlayer.ErrorHandlingAction = (m, e) =>
            {
                errorStringProvided = m;
                signalEvent.Set();
            };

            _stream = 0;

            _mediaPlayer.LoadFile(_audioFilename);

            signalEvent.WaitOne(500);

            var errorStringCorrect = errorStringProvided.Contains(errorString);

            Assert.True(errorStringCorrect, "MediaPlayer failed to provide audio error string to error handler");
        }

        [Fact]
        public void LoadFile_Should_Throw_Exception_On_Non_Existing_Path()
        {
            ArgumentException loadException = null;

            _audioFilename = @"C:\AwesomeSong.mp3";

            try
            {
                StartPlayback();
            }
            catch (ArgumentException ex)
            {
                loadException = ex;
            }

            var loadExceptionDetected = loadException != null;

            Assert.True(loadExceptionDetected, "MediaPlayer failed to throw exception when attempting to load non existing path");
        }

        [Fact]
        public void ChangeAudioPosition_Should_Change_Audio_Positon_When_File_Loaded()
        {
            StartPlayback();
            _mediaPlayer.ChangeAudioPosition(60);

            Thread.Sleep(250);

            var channelPositionIsCorrect = !_mediaPlayer.SecondsPlayed.Equals(60);

            Assert.True(channelPositionIsCorrect, "MediaPlayer failed to change audio position when file loaded");            
        }

        [Fact]
        public void ChangeAudioPosition_Should_Not_Change_Audio_Positon_When_File_Not_Loaded()
        {
            ChangeAudioPosition(60);
            
            var channelPositionIsCorrect = _mediaPlayer.SecondsPlayed.Equals(0);

            Assert.True(channelPositionIsCorrect, "MediaPlayer failed to change audio position when file loaded");
        }

        private void StartPlayback()
        {
            var signalEvent = new ManualResetEvent(false);

            _mediaPlayer.StartPlayingTrack += (o, e) => signalEvent.Set();

            _mediaPlayer.LoadFile(_audioFilename);

            signalEvent.WaitOne(500);
        }

        private void PausePlayback()
        {
            var signalEvent = new ManualResetEvent(false);

            _mediaPlayer.TrackPaused += (o, e) => signalEvent.Set();

            _mediaPlayer.PausePlayAudio();

            signalEvent.WaitOne(500);
        }

        private void StopPlayback()
        {
            var signalEvent = new ManualResetEvent(false);

            _mediaPlayer.FinishedPlayingTrack += (o, e) => signalEvent.Set();

            _mediaPlayer.StopAudio();

            signalEvent.WaitOne(500);
        }

        private void EndPlayback()
        {
            var signalEvent = new ManualResetEvent(false);

            _mediaPlayer.FinishedPlayingTrack += (o, e) => signalEvent.Set();

            _isChannelActive = false;

            signalEvent.WaitOne(500);
        }

        private void ChangeAudioPosition(double seconds)
        {
            var signalEvent = new ManualResetEvent(false);

            _mediaPlayer.AudioPositionChanged += (o, e) => signalEvent.Set();

            _mediaPlayer.ChangeAudioPosition(seconds);

            signalEvent.WaitOne(500);
        }
    }
}
