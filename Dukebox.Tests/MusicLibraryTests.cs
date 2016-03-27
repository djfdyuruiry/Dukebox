using Dukebox.Audio;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Repositories;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Dukebox.Tests
{
    public class MusicLibraryTests
    {
        private readonly IMusicLibraryDbContext _musicLibraryDbContext;
        private readonly DbMockDataLoader _mockDataLoader;
        private readonly MusicLibrary _musicLibrary;

        public MusicLibraryTests()
        {
            _musicLibraryDbContext = A.Fake<IMusicLibraryDbContext>();
            _mockDataLoader = new DbMockDataLoader(_musicLibraryDbContext);
            
            _mockDataLoader.LoadMockData();

            var settings = A.Fake<IDukeboxSettings>();
            var albumArtCache = A.Fake<IAlbumArtCacheService>();
            var audioFormats = new AudioFileFormats();

            A.CallTo(() => settings.AddDirectoryConcurrencyLimit).Returns(10);

            audioFormats.SupportedFormats.Add(".mp3");

            _musicLibrary = new MusicLibrary(_musicLibraryDbContext, settings, albumArtCache, audioFormats);

        }

        [Fact]
        public void Album()
        {
            var albums = _musicLibrary.OrderedAlbums;

            var mockOrderedAlbumNames = _mockDataLoader.Albums.OrderBy(a => a.name).Select(a => a.name);
            var allAlbumsReturned = albums.Select(a => a.name).SequenceEqual(mockOrderedAlbumNames);

            Assert.True(allAlbumsReturned, "Failed to return all albums from the database");

            var albumCountCorrect = _musicLibrary.GetAlbumCount() == _mockDataLoader.Albums.Count;

            Assert.True(albumCountCorrect, "Album count returned was incorrect");

            var albumIdsReturned = new List<long>();
            Exception fetchByIdException = null;

            try
            {
                _mockDataLoader.Albums.ForEach(a =>
                {
                    var album = _musicLibrary.GetAlbumById(a.id);
                    albumIdsReturned.Add(album.id);
                });
            }
            catch (Exception ex)
            {
                fetchByIdException = ex;
            }

            Assert.Null(fetchByIdException);

            albumIdsReturned.Sort();

            var mockAlbumsIds = _mockDataLoader.Albums.OrderBy(a => a.id).Select(a => a.id);
            var allAblumsRetured = albumIdsReturned.SequenceEqual(mockAlbumsIds);

            Assert.True(allAblumsRetured, "Failed to return all albums by ID");
        }

        [Fact]
        public void Artist()
        {
            var artists = _musicLibrary.OrderedArtists;

            var mockOrderedArtistNames = _mockDataLoader.Artists.OrderBy(a => a.name).Select(a => a.name);
            var allArtistsReturned = artists.Select(a => a.name).SequenceEqual(mockOrderedArtistNames);

            Assert.True(allArtistsReturned, "Failed to return all artists from the database");

            var artistCountCorrect = _musicLibrary.GetArtistCount() == _mockDataLoader.Artists.Count;

            Assert.True(artistCountCorrect, "Artist count returned was incorrect");

            var artistIdsReturned = new List<long>();
            Exception fetchByIdException = null;

            try
            {
                _mockDataLoader.Artists.ForEach(a =>
                {
                    var artist = _musicLibrary.GetArtistById(a.id);
                    artistIdsReturned.Add(artist.id);
                });
            }
            catch (Exception ex)
            {
                fetchByIdException = ex;
            }

            Assert.Null(fetchByIdException);

            artistIdsReturned.Sort();

            var mockArtistIds = _mockDataLoader.Artists.OrderBy(a => a.id).Select(a => a.id);
            var allArtistsRetured = artistIdsReturned.SequenceEqual(mockArtistIds);

            Assert.True(allArtistsRetured, "Failed to return all artists by ID");
        }

        [Fact]
        public void Playlist()
        {
            var playlists = _musicLibrary.OrderedPlaylists;

            var mockOrderedPlaylistNames = _mockDataLoader.Playlists.OrderBy(a => a.name).Select(a => a.name);
            var allPlaylistsReturned = playlists.Select(a => a.name).SequenceEqual(mockOrderedPlaylistNames);

            Assert.True(allPlaylistsReturned, "Failed to return all playlists from the database");

            var playlistCountCorrect = _musicLibrary.GetArtistCount() == _mockDataLoader.Artists.Count;

            Assert.True(playlistCountCorrect, "Playlists count returned was incorrect");

            var playlistIdsReturned = new List<long>();
            Exception fetchByIdException = null;

            try
            {
                _mockDataLoader.Playlists.ForEach(a =>
                {
                    var playlist = _musicLibrary.GetPlaylistById(a.id);
                    playlistIdsReturned.Add(playlist.id);
                });
            }
            catch (Exception ex)
            {
                fetchByIdException = ex;
            }

            Assert.Null(fetchByIdException);

            playlistIdsReturned.Sort();

            var mockPlaylistIds = _mockDataLoader.Playlists.OrderBy(a => a.id).Select(a => a.id);
            var allPlaylistsRetured = playlistIdsReturned.SequenceEqual(mockPlaylistIds);

            Assert.True(allPlaylistsRetured, "Failed to return all playlists by ID");
        }

        [Fact]
        public void SearchByText()
        {

        }

        [Fact]
        public void SearchByAttribute()
        {

        }

        [Fact]
        public void AddDirectory()
        {

        }

        [Fact]
        public void AddFile()
        {

        }

        [Fact]
        public void AddPlaylist()
        {

        }

        [Fact]
        public void RemoveTrack()
        {

        }
    }
}
