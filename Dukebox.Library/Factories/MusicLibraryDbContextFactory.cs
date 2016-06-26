using System;
using System.Data.Entity.Validation;
using System.IO;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Repositories;
using System.Threading.Tasks;
using Dukebox.Library.Model;

namespace Dukebox.Library.Factories
{
    public class MusicLibraryDbContextFactory : IMusicLibraryDbContextFactory
    {
        private const string appDataFolderName = "Dukebox";
        private const string libraryDbFileName = "library.s3db";

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMusicLibraryEventService _eventService;
        private readonly string _dbFilePath;

        public MusicLibraryDbContextFactory(IMusicLibraryEventService eventService)
        {
            _eventService = eventService;
            _dbFilePath = CreateDbFileIfMissingAndSetConnectionString();
        }

        /// <summary>
        /// Ensure application data folders are correct and a DB file is present, also populate 
        /// DB connection string in app settings.
        /// </summary>
        private string CreateDbFileIfMissingAndSetConnectionString()
        {
            var appDirectoryUri = Assembly.GetExecutingAssembly().CodeBase;
            var appDirectory = Path.GetDirectoryName(appDirectoryUri.Replace("file:///", string.Empty));
            var libraryDbFilePath = Path.Combine(appDirectory, libraryDbFileName);

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appDataPath = Path.Combine(appDataPath, appDataFolderName);

            if (!Directory.Exists(appDataPath))
            {
                // create app data directory
                Directory.CreateDirectory(appDataPath);
            }

#if !DEBUG 
            var dbFilePath = Path.Combine(appDataPath, libraryDbFileName);
#else
            var dbFilePath = "./library.s3db";
            appDataPath = Environment.CurrentDirectory;
#endif

            if (!File.Exists(dbFilePath))
            {
                // create new library DB file
                File.Copy(libraryDbFilePath, dbFilePath);
            }

            // populate database connection string
            AppDomain.CurrentDomain.SetData("DataDirectory", appDataPath);

            logger.Debug($"DataDirectory set to '{appDataPath}'");
            logger.Debug($"Database file path set to '{dbFilePath}'");

            return dbFilePath;
        }

        public IMusicLibraryDbContext GetInstance()
        {
            try
            {
                return new MusicLibraryDbContext();
            }
            catch (Exception ex)
            {
                var errMsg = $"Error opening connection to SQLite music library database at path: {_dbFilePath}";

                logger.Error(errMsg, ex);
                throw new Exception(errMsg, ex);
            }
        }
        
        public async Task SaveDbChanges(IMusicLibraryDbContext dukeboxData)
        {
            try
            {
                await dukeboxData.SaveChangesAsync();
                _eventService.TriggerEvent(MusicLibraryEvent.DatabaseChangesSaved);
            }
            catch (DbEntityValidationException ex)
            {
                logger.Error("Error updating database due to entity validation errors", ex);
                LogEntityValidationException(ex);
            }
            catch (Exception ex)
            {
                logger.Error("Error updating database", ex);
            }
        }

        private void LogEntityValidationException(DbEntityValidationException ex)
        {
            foreach (var vr in ex.EntityValidationErrors)
            {
                var entity = vr?.Entry?.Entity;
                var entityJson = entity != null ? JsonConvert.SerializeObject(vr.Entry) : "null";
                logger.ErrorFormat("Database validation error occurred; Entity is valid? {0} | Entity JSON: '{1}'",
                    vr.IsValid, entityJson);

                foreach (var ve in vr.ValidationErrors)
                {
                    logger.ErrorFormat("Error on property '{1}': {0}", ve.ErrorMessage, ve.PropertyName);
                }
            }

            logger.Error(ex);
        }
    }
}
