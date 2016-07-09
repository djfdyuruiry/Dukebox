using System;
using System.Data.Entity.Validation;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Dukebox.Library.Helper;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Repositories;

namespace Dukebox.Library.Factories
{
    public class MusicLibraryDbContextFactory : IMusicLibraryDbContextFactory
    {
        private const string appDataFolderName = "Dukebox";
        private const string libraryDbFileName = "library_v0.9.s3db";

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMusicLibraryEventService _eventService;
        private readonly string _dbFilePath;
        private string _dbFileDirectory;

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
            var dbFilePath = "./library_v0.9.s3db";
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

            _dbFileDirectory = appDataPath;

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

        private void LogEntityValidationException(DbEntityValidationException dbValidationExecption)
        {
            foreach (var dbValidationResult in dbValidationExecption.EntityValidationErrors)
            {
                string entityJson = "null";
                var entity = dbValidationResult?.Entry?.Entity;

                if (entity != null)
                {
                    entityJson = JsonConvert.SerializeObject(entity);
                }

                logger.ErrorFormat("Database validation error occurred: Entity is valid? {0} | Entity JSON: '{1}'",
                    dbValidationResult.IsValid, entityJson);

                foreach (var ve in dbValidationResult.ValidationErrors)
                {
                    logger.ErrorFormat("Error on property '{1}': {0}", ve.ErrorMessage, ve.PropertyName);
                }
            }

            logger.Error(dbValidationExecption);
        }

        public string ImportLibraryFile(string libraryFileToImport)
        {
            var fileNameSafeDate = StringToFilenameConverter.ConvertStringToValidFileName(DateTime.UtcNow.ToString());
            var libraryBackupPath = Path.Combine(_dbFileDirectory, $"_dbFilePath_{fileNameSafeDate}.backup");

            File.Copy(_dbFilePath, libraryBackupPath);
            File.Delete(_dbFilePath);
            File.Copy(libraryFileToImport, _dbFilePath, true);

            return libraryBackupPath;
        }

        public void ExportCurrentLibraryFile(string outputPath)
        {
            File.Copy(_dbFilePath, outputPath, true);
        }
    }
}
