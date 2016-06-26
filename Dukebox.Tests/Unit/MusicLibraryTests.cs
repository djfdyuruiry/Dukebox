using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Newtonsoft.Json;
using Xunit;
using Dukebox.Audio;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Repositories;
using Dukebox.Tests.Utils;
using Dukebox.Library.Model;
using Dukebox.Configuration.Interfaces;
using Dukebox.Library.Factories;
using Dukebox.Audio.Interfaces;
using Dukebox.Library.Services.MusicLibrary;
using Dukebox.Library.Services;

namespace Dukebox.Tests.Unit
{
    public class MusicLibraryTests
    {
        private readonly LibraryDbMockGenerator _mockDataLoader;
        private readonly MusicLibraryCacheService _musicLibraryCacheService;
        private readonly MusicLibraryImportService _musicLibraryImportService;
        private readonly MusicLibraryRepository _musicLibraryRepo;
        private readonly MusicLibrarySearchService _musicLibrarySearchService;
        private readonly TrackGeneratorService _trackGenerator;
        private readonly MusicLibraryUpdateService _musicLibaryUpdateService;

        public MusicLibraryTests()
        {
            _mockDataLoader = new LibraryDbMockGenerator();

            var dbContextFactory = A.Fake<IMusicLibraryDbContextFactory>();   
            var settings = A.Fake<IDukeboxSettings>();
            var albumArtCache = A.Fake<IAlbumArtCacheService>();
            var audioFormats = new AudioFileFormats();
            var audioFileMetadataFactory = new AudioFileMetadataFactory(A.Fake<ICdMetadataService>(), A.Fake<IAudioCdService>());
            var trackFactory = new TrackFactory(settings, audioFileMetadataFactory);

            A.CallTo(() => dbContextFactory.GetInstance()).Returns(_mockDataLoader.DbContextMock);
            A.CallTo(() => settings.AddDirectoryConcurrencyLimit).Returns(5);

            audioFormats.SupportedFormats.Add(".mp3");

            var eventService = new MusicLibraryEventService();
            var musicPlaylistGenerator = new PlaylistGeneratorService();
            
            _musicLibraryCacheService = new MusicLibraryCacheService(dbContextFactory, eventService);
            _musicLibraryRepo = new MusicLibraryRepository(dbContextFactory, trackFactory, _musicLibraryCacheService);
            _musicLibrarySearchService = new MusicLibrarySearchService(dbContextFactory, trackFactory, _musicLibraryCacheService);
            _musicLibraryImportService = new MusicLibraryImportService(settings, audioFormats, dbContextFactory, audioFileMetadataFactory,
                trackFactory, _musicLibraryCacheService, eventService, albumArtCache, musicPlaylistGenerator);
            _musicLibaryUpdateService = new MusicLibraryUpdateService(dbContextFactory);
            _trackGenerator = new TrackGeneratorService(audioFormats, _musicLibrarySearchService, trackFactory);
        }
        
        [Fact]
        public void Album()
        {
            var albums = _musicLibraryCacheService.OrderedAlbums;

            var mockOrderedAlbumNames = _mockDataLoader.Albums.OrderBy(a => a.Name).Select(a => a.Name);
            var allAlbumsReturned = albums.Select(a => a.Name).SequenceEqual(mockOrderedAlbumNames);

            Assert.True(allAlbumsReturned, "Failed to return all albums from the database");

            var albumCountCorrect = _musicLibraryRepo.GetAlbumCount() == _mockDataLoader.Albums.Count;

            Assert.True(albumCountCorrect, "Album count returned was incorrect");

            var albumsReturned = new List<string>();
            Exception fetchByIdException = null;

            try
            {
                _mockDataLoader.Albums.ForEach(a =>
                {
                    var album = _musicLibraryCacheService.OrderedAlbums.FirstOrDefault(la => la.Name.Equals(a.Name));

                    if (album != null)
                    {
                        albumsReturned.Add(album.Name);
                    }
                });
            }
            catch (Exception ex)
            {
                fetchByIdException = ex;
            }

            Assert.Null(fetchByIdException);

            albumsReturned.Sort();

            var mockAlbums = _mockDataLoader.Albums.Select(a => a.Name).OrderBy(a => a);
            var allAblumsRetured = albumsReturned.SequenceEqual(mockAlbums.OrderBy(a => a));

            Assert.True(allAblumsRetured, "Failed to return all albums by name");
        }

