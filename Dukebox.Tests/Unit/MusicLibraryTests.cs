using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FakeItEasy;
using Newtonsoft.Json;
using Xunit;
using Dukebox.Audio;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Repositories;
using Dukebox.Tests.Utils;
using Dukebox.Library.Model;

namespace Dukebox.Tests.Unit
{
    public class MusicLibraryTests
    {
        private readonly LibraryDbMockGenerator _mockDataLoader;
        private readonly IMusicLibraryDbContext _musicLibraryDbContext;
        private readonly MusicLibrary _musicLibrary;

        public MusicLibraryTests()
        {
            _mockDataLoader = new LibraryDbMockGenerator();
            _musicLibraryDbContext = _mockDataLoader.DbContextMock;

            var settings = A.Fake<IDukeboxSettings>();
            var albumArtCache = A.Fake<IAlbumArtCacheService>();
            var audioFormats = new AudioFileFormats();

            A.CallTo(() => settings.AddDirectoryConcurrencyLimit).Returns(5);

            audioFormats.SupportedFormats.Add(".mp3");

            _musicLibrary = new MusicLibrary(_musicLibraryDbContext, settings, albumArtCache, audioFormats);

        }
        
        [Fact]
        public void Album()
        {
            var albums = _musicLibrary.OrderedAlbums;

            var mockOrderedAlbumNames = _mockDataLoader.Albums.OrderBy(a => a.Name).Select(a => a.Name);
            var allAlbumsReturned = albums.Select(a => a.Name).SequenceEqual(mockOrderedAlbumNames);

            Assert.True(allAlbumsReturned, "Failed to return all albums from the database");

            var albumCountCorrect = _musicLibrary.GetAlbumCount() == _mockDataLoader.Albums.Count;

            Assert.True(albumCountCorrect, "Album count returned was incorrect");

            var albumIdsReturned = new List<long>();
            Exception fetchByIdException = null;

            try
            {
                _mockDataLoader.Albums.ForEach(a =>
                {
                    var album = _musicLibrary.GetAlbumById(a.Id);
                    albumIdsReturned.Add(album.Id);
                });
            }
            catch (Exception ex)
            {
                fetchByIdException = ex;
            }

            Assert.Null(fetchByIdException);

            albumIdsReturned.Sort();

            var mockAlbumsIds = _mockDataLoader.Albums.OrderBy(a => a.Id).Select(a => a.Id);
            var allAblumsRetured = albumIdsReturned.SequenceEqual(mockAlbumsIds);

            Assert.True(allAblumsRetured, "Failed to return all albums by ID");
        }

        [Fact]
        public void Artist()
        {
            var artists = _musicLibrary.OrderedArtists;

            var mockOrderedArtistNames = _mockDataLoader.Artists.OrderBy(a => a.Name).Select(a => a.Name);
            var allArtistsReturned = artists.Select(a => a.Name).SequenceEqual(mockOrderedArtistNames);

            Assert.True(allArtistsReturned, "Failed to return all artists from the database");

            var artistCountCorrect = _musicLibrary.GetArtistCount() == _mockDataLoader.Artists.Count;

            Assert.True(artistCountCorrect, "Artist count returned was incorrect");

            var artistIdsReturned = new List<long>();
            Exception fetchByIdException = null;

            try
            {
                _mockDataLoader.Artists.ForEach(a =>
                {
                    var artist = _musicLibrary.GetArtistById(a.Id);
                    artistIdsReturned.Add(artist.Id);
                });
            }
            catch (Exception ex)
            {
                fetchByIdException = ex;
            }

            Assert.Null(fetchByIdException);

            artistIdsReturned.Sort();

            var mockArtistIds = _mockDataLoader.Artists.OrderBy(a => a.Id).Select(a => a.Id);
            var allArtistsRetured = artistIdsReturned.SequenceEqual(mockArtistIds);

            Assert.True(allArtistsRetured, "Failed to return all artists by ID");
        }

