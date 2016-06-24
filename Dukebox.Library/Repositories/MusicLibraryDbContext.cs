using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using System.Data.Entity.Infrastructure;

namespace Dukebox.Library.Repositories
{
    public class MusicLibraryDbContext : DbContext, IMusicLibraryDbContext
    {
        private readonly object _addSongLock;

        public List<Album> Albums
        {
            get
            {
                return Songs.Select(s => s.AlbumName).Distinct().ToList().Select(a => new Album(a)).ToList();
            }
        }
        
        public List<Artist> Artists
        {
            get
            {
                return Songs.Select(s => s.ArtistName).Distinct().ToList().Select(a => new Artist(a)).ToList();
            }
        }

        public MusicLibraryDbContext() : this("name=Library")
        {
        }

        public MusicLibraryDbContext(string connectionString) : base(connectionString)
        {
            _addSongLock = new object();

            Database.Log = (s) => Debug.WriteLine(s);
        }

        public virtual DbSet<Playlist> Playlists { get; set; }
        public virtual DbSet<Song> Songs { get; set; }

        public new DbEntityEntry Entry(object entity)
        {
            return base.Entry(entity);
        }

        public void SynchronisedAddSong(Song song)
        {
            lock(_addSongLock)
            {
                Songs.Add(song);
            }
        }
    }
}
