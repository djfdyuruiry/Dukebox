using Dukebox.Library.Model;
using System.Data.Entity;

namespace Dukebox.Library.Repositories
{
    internal class Library : DbContext
    {
        public Library()
            : base("name=Library")
        {
        }

        public virtual DbSet<Album> Albums { get; set; }
        public virtual DbSet<Artist> Artists { get; set; }
        public virtual DbSet<Playlist> Playlists { get; set; }
        public virtual DbSet<Song> Songs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
