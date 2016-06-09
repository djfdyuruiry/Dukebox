using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using Xunit;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Services;

namespace Dukebox.Tests.Unit
{
    public class TrackTests
    {
        [Fact]
        public void Artist()
        {
            var song = new Song { Artist = new Artist { Id = 0, Name = "artist" }, FileName = "C:/some.mp3" };
            var track = BuildTrack(song);

            var artist = track.Artist;

            var artistIdIsCorrect = artist.Id == 0;
            var artistNameIsCorrect = artist.Name == "artist";
            var artistReturnedIsCorrect = artistIdIsCorrect && artistNameIsCorrect;

            Assert.True(artistReturnedIsCorrect, "Artist returned by Track was incorrect");
        }

        [Fact]
        public void Album()
        {
            var song = new Song { Album = new Album { Id = 0, Name = "album" }, FileName = "C:/some.mp3" };
            var track = BuildTrack(song);

            var album = track.Album;

            var albumIdIsCorrect = album.Id == 0;
            var albumNameIsCorrect = album.Name == "album";
            var albumReturnedIsCorrect = albumIdIsCorrect && albumNameIsCorrect;

            Assert.True(albumReturnedIsCorrect, "Album returned by Track was incorrect");
        }

        [Fact]
        public void TrackToString()
        {
            var song = new Song { Artist = new Artist { Id = 0, Name = "artist" }, FileName = "C:/some.mp3", Title = "song" };
            var track = BuildTrack(song);

            var trackString = track.ToString();

            var trackStringIsCorrect = trackString == "artist - song";

            Assert.True(trackStringIsCorrect, string.Format("Track string was incorrect for current track details (track string: '{0}')", trackString));
        }

        private Track BuildTrack(Song song)
        {
            var settings = A.Fake<IDukeboxSettings>();

            A.CallTo(() => settings.TrackDisplayFormat).Returns("{artist} - {title}");

            return new Track(song, settings);
        }
    }
}
