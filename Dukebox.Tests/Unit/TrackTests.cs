using System.IO;
using System.Threading;
using FakeItEasy;
using Xunit;
using Dukebox.Library.Model;
using Dukebox.Library.Services;
using Dukebox.Configuration.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Audio.Interfaces;
using Dukebox.Library.Factories;

namespace Dukebox.Tests.Unit
{
    public class TrackTests
    {
        [Fact]
        public void Artist()
        {
            var song = new Song { ArtistName = "artist", FileName = "C:/some.mp3" };
            var track = BuildTrack(song);

            var artist = track.Artist;
            
            var artistNameIsCorrect = artist.Name == "artist";

            Assert.True(artistNameIsCorrect, "Artist returned by Track was incorrect");
        }

        [Fact]
        public void Album()
        {
            var song = new Song { AlbumName = "album", FileName = "C:/some.mp3" };
            var track = BuildTrack(song);

            var album = track.Album;
            
            var albumNameIsCorrect = album.Name == "album";

            Assert.True(albumNameIsCorrect, "Album returned by Track was incorrect");
        }

        [Fact]
        public void TrackToString()
        {
            var song = new Song { ArtistName = "artist", FileName = "C:/some.mp3", Title = "song" };
            var track = BuildTrack(song);

            var trackString = track.ToString();

            var trackStringIsCorrect = trackString == "artist - song";

            Assert.True(trackStringIsCorrect, string.Format("Track string was incorrect for current track details (track string: '{0}')", trackString));
        }

        //TODO: Refactor test with new sync metadata method
        //[Fact]
        public void SaveMetadataChangesToDisk()
        {
            var saveChangesMp3FileName = string.Format("{0}._saveChangesTest_.mp3", AudioFileMetaDataTests.SampleMp3FileName);
            var audioFileMetadataFactory = new AudioFileMetadataFactory(A.Fake<ICdMetadataService>(), A.Fake<IAudioCdService>());
            var newArtistName = "funky artist";
            var newAlbumName = "funky album";
            var newTitle = "funky title";

            File.Copy(AudioFileMetaDataTests.SampleMp3FileName, saveChangesMp3FileName, true);

            var song = new Song { ArtistName = "artist", FileName = saveChangesMp3FileName, Title = "song" };
            var track = BuildTrack(song);
            var signalEvent = new ManualResetEvent(false);
            var numChangesSaved = 0;

            track.Song.Title = newTitle;
            track.AlbumName = newAlbumName;
            track.ArtistName = newArtistName;

            signalEvent.WaitOne(1000);

            var audioFileMetadata = audioFileMetadataFactory.BuildAudioFileMetadataInstance(saveChangesMp3FileName);

            var metadataCorrect = audioFileMetadata.Title == newTitle && audioFileMetadata.Artist == newArtistName && audioFileMetadata.Album == newAlbumName;

            Assert.True(metadataCorrect, "Failed to automatically save correct audio file metadata to disk on property update");
        }

        private Track BuildTrack(Song song)
        {
            var settings = A.Fake<IDukeboxSettings>();

            A.CallTo(() => settings.TrackDisplayFormat).Returns("{artist} - {title}");

            var audioFileMetadataFactory = new AudioFileMetadataFactory(A.Fake<ICdMetadataService>(), A.Fake<IAudioCdService>());

            return new Track(song, settings, audioFileMetadataFactory);
        }
    }
}