        [Fact]
        public void Artist()
        {
            var artists = _musicLibraryCacheService.OrderedArtists;

            var mockOrderedArtistNames = _mockDataLoader.Artists.Select(a => a.Name).OrderBy(a => a);
            var allArtistsReturned = artists.Select(a => a.Name).SequenceEqual(mockOrderedArtistNames.OrderBy(a => a));

            Assert.True(allArtistsReturned, "Failed to return all artists from the database");

            var artistCountCorrect = _musicLibraryRepo.GetArtistCount() == _mockDataLoader.Artists.Count;

            Assert.True(artistCountCorrect, "Artist count returned was incorrect");

            var artistsReturned = new List<string>();
            Exception fetchByIdException = null;

            try
            {
                _mockDataLoader.Artists.ForEach(a =>
                {
                    var artist = _musicLibraryCacheService.OrderedArtists.FirstOrDefault(la => la.Name.Equals(a.Name));

                    if (artist != null)
                    {
                        artistsReturned.Add(artist.Name);
                    }
                });
            }
            catch (Exception ex)
            {
                fetchByIdException = ex;
            }

            Assert.Null(fetchByIdException);

            artistsReturned.Sort();

            var mockArtists = _mockDataLoader.Artists.Select(a => a.Name).OrderBy(a => a);
            var allArtistsRetured = artistsReturned.SequenceEqual(mockArtists.OrderBy(a => a));

            Assert.True(allArtistsRetured, "Failed to return all artists by name");
        }