        [Fact]
        public void Playlist()
        {
            var playlists = _musicLibrary.OrderedPlaylists;

            var mockOrderedPlaylistNames = _mockDataLoader.Playlists.OrderBy(a => a.Name).Select(a => a.Name);
            var allPlaylistsReturned = playlists.Select(a => a.Name).SequenceEqual(mockOrderedPlaylistNames);

            Assert.True(allPlaylistsReturned, "Failed to return all playlists from the database");

            var playlistCountCorrect = _musicLibrary.GetArtistCount() == _mockDataLoader.Artists.Count;

            Assert.True(playlistCountCorrect, "Playlists count returned was incorrect");

            var playlistIdsReturned = new List<long>();
            Exception fetchByIdException = null;

            try
            {
                _mockDataLoader.Playlists.ForEach(a =>
                {
                    var playlist = _musicLibrary.GetPlaylistById(a.Id);
                    playlistIdsReturned.Add(playlist.Id);
                });
            }
            catch (Exception ex)
            {
                fetchByIdException = ex;
            }

            Assert.Null(fetchByIdException);

            playlistIdsReturned.Sort();

            var mockPlaylistIds = _mockDataLoader.Playlists.OrderBy(a => a.Id).Select(a => a.Id);
            var allPlaylistsRetured = playlistIdsReturned.SequenceEqual(mockPlaylistIds);

            Assert.True(allPlaylistsRetured, "Failed to return all playlists by ID");
        }

        [Fact]
        public void SearchByText()
        {
            var resultsForKnownPhrase = _musicLibrary.SearchForTracksInArea(Library.Model.SearchAreas.All, "drom");
            var resultsForMissingPhrase = _musicLibrary.SearchForTracksInArea(Library.Model.SearchAreas.All, "fake.it.easy");

            Assert.True(resultsForKnownPhrase.Any(), "Failed to get tracks by known phrase");
            Assert.False(resultsForMissingPhrase.Any(), "Incorrectly got results for an unknown phrase");

            var resultsForKnownArtist = _musicLibrary.SearchForTracksInArea(Library.Model.SearchAreas.Artist, "spike");
            var resultsForMissingArtist = _musicLibrary.SearchForTracksInArea(Library.Model.SearchAreas.Artist, "gamblor");

            Assert.True(resultsForKnownArtist.Any(), "Failed to get tracks by known artist title");
            Assert.False(resultsForMissingArtist.Any(), "Incorrectly got results for an unknown artist title");

            var resultsForKnownAlbum = _musicLibrary.SearchForTracksInArea(Library.Model.SearchAreas.Album, "neptune");
            var resultsForMissingAlbum = _musicLibrary.SearchForTracksInArea(Library.Model.SearchAreas.Album, "homer");

            Assert.True(resultsForKnownAlbum.Any(), "Failed to get tracks by known album title");
            Assert.False(resultsForMissingAlbum.Any(), "Incorrectly got results for an unknown album title");

            var resultsForKnownSong = _musicLibrary.SearchForTracksInArea(Library.Model.SearchAreas.Song, "wish you were here");
            var resultsForMissingSong = _musicLibrary.SearchForTracksInArea(Library.Model.SearchAreas.Song, "shine on");

            Assert.True(resultsForKnownSong.Any(), "Failed to get tracks by known song title");
            Assert.False(resultsForMissingSong.Any(), "Incorrectly got results for an unknown song title");

            var resultsForKnownFilename = _musicLibrary.SearchForTracksInArea(Library.Model.SearchAreas.Filename, "sample.mp3");
            var resultsForMissingFilename = _musicLibrary.SearchForTracksInArea(Library.Model.SearchAreas.Filename, "fakeiteasy.man");

            Assert.True(resultsForKnownFilename.Any(), "Failed to get tracks by known filename");
            Assert.False(resultsForMissingFilename.Any(), "Incorrectly got results for an unknown filename");
        }

