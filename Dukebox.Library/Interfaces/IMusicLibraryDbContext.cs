using System.Data.Entity;
using Dukebox.Library.Model;
using System.Threading.Tasks;
using System.Data.Entity.Validation;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryDbContext
    {
        DbSet<Album> Albums { get; set; }
        DbSet<Artist> Artists { get; set; }
        DbSet<Playlist> Playlists { get; set; }
        DbSet<Song> Songs { get; set; }
        int SaveChanges();
        Task<int> SaveChangesAsync();
        void LogEntityValidationException(DbEntityValidationException ex);
    }
}