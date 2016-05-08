using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Repositories
{
    public class MusicLibraryDbContext : DbContext, IMusicLibraryDbContext
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        public void LogEntityValidationException(DbEntityValidationException ex)
        {
            foreach (var vr in ex.EntityValidationErrors)
            {
                foreach (var ve in vr.ValidationErrors)
                {
                    logger.ErrorFormat("Database validation error on property '{1}': {0}",
                        ve.ErrorMessage, ve.PropertyName);
                }
            }

            logger.Error(ex);
        }
    }
}