        [Fact]
        public void Playlist()
        {
            var playlists = _musicLibraryCacheService.OrderedPlaylists;

            var mockOrderedPlaylistNames = _mockDataLoader.Playlists.OrderBy(a => a.Name).Select(a => a.Name);
            var allPlaylistsReturned = playlists.Select(a => a.Name).SequenceEqual(mockOrderedPlaylistNames);

            Assert.True(allPlaylistsReturned, "Failed to return all playlists from the database");

            var playlistCountCorrect = _musicLibraryRepo.GetArtistCount() == _mockDataLoader.Artists.Count;

            Assert.True(playlistCountCorrect, "Playlists count returned was incorrect");

            var playlistIdsReturned = new List<long>();
            Exception fetchByIdException = null;

            try
            {
                _mockDataLoader.Playlists.ForEach(a =>
                {
                    var playlist = _musicLibraryRepo.GetPlaylistById(a.Id);
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
            var resultsForKnownPhrase = _musicLibrarySearchService.SearchForTracksInArea(Library.Model.SearchAreas.All, "drom");
            var resultsForMissingPhrase = _musicLibrarySearchService.SearchForTracksInArea(Library.Model.SearchAreas.All, "fake.it.easy");

            Assert.True(resultsForKnownPhrase.Any(), "Failed to get tracks by known phrase");
            Assert.False(resultsForMissingPhrase.Any(), "Incorrectly got results for an unknown phrase");

            var resultsForKnownArtist = _musicLibrarySearchService.SearchForTracksInArea(Library.Model.SearchAreas.Artist, "spike");
            var resultsForMissingArtist = _musicLibrarySearchService.SearchForTracksInArea(Library.Model.SearchAreas.Artist, "gamblor");

            Assert.True(resultsForKnownArtist.Any(), "Failed to get tracks by known artist title");
            Assert.False(resultsForMissingArtist.Any(), "Incorrectly got results for an unknown artist title");

            var resultsForKnownAlbum = _musicLibrarySearchService.SearchForTracksInArea(Library.Model.SearchAreas.Album, "neptune");
            var resultsForMissingAlbum = _musicLibrarySearchService.SearchForTracksInArea(Library.Model.SearchAreas.Album, "homer");

            Assert.True(resultsForKnownAlbum.Any(), "Failed to get tracks by known album title");
            Assert.False(resultsForMissingAlbum.Any(), "Incorrectly got results for an unknown album title");

            var resultsForKnownSong = _musicLibrarySearchService.SearchForTracksInArea(Library.Model.SearchAreas.Song, "wish you were here");
            var resultsForMissingSong = _musicLibrarySearchService.SearchForTracksInArea(Library.Model.SearchAreas.Song, "shine on");

            Assert.True(resultsForKnownSong.Any(), "Failed to get tracks by known song title");
            Assert.False(resultsForMissingSong.Any(), "Incorrectly got results for an unknown song title");

            var resultsForKnownFilename = _musicLibrarySearchService.SearchForTracksInArea(Library.Model.SearchAreas.Filename, "sample.mp3");
            var resultsForMissingFilename = _musicLibrarySearchService.SearchForTracksInArea(Library.Model.SearchAreas.Filename, "fakeiteasy.man");

            Assert.True(resultsForKnownFilename.Any(), "Failed to get tracks by known filename");
            Assert.False(resultsForMissingFilename.Any(), "Incorrectly got results for an unknown filename");
        }

        [Fact]
        public void GetTrackByAttributeValue()
        {
            var resultsForKnownAlbum = _musicLibrarySearchService.GetTracksByAttributeValue(SearchAreas.Album, "neptune");
            var resultsForMissingAlbum = _musicLibrarySearchService.GetTracksByAttributeValue(SearchAreas.Album, "homer");

            Assert.True(resultsForKnownAlbum.Any(), "Failed to get tracks by known album title");
            Assert.False(resultsForMissingAlbum.Any(), "Incorrectly got results for an unknown album title");

            var resultsForKnownArtist = _musicLibrarySearchService.GetTracksByAttributeValue(SearchAreas.Artist, "spike");
            var resultsForMissingArtist = _musicLibrarySearchService.GetTracksByAttributeValue(SearchAreas.Artist, "gamblor");

            Assert.True(resultsForKnownArtist.Any(), "Failed to get tracks by known artist title");
            Assert.False(resultsForMissingArtist.Any(), "Incorrectly got results for an unknown artist title");

            var resultsForKnownFilename = _musicLibrarySearchService.GetTracksByAttributeValue(SearchAreas.Filename, LibraryDbMockGenerator.Mp3FilePath);
            var resultsForMissingFilename = _musicLibrarySearchService.GetTracksByAttributeValue(SearchAreas.Filename, "fakeiteasy.man");

            Assert.True(resultsForKnownFilename.Any(), "Failed to get tracks by known filename");
            Assert.False(resultsForMissingFilename.Any(), "Incorrectly got results for an unknown filename");

            var resultsForKnownSong = _musicLibrarySearchService.GetTracksByAttributeValue(SearchAreas.Song, "wish you were here");
            var resultsForMissingSong = _musicLibrarySearchService.GetTracksByAttributeValue(SearchAreas.Song, "shine on");

            Assert.True(resultsForKnownSong.Any(), "Failed to get tracks by known song title");
            Assert.False(resultsForMissingSong.Any(), "Incorrectly got results for an unknown song title");
        }

        [Fact]
        public void GetTrackByAttributeId()
        {
            var resultsForKnownAlbum = _musicLibrarySearchService.GetTracksByAttributeId(SearchAreas.Album, "jupiter");
            var resultsForMissingAlbum = _musicLibrarySearchService.GetTracksByAttributeId(SearchAreas.Album, "crazypants");

            Assert.True(resultsForKnownAlbum.Any(), "Failed to get tracks by known album id");
            Assert.False(resultsForMissingAlbum.Any(), "Incorrectly got results for an unknown album id");

            var resultsForKnownArtist = _musicLibrarySearchService.GetTracksByAttributeId(SearchAreas.Artist, "spike");
            var resultsForMissingArtist = _musicLibrarySearchService.GetTracksByAttributeId(SearchAreas.Artist, "crazierpants");

            Assert.True(resultsForKnownArtist.Any(), "Failed to get tracks by known artist id");
            Assert.False(resultsForMissingArtist.Any(), "Incorrectly got results for an unknown artist id");

            var resultsForKnownSong = _musicLibrarySearchService.GetTracksByAttributeId(SearchAreas.Song, "0");
            var resultsForMissingSong = _musicLibrarySearchService.GetTracksByAttributeId(SearchAreas.Song, "25");

            Assert.True(resultsForKnownSong.Any(), "Failed to get tracks by known song id");
            Assert.False(resultsForMissingSong.Any(), "Incorrectly got results for an unknown song id");
        }

        [Fact]
        public void GetTracksForDirectory()
        {
            var numSamples = 5;

            PrepareSamplesDirectory("samples", numSamples);
            PrepareSamplesDirectory("samples/samples", numSamples);

            var tracks = _trackGenerator.GetTracksForDirectory("samples", true);

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

            var tracks = _trackGenerator.GetTracksForPlaylist(playlist);
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
            _musicLibraryImportService.AddSupportedFilesInDirectory("samples", false, null, null).Wait();

            var tracks = _musicLibrarySearchService.SearchForTracks("samples", new List<SearchAreas> { SearchAreas.Filename });
            var tracksReturned = tracks.Any();

            Assert.True(tracksReturned, "Music library failed to return any tracks after adding directory");

            var trackAreCorrect = tracks.All(t => t.Song.Title == "sample title") && tracks.Count == numSamples;

            Assert.True(trackAreCorrect, "Music library failed to return correct tracks after adding directory");
        }

        [Fact]
        public async Task AddFile()
        {
            var sampleFileName = "new_sample.mp3";
            var songTitle = "Unique Song Title $%3£$";

            File.Copy("sample.mp3", sampleFileName, true);

            var trackFile = new FileInfo(sampleFileName);
            var audioMetadata = A.Fake<IAudioFileMetadata>();
            
            A.CallTo(() => audioMetadata.Title).Returns(songTitle);
            A.CallTo(() => audioMetadata.HasFutherMetadataTag).Returns(false);
            A.CallTo(() => audioMetadata.HasAlbumArt).Returns(false);

            _musicLibraryImportService.AddFile(trackFile.FullName, audioMetadata);

            var tracks = _musicLibrarySearchService.SearchForTracks(songTitle, new List<SearchAreas> { SearchAreas.Song });
            var tracksReturned = tracks.Any();

            Assert.True(tracksReturned, "Music library failed to return any tracks after adding new track");

            var track = tracks.First();
            var trackIsCorrect = track.Song.FileName == trackFile.FullName;

            Assert.True(trackIsCorrect, "Music library failed to return correct track after adding new track");
        }

        [Fact]
        public async Task AddPlaylistFile()
        {
            var jplFileName = "sample_playlist.jpl";
            var numSamples = 5;

            var files = PrepareSamplesDirectory("samples", numSamples);

            var jplJson = JsonConvert.SerializeObject(files);

            File.Delete(jplFileName);
            File.WriteAllText(jplFileName, jplJson);

            await _musicLibraryImportService.AddPlaylistFiles(jplFileName);

            var tracks = _musicLibrarySearchService.SearchForTracksInArea(SearchAreas.Filename, "samples");
            var tracksReturned = tracks.Any();

            Assert.True(tracksReturned, "Music library failed to return any tracks after adding playlist");

            var trackAreCorrect = tracks.All(t => t.Song.Title == "sample title") && tracks.Count == numSamples;

            Assert.True(trackAreCorrect, "Music library failed to return correct tracks after adding playlist");
        }

        [Fact]
        public async Task AddPlaylist()
        {
            var playlistName = "magical music";
            var files = new List<string> { "samples/music.mp3", "samples/music1.mp3", "samples/music2.mp3", "samples/music3.mp3" };

            await _musicLibraryImportService.AddPlaylist(playlistName, files);

            var playlist = _musicLibraryCacheService.OrderedPlaylists.FirstOrDefault(p => p.Name == playlistName);
            var playlistReturned = playlist != null;

            Assert.True(playlistReturned, "Music library failed to add playlist");

            var playlistFileNames = playlist.Files;
            var foundFilesInPlaylist = files.Where(f => playlistFileNames.Any(pf => pf.Contains(f)));
            var allFilesInPlaylist = foundFilesInPlaylist.Count() == files.Count;

            Assert.True(allFilesInPlaylist, "Music library failed to store all files for playlist");
        }
        
        [Fact]
        public async Task RemoveTrack()
        {
            var tracks = _musicLibrarySearchService.SearchForTracks("wish you were here", new List<SearchAreas> { SearchAreas.Song });
            var track = tracks.First();

            await _musicLibaryUpdateService.RemoveTrack(track);

            tracks = _musicLibrarySearchService.GetTracksByAttributeId(SearchAreas.Song, track.Song.Id.ToString());
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
