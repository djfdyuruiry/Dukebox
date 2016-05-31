using Dukebox.Library;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using FakeItEasy;
using SimpleInjector;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;

namespace Dukebox.Tests.Utils
{
    public class LibraryDbMockGenerator
    {
        private static bool containerModified;
        public static readonly string Mp3FilePath = Path.Combine(Directory.GetCurrentDirectory(), "sample.mp3");

        public readonly List<Album> Albums = new List<Album>
        {
            new Album { hasAlbumArt = 0, Id = 0, Name = "jupiter" },
            new Album { hasAlbumArt = 1, Id = 1, Name = "neptune" },
            new Album { hasAlbumArt = 0, Id = 2, Name = "andromeda" },
            new Album { hasAlbumArt = 1, Id = 3, Name = "uranus" },
            new Album { hasAlbumArt = 0, Id = 4, Name = "mars" }
        };

        public readonly List<Artist> Artists = new List<Artist>
        {
            new Artist { Id = 0, Name = "spike" },
            new Artist { Id = 1, Name = "buzz" },
            new Artist { Id = 2, Name = "specter" },
            new Artist { Id = 3, Name = "professor" },
            new Artist { Id = 4, Name = "katy" }
        };

        public readonly List<Playlist> Playlists = new List<Playlist>
        {
            new Playlist { Id = 0, Name = "awesome", FilenamesCsv = string.Format("{0},{0},{0}", Mp3FilePath) },
            new Playlist { Id = 1, Name = "fantastic", FilenamesCsv = string.Format("{0},{0},{0}", Mp3FilePath) },
            new Playlist { Id = 2, Name = "amazing", FilenamesCsv = string.Format("{0},{0},{0}", Mp3FilePath) }
        };

        public readonly List<Song> Songs;
        
        public IMusicLibraryDbContext DbContextMock { get; private set; }

        public LibraryDbMockGenerator(bool overrideLibraryPackage = true)
        {
            DbContextMock = A.Fake<IMusicLibraryDbContext>();

            if (overrideLibraryPackage && !containerModified)
            {
                LibraryPackage.ExecutingForUnitTests = true;
                var libraryContainer = LibraryPackage.GetContainerForTestOverrides();

                libraryContainer.Options.AllowOverridingRegistrations = true;
                libraryContainer.RegisterSingleton<IMusicLibraryDbContext>(DbContextMock);

                containerModified = true;
            }

            Songs = new List<Song>();

            var songId = 0;

            Artists.ForEach(artist =>
            {
                Albums.ForEach(album =>
                {
                    Songs.Add(new Song
                    {
                        Id = songId,
                        FileName = Mp3FilePath,
                        Title = "wish you were here",
                        LengthInSeconds = 120,
                        AlbumId = album.Id,
                        ArtistId = artist.Id
                    });

                    songId++;
                });
            });

            WireUpMockData();
        }

        private void WireUpMockData()
        {
            var albums = A.Fake<DbSet<Album>>(o => o.Implements(typeof(IQueryable<Album>))
                .Implements(typeof(IDbAsyncEnumerable<Album>)))
                .SetupData(Albums);

            A.CallTo(() => DbContextMock.Albums).Returns(albums);

            var artists = A.Fake<DbSet<Artist>>(o => o.Implements(typeof(IQueryable<Artist>))
                .Implements(typeof(IDbAsyncEnumerable<Artist>)))
                .SetupData(Artists);

            A.CallTo(() => DbContextMock.Artists).Returns(artists);

            var playlists = A.Fake<DbSet<Playlist>>(o => o.Implements(typeof(IQueryable<Playlist>))
                            .Implements(typeof(IDbAsyncEnumerable<Playlist>)))
                            .SetupData(Playlists);
            
            A.CallTo(() => DbContextMock.Playlists).Returns(playlists);

            var songs = A.Fake<DbSet<Song>>(o => o.Implements(typeof(IQueryable<Song>))
                    .Implements(typeof(IDbAsyncEnumerable<Song>)))
                    .SetupData(Songs);

            A.CallTo(() => DbContextMock.Songs).Returns(songs);
        }
    }
}
