using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Tests.Utils
{
    public class LibraryDbMockGenerator
    {
        public static readonly string Mp3FilePath = Path.Combine(Directory.GetCurrentDirectory(), "sample.mp3");

        public readonly List<Album> Albums = new List<Album>
        {
            new Album("jupiter"),
            new Album("neptune"),
            new Album("andromeda"),
            new Album("uranus"),
            new Album("mars")
        };

        public readonly List<Artist> Artists = new List<Artist>
        {
            new Artist("spike"),
            new Artist("buzz"),
            new Artist("specter"),
            new Artist("professor"),
            new Artist("katy")
        };

        public readonly List<Playlist> Playlists = new List<Playlist>
        {
            new Playlist { Id = 0, Name = "awesome", FilenamesCsv = string.Format("{0},{0},{0}", Mp3FilePath) },
            new Playlist { Id = 1, Name = "fantastic", FilenamesCsv = string.Format("{0},{0},{0}", Mp3FilePath) },
            new Playlist { Id = 2, Name = "amazing", FilenamesCsv = string.Format("{0},{0},{0}", Mp3FilePath) }
        };

        public readonly List<Song> Songs;
        
        public IMusicLibraryDbContext DbContextMock { get; private set; }
        
        public LibraryDbMockGenerator()
        {
            DbContextMock = A.Fake<IMusicLibraryDbContext>();

            A.CallTo(() => DbContextMock.SaveChangesAsync()).Returns(Task.FromResult(0));
            
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
                        AlbumName = album.Name,
                        ArtistName = artist.Name
                    });

                    songId++;
                });
            });

            WireUpMockData();
        }

        private void WireUpMockData()
        {
            A.CallTo(() => DbContextMock.Albums).Returns(Albums);
            A.CallTo(() => DbContextMock.Artists).Returns(Artists);

            var playlists = A.Fake<DbSet<Playlist>>(o => o.Implements(typeof(IQueryable<Playlist>))
                            .Implements(typeof(IDbAsyncEnumerable<Playlist>)))
                            .SetupData(Playlists);
            
            A.CallTo(() => DbContextMock.Playlists).Returns(playlists);

            var songs = A.Fake<DbSet<Song>>(o => o.Implements(typeof(IQueryable<Song>))
                    .Implements(typeof(IDbAsyncEnumerable<Song>)))
                    .SetupData(Songs);

            A.CallTo(() => DbContextMock.Songs).Returns(songs);

            A.CallTo(() => DbContextMock.SynchronisedAddSong(A<Song>.Ignored))
                .Invokes(e => songs.Add(e.Arguments[0] as Song));
        }
    }
}
