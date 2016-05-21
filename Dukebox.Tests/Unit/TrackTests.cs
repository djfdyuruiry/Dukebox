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
            var track = BuildTrack();

            track.Song = new Song { ArtistId = 0 };

            var artist = track.Artist;

            var artistIdIsCorrect = artist.Id == 0;
            var artistNameIsCorrect = artist.Name == "artist";
            var artistReturnedIsCorrect = artistIdIsCorrect && artistNameIsCorrect;

            Assert.True(artistReturnedIsCorrect, "Artist returned by Track was incorrect");
        }

        [Fact]
        public void Album()
        {
            var track = BuildTrack();

            track.Song = new Song { AlbumId = 0 };

            var album = track.Album;

            var albumIdIsCorrect = album.Id == 0;
            var albumNameIsCorrect = album.Name == "album";
            var albumReturnedIsCorrect = albumIdIsCorrect && albumNameIsCorrect;

            Assert.True(albumReturnedIsCorrect, "Album returned by Track was incorrect");
        }

        [Fact]
        public new void ToString()
        {
            var track = BuildTrack();

            track.Song = new Song { AlbumId = 0, ArtistId = 0, FileName = "C:/some.mp3", Title = "song" };

            var trackString = track.ToString();

            var trackStringIsCorrect = trackString == "artist - song";

            Assert.True(trackStringIsCorrect, string.Format("Track string was incorrect for current track details (track string: '{0}')", trackString));
        }

        private Track BuildTrack()
        {
            var settings = A.Fake<IDukeboxSettings>();
            var library = A.Fake<IMusicLibrary>();

            var artists = new List<Artist>
            {
                new Artist { Id = 0, Name = "artist" }
            };
            var albums = new List<Album>
            {
                new Album { Id = 0, Name = "album" }
            };

            A.CallTo(() => settings.TrackDisplayFormat).Returns("{artist} - {title}");

            A.CallTo(() => library.GetArtistById(A<long>.Ignored)).ReturnsLazily(o => artists[0]);
            A.CallTo(() => library.GetArtistCount()).Returns(artists.Count);

            A.CallTo(() => library.GetAlbumById(A<long>.Ignored)).ReturnsLazily(o => albums[0]);
            A.CallTo(() => library.GetAlbumCount()).Returns(albums.Count);

            return new Track(settings, library);
        }
    }
}
