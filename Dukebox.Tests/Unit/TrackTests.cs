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

            track.Song = new Song { artistId = 0 };

            var artist = track.Artist;

            var artistIdIsCorrect = artist.id == 0;
            var artistNameIsCorrect = artist.name == "artist";
            var artistReturnedIsCorrect = artistIdIsCorrect && artistNameIsCorrect;

            Assert.True(artistReturnedIsCorrect, "Artist returned by Track was incorrect");
        }

        [Fact]
        public void Album()
        {
            var track = BuildTrack();

            track.Song = new Song { albumId = 0 };

            var album = track.Album;

            var albumIdIsCorrect = album.id == 0;
            var albumNameIsCorrect = album.name == "album";
            var albumReturnedIsCorrect = albumIdIsCorrect && albumNameIsCorrect;

            Assert.True(albumReturnedIsCorrect, "Album returned by Track was incorrect");
        }

        [Fact]
        public new void ToString()
        {
            var track = BuildTrack();

            track.Song = new Song { albumId = 0, artistId = 0, filename = "C:/some.mp3", title = "song" };

            var trackString = track.ToString();

            var trackStringIsCorrect = trackString == "artist - song";

            Assert.True(trackStringIsCorrect, "Track string was incorrect for current track details");
        }

        private Track BuildTrack()
        {
            var settings = A.Fake<IDukeboxSettings>();
            var library = A.Fake<IMusicLibrary>();

            var artists = new List<Artist>
            {
                new Artist { id = 0, name = "artist" }
            };
            var albums = new List<Album>
            {
                new Album { id = 0, name = "album" }
            };

            A.CallTo(() => settings.TrackDisplayFormat).Returns("{artist} - {title}");

            A.CallTo(() => library.GetArtistById(A<long>.Ignored)).ReturnsLazily(o => artists.FirstOrDefault(a => a.id == (long)o.Arguments[0]));
            A.CallTo(() => library.GetArtistCount()).Returns(artists.Count);

            A.CallTo(() => library.GetAlbumById(A<long>.Ignored)).ReturnsLazily(o => albums.FirstOrDefault(a => a.id == (long)o.Arguments[0]));
            A.CallTo(() => library.GetAlbumCount()).Returns(albums.Count);

            return new Track(settings, library);
        }
    }
}
