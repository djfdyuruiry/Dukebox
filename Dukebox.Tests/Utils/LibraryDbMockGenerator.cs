using Dukebox.Library;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using FakeItEasy;
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
            new Album { hasAlbumArt = 0, id = 0, name = "jupiter" },
            new Album { hasAlbumArt = 1, id = 1, name = "neptune" },
            new Album { hasAlbumArt = 0, id = 2, name = "andromeda" },
            new Album { hasAlbumArt = 1, id = 3, name = "uranus" },
            new Album { hasAlbumArt = 0, id = 4, name = "mars" }
        };

        public readonly List<Artist> Artists = new List<Artist>
        {
            new Artist { id = 0, name = "spike" },
            new Artist { id = 1, name = "buzz" },
            new Artist { id = 2, name = "specter" },
            new Artist { id = 3, name = "professor" },
            new Artist { id = 4, name = "katy" }
        };

        public readonly List<Playlist> Playlists = new List<Playlist>
        {
            new Playlist { id = 0, name = "awesome", filenamesCsv = string.Format("{0},{0},{0}", Mp3FilePath) },
            new Playlist { id = 1, name = "fantastic", filenamesCsv = string.Format("{0},{0},{0}", Mp3FilePath) },
            new Playlist { id = 2, name = "amazing", filenamesCsv = string.Format("{0},{0},{0}", Mp3FilePath) }
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
                        id = songId,
                        filename = Mp3FilePath,
                        title = "wish you were here",
                        lengthInSeconds = 120,
                        albumId = album.id,
                        artistId = artist.id
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
