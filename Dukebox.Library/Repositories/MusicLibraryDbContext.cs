using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using System.Data.Entity;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dukebox.Library.Repositories
{
    public class MusicLibraryDbContext : DbContext, IMusicLibraryDbContext
    {
        public MusicLibraryDbContext() : this("name=Library")
        {
        }

        public MusicLibraryDbContext(string connectionString) : base(connectionString)
        {
            Database.Log = (s) => Debug.WriteLine(s);
        }

        public virtual DbSet<Album> Albums { get; set; }
        public virtual DbSet<Artist> Artists { get; set; }
        public virtual DbSet<Playlist> Playlists { get; set; }
        public virtual DbSet<Song> Songs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }
    }
}
