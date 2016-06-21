using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryDbContext
    {
        List<Album> Albums { get; }
        List<Artist> Artists { get; }
        DbSet<Playlist> Playlists { get; set; }
        DbSet<Song> Songs { get; set; }
        int SaveChanges();
        Task<int> SaveChangesAsync();
        void LogEntityValidationException(DbEntityValidationException ex);
    }
}