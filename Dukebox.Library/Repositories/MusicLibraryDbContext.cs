using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using log4net;
using Newtonsoft.Json;
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

        public void LogEntityValidationException(DbEntityValidationException ex)
        {
            foreach (var vr in ex.EntityValidationErrors)
            {
                var entity = vr?.Entry?.Entity;
                var entityJson = entity != null ? JsonConvert.SerializeObject(vr.Entry) : "null";
                logger.ErrorFormat("Database validation error occurred; Entity is valid? {0} | Entity JSON: '{1}'", 
                    vr.IsValid, entityJson);

                foreach (var ve in vr.ValidationErrors)
                {
                    logger.ErrorFormat("Error on property '{1}': {0}",
                        ve.ErrorMessage, ve.PropertyName);
                }
            }

            logger.Error(ex);
        }
    }
}
