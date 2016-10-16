using System;
using System.Collections.Generic;
using System.Reflection;
using SimpleInjector;
using SimpleInjector.Packaging;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Services;
using Dukebox.Desktop.ViewModel;
using Dukebox.Library;
using Dukebox.Desktop.Factories;

namespace Dukebox.Desktop
{
    public class DesktopContainer : IPackage
    {        
        private static Container container;

        public static bool ExecutingForUnitTests { get; set; }

        static DesktopContainer()
        {
            container = new Container();
            Configure(container);
        }

        private static void Configure(Container container)
        {
            // user settings
            container.RegisterSingleton<IDukeboxUserSettings, DukeboxUserSettings>();

            // window view model
            container.RegisterSingleton<ILoadingScreenViewModel, LoadingScreenViewModel>();
            container.RegisterSingleton<IInitalImportWindowViewModel, InitalImportWindowViewModel>();
            container.RegisterSingleton<IMainWindowViewModel, MainWindowViewModel>();
            container.RegisterSingleton<IMetadataColumnsSettingsViewModel, MetadataColumnsSettingsViewModel>();
            container.RegisterSingleton<IWatchFolderSettingsViewModel, WatchFolderSettingsViewModel>();

            // menu view models
            container.RegisterSingleton<IFileMenuViewModel, FileMenuViewModel>();
            container.RegisterSingleton<IAudioCdMenuViewModel, AudioCdMenuViewModel>();
            container.RegisterSingleton<IPlaybackMenuViewModel, PlaybackMenuViewModel>();
            container.RegisterSingleton<IPlaylistMenuViewModel, PlaylistMenuViewModel>();
            container.RegisterSingleton<IHelpMenuViewModel, HelpMenuViewModel>();
            container.RegisterSingleton<ISettingsMenuViewModel, SettingsMenuViewModel>();

            // screen view models
            container.RegisterSingleton<IPlaybackMonitorViewModel, PlaybackMonitorViewModel>();
            container.RegisterSingleton<IAlbumListingViewModel, AlbumListingViewModel>();
            container.RegisterSingleton<IArtistListingViewModel, ArtistListingViewModel>();
            container.RegisterSingleton<IPlaylistListingViewModel, PlaylistListingViewModel>();
            container.RegisterSingleton<TrackListingPreviewViewModel>();
            container.RegisterSingleton<LibraryListingViewModel>();
            container.RegisterSingleton<RecentlyPlayedListingViewModel>();
            container.RegisterSingleton<AudioCdViewModel>();

            // services
            container.Register<Album>(); 
            container.RegisterSingleton<TrackSourceFactory>();

            // register external packages
            var assemblies = new List<Assembly> {Assembly.GetAssembly(typeof(LibraryPackage))};

            container.RegisterPackages(assemblies);
        }

        public static TService GetInstance<TService>() where TService : class
        {
            return container.GetInstance<TService>();
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
