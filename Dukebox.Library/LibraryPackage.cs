using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using SimpleInjector;
using SimpleInjector.Packaging;
using Dukebox.Audio;
using Dukebox.Library.Config;
using Dukebox.Library.Helper;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using Dukebox.Library.Repositories;
using System.IO;

namespace Dukebox.Library
{
    public class LibraryPackage : IPackage
    {
        private const string appDataFolderName = "Dukebox";
        private const string libraryDbFileName = "library.s3db";

        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Container container;

        public static bool ExecutingForUnitTests { get; set; }

        static LibraryPackage()
        {
            container = new Container();
            Configure(container);            
        }

        private static void Configure(Container container)
        {
            container.RegisterSingleton<IDukeboxSettings, DukeboxSettings>();
            container.RegisterSingleton(() => GetAlbumArtCacheServiceInstance(container));
            container.RegisterSingleton<ICdMetadataService, CdMetadataService>();
            container.RegisterSingleton<IAudioCdRippingService, AudioCdRippingService>();
            container.RegisterSingleton<IAudioPlaylist, AudioPlaylist>();
            container.RegisterSingleton<IMusicLibraryDbContext>(() => new MusicLibraryDbContext());
            container.RegisterSingleton(() => GetMusicLibraryInstance(container));
            container.RegisterSingleton<IAudioCdDriveMonitoringService, AudioCdDriveMonitoringService>();
            container.Register<ITrack, Track>();
            container.Register<IAudioFileMetadata, AudioFileMetadata>();

            var assemblies = new List<Assembly> {Assembly.GetAssembly(typeof(AudioPackage))};

            container.RegisterPackages(assemblies);

            container.RegisterSingleton<DukeboxInitialisationHelper>();
        }

        public static TService GetInstance<TService>() where TService : class
        {
            return container.GetInstance<TService>();
        }

        private static IAlbumArtCacheService GetAlbumArtCacheServiceInstance(Container container)
        {
            try
            {
                var albumArtCacheService = new AlbumArtCacheService(container.GetInstance<IDukeboxSettings>());
                return albumArtCacheService;
            }
            catch (Exception ex)
            {
                var errMsg = "Error opening the album art cache";

                logger.Error(errMsg, ex);
                throw new Exception(errMsg, ex);
            }
        }

        private static IMusicLibrary GetMusicLibraryInstance(Container container)
        {
            try
            {
                EnsureLocalEnvironmentValid();

                var musicLibrary = new MusicLibrary(container.GetInstance<IMusicLibraryDbContext>(), container.GetInstance<IDukeboxSettings>(),
                    container.GetInstance<IAlbumArtCacheService>(), container.GetInstance<AudioFileFormats>());

                return musicLibrary;
            }
            catch (Exception ex)
            {
                var errMsg = "Error opening SQLite music library database";

                logger.Error(errMsg, ex);
                throw new Exception(errMsg, ex);
            }
        }

        /// <summary>
        /// Ensure application data folders are correct and a DB file is present, also populate DB connection string.
        /// </summary>
        private static void EnsureLocalEnvironmentValid()
        {
            var appDirectoryUri = Assembly.GetExecutingAssembly().CodeBase;
            var appDirectory = Path.GetDirectoryName(appDirectoryUri.Replace("file:///", string.Empty));
            var libraryDbFilePath = Path.Combine(appDirectory, libraryDbFileName);

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appDataPath = Path.Combine(appDataPath, appDataFolderName);

            var dbFilePath = Path.Combine(appDataPath, libraryDbFileName);

            if (!Directory.Exists(appDataPath))
            {
                // create app data directory
                Directory.CreateDirectory(appDataPath);
            }

#if DEBUG 
            if (File.Exists(dbFilePath))
            {
                // ensure fresh DB state on each debug run
                File.Delete(dbFilePath);
            }
#endif

            if (!File.Exists(dbFilePath))
            {
                // create new library DB file
                File.Copy(libraryDbFilePath, dbFilePath);
            }
            
            // populate database connection string
            AppDomain.CurrentDomain.SetData("DataDirectory", appDataPath);
        }

        public void RegisterServices(Container container)
        {
            Configure(container);
        }

        public static Container GetContainerForTestOverrides()
        {
            if (!ExecutingForUnitTests)
            {
                throw new InvalidOperationException("Accessing the internal container is only valid when ExecutingForUnitTests is true");
            }

            return container;
        }
    }
}