        [Fact]
        public void GetTrackByAttributeValue()
        {
            var resultsForKnownAlbum = _musicLibrary.GetTracksByAttributeValue(SearchAreas.Album, "neptune");
            var resultsForMissingAlbum = _musicLibrary.GetTracksByAttributeValue(SearchAreas.Album, "homer");

            Assert.True(resultsForKnownAlbum.Any(), "Failed to get tracks by known album title");
            Assert.False(resultsForMissingAlbum.Any(), "Incorrectly got results for an unknown album title");

            var resultsForKnownArtist = _musicLibrary.GetTracksByAttributeValue(SearchAreas.Artist, "spike");
            var resultsForMissingArtist = _musicLibrary.GetTracksByAttributeValue(SearchAreas.Artist, "gamblor");

            Assert.True(resultsForKnownArtist.Any(), "Failed to get tracks by known artist title");
            Assert.False(resultsForMissingArtist.Any(), "Incorrectly got results for an unknown artist title");

            var resultsForKnownFilename = _musicLibrary.GetTracksByAttributeValue(SearchAreas.Filename, LibraryDbMockGenerator.Mp3FilePath);
            var resultsForMissingFilename = _musicLibrary.GetTracksByAttributeValue(SearchAreas.Filename, "fakeiteasy.man");

            Assert.True(resultsForKnownFilename.Any(), "Failed to get tracks by known filename");
            Assert.False(resultsForMissingFilename.Any(), "Incorrectly got results for an unknown filename");

            var resultsForKnownSong = _musicLibrary.GetTracksByAttributeValue(SearchAreas.Song, "wish you were here");
            var resultsForMissingSong = _musicLibrary.GetTracksByAttributeValue(SearchAreas.Song, "shine on");

            Assert.True(resultsForKnownSong.Any(), "Failed to get tracks by known song title");
            Assert.False(resultsForMissingSong.Any(), "Incorrectly got results for an unknown song title");
        }

        [Fact]
        public void GetTrackByAttributeId()
        {
            var resultsForKnownAlbum = _musicLibrary.GetTracksByAttributeId(SearchAreas.Album, 0);
            var resultsForMissingAlbum = _musicLibrary.GetTracksByAttributeId(SearchAreas.Album, 5);

            Assert.True(resultsForKnownAlbum.Any(), "Failed to get tracks by known album id");
            Assert.False(resultsForMissingAlbum.Any(), "Incorrectly got results for an unknown album id");

            var resultsForKnownArtist = _musicLibrary.GetTracksByAttributeId(SearchAreas.Artist, 0);
            var resultsForMissingArtist = _musicLibrary.GetTracksByAttributeId(SearchAreas.Artist, 5);

            Assert.True(resultsForKnownArtist.Any(), "Failed to get tracks by known artist id");
            Assert.False(resultsForMissingArtist.Any(), "Incorrectly got results for an unknown artist id");

            var resultsForKnownSong = _musicLibrary.GetTracksByAttributeId(SearchAreas.Song, 0);
            var resultsForMissingSong = _musicLibrary.GetTracksByAttributeId(SearchAreas.Song, 25);

            Assert.True(resultsForKnownSong.Any(), "Failed to get tracks by known song id");
            Assert.False(resultsForMissingSong.Any(), "Incorrectly got results for an unknown song id");
        }

        [Fact]
        public void GetTracksForDirectory()
        {
            var numSamples = 5;

            PrepareSamplesDirectory("samples", numSamples);
            PrepareSamplesDirectory("samples/samples", numSamples);

            var tracks = _musicLibrary.GetTracksForDirectory("samples", true);

            var tracksReturned = tracks.Any();

            Assert.True(tracksReturned, "Music library failed to get any tracks for directory");

            var trackAreCorrect = tracks.All(t => t.Song.Title == "sample title") && tracks.Count == numSamples * 2;

            Assert.True(trackAreCorrect, "Music library failed to get correct tracks for directory");
        }

        [Fact]
        public void GetTracksForPlaylist()
        {
            var numSamples = 5;

            var files = PrepareSamplesDirectory("samples", numSamples);
            var playlist = new Playlist
            {
                FilenamesCsv = string.Join(",", files)
            };

            var tracks = _musicLibrary.GetTracksForPlaylist(playlist);
            var tracksReturned = tracks.Any();

            Assert.True(tracksReturned, "Music library failed to return any tracks after adding playlist");

            var trackAreCorrect = tracks.All(t => t.Song.Title == "sample title") && tracks.Count == numSamples;

            Assert.True(trackAreCorrect, "Music library failed to return correct tracks after adding playlist");
        }

        [Fact]
        public void AddDirectory()
        {
            var numSamples = 5;

            PrepareSamplesDirectory("samples", numSamples);
            _musicLibrary.AddSupportedFilesInDirectory("samples", false, null, null).Wait();

            var tracks = _musicLibrary.SearchForTracks("samples", new List<SearchAreas> { SearchAreas.Filename });
            var tracksReturned = tracks.Any();

            Assert.True(tracksReturned, "Music library failed to return any tracks after adding directory");

            var trackAreCorrect = tracks.All(t => t.Song.Title == "sample title") && tracks.Count == numSamples;

            Assert.True(trackAreCorrect, "Music library failed to return correct tracks after adding directory");
        }

