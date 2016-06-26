using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using SimpleInjector;
using SimpleInjector.Packaging;
using Dukebox.Audio;
using Dukebox.Configuration.Interfaces;
using Dukebox.Library.Helper;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using Dukebox.Library.Repositories;
using Dukebox.Library.Factories;
using Dukebox.Library.Services.MusicLibrary;

namespace Dukebox.Library
{
    public class LibraryPackage : IPackage
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private static void Configure(Container container)
        {
            container.RegisterSingleton(() => GetAlbumArtCacheServiceInstance(container));

            container.RegisterSingleton<IAudioPlaylist, AudioPlaylist>();

            container.RegisterSingleton<AudioFileMetadataFactory, AudioFileMetadataFactory>();
            container.RegisterSingleton<TrackFactory, TrackFactory>();
            container.RegisterSingleton<IMusicLibraryDbContextFactory, MusicLibraryDbContextFactory>();

            container.RegisterSingleton<ICdMetadataService, CdMetadataService>();
            container.RegisterSingleton<IAudioCdRippingService, AudioCdRippingService>();
            container.RegisterSingleton<IAudioCdDriveMonitoringService, AudioCdDriveMonitoringService>();
            container.RegisterSingleton<IMusicLibraryEventService, MusicLibraryEventService>();
            container.RegisterSingleton<IMusicLibraryCacheService, MusicLibraryCacheService>();
            container.RegisterSingleton<IMusicLibraryUpdateService, MusicLibraryUpdateService>();
            container.RegisterSingleton<IMusicLibrarySearchService, MusicLibrarySearchService>();
            container.RegisterSingleton<IMusicLibraryImportService, MusicLibraryImportService>();
            container.RegisterSingleton<IPlaylistGeneratorService, PlaylistGeneratorService>();
            container.RegisterSingleton<ITrackGeneratorService, TrackGeneratorService>();

            container.RegisterSingleton<IMusicLibraryRepository, MusicLibraryRepository>();
            container.RegisterSingleton<IRecentlyPlayedRepository, RecentlyPlayedRepository>();

            var assemblies = new List<Assembly> { Assembly.GetAssembly(typeof(AudioPackage)) };

            container.RegisterPackages(assemblies);

            container.RegisterSingleton<DukeboxInitialisationHelper>();
        }

        internal static TService GetInstance<TService>() where TService : class
        {
            var container = new Container();
            Configure(container);

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

        public void RegisterServices(Container container)
        {
            Configure(container);
        }
    }
}
