using Dukebox.Audio;
using Dukebox.Library.Config;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model.Services;
using Dukebox.Library.Repositories;
using Dukebox.Library.Services;
using Dukebox.Model.Services;
using log4net;
using SimpleInjector;
using SimpleInjector.Packaging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dukebox.Library
{
    public class LibraryPackage : IPackage
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Container container;

        static LibraryPackage()
        {
            container = new Container();
            Configure(container);            
        }

        private static void Configure(Container container)
        {
            container.RegisterSingleton<IDukeboxSettings, DukeboxSettings>();
            container.RegisterSingleton<IAlbumArtCacheService, AlbumArtCacheService>();
            container.RegisterSingleton<ICdMetadataService, CdMetadataService>();
            container.Register<Track>();
            container.Register<Playlist>();
            container.Register<AudioFileMetaData>();
            container.RegisterSingleton<IAudioCdRippingService, AudioCdRippingService>();
            container.RegisterSingleton<IMusicLibrary>(() => GetMusicLibraryInstance(container));

            var assemblies = new List<Assembly> {Assembly.GetAssembly(typeof(AudioPackage))};

            container.RegisterPackages(assemblies);
        }

        public static TService GetInstance<TService>() where TService : class
        {
            return container.GetInstance<TService>();
        }

        private static IMusicLibrary GetMusicLibraryInstance(Container container)
        {
            try
            {
                var musicLibrary = new MusicLibrary(container.GetInstance<IDukeboxSettings>(),
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

        public void RegisterServices(Container container)
        {
            Configure(container);
        }
    }
}