        [Fact]
        public async void AddFile()
        {
            var sampleFileName = "new_sample.mp3";
            var songTitle = "Unique Song Title $%3£$";

            File.Copy("sample.mp3", sampleFileName, true);

            var trackFile = new FileInfo(sampleFileName);
            var audioMetadata = A.Fake<IAudioFileMetadata>();
            
            A.CallTo(() => audioMetadata.Title).Returns(songTitle);
            A.CallTo(() => audioMetadata.HasFutherMetadataTag).Returns(false);
            A.CallTo(() => audioMetadata.HasAlbumArt).Returns(false);

            await _musicLibrary.AddFile(trackFile.FullName, audioMetadata);

            var tracks = _musicLibrary.SearchForTracks(songTitle, new List<SearchAreas> { SearchAreas.Song });
            var tracksReturned = tracks.Any();

            Assert.True(tracksReturned, "Music library failed to return any tracks after adding new track");

            var track = tracks.First();
            var trackIsCorrect = track.Song.FileName == trackFile.FullName;

            Assert.True(trackIsCorrect, "Music library failed to return correct track after adding new track");
        }

        [Fact]
        public async void AddPlaylistFile()
        {
            var jplFileName = "sample_playlist.jpl";
            var numSamples = 5;

            var files = PrepareSamplesDirectory("samples", numSamples);

            var jplJson = JsonConvert.SerializeObject(files);

            File.Delete(jplFileName);
            File.WriteAllText(jplFileName, jplJson);

            await _musicLibrary.AddPlaylistFiles(jplFileName);

            var tracks = _musicLibrary.SearchForTracksInArea(SearchAreas.Filename, "samples");
            var tracksReturned = tracks.Any();

            Assert.True(tracksReturned, "Music library failed to return any tracks after adding playlist");

            var trackAreCorrect = tracks.All(t => t.Song.Title == "sample title") && tracks.Count == numSamples;

            Assert.True(trackAreCorrect, "Music library failed to return correct tracks after adding playlist");
        }

        [Fact]
        public async void AddPlaylist()
        {
            var playlistName = "magical music";
            var files = new List<string> { "samples/music.mp3", "samples/music1.mp3", "samples/music2.mp3", "samples/music3.mp3" };
            var maxPlaylistId = _musicLibrary.OrderedPlaylists.Max(p => p.Id);

            await _musicLibrary.AddPlaylist(playlistName, files);

            var playlist = _musicLibrary.OrderedPlaylists.FirstOrDefault(p => p.Name == playlistName);
            var playlistReturned = playlist != null;

            Assert.True(playlistReturned, "Music library failed to add playlist");

            var playlistFileNames = playlist.Files;
            var foundFilesInPlaylist = files.Where(f => playlistFileNames.Any(pf => pf.Contains(f)));
            var allFilesInPlaylist = foundFilesInPlaylist.Count() == files.Count;

            Assert.True(allFilesInPlaylist, "Music library failed to store all files for playlist");
        }
        
        [Fact]
        public async void RemoveTrack()
        {
            var tracks = _musicLibrary.SearchForTracks("wish you were here", new List<SearchAreas> { SearchAreas.Song });
            var track = tracks.First();

            await _musicLibrary.RemoveTrack(track);

            tracks = _musicLibrary.GetTracksByAttributeId(SearchAreas.Song, track.Song.Id);
            var trackDeleted = !tracks.Any();

            Assert.True(trackDeleted, "Music library failed to delete track");
        }

        private List<string> PrepareSamplesDirectory(string directoryName = "samples", int numSamples = 5)
        {
            var files = new List<string>();
            Directory.CreateDirectory(directoryName);

            for (var i = 0; i < numSamples; i++)
            {
                var newFilePath = Path.Combine(directoryName, string.Format("sample{0}.mp3", i));

                File.Copy("sample.mp3", newFilePath, true);
                files.Add(newFilePath);
            }

            return files;
        }
    }
}
