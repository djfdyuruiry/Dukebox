using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Services;
using Dukebox.Desktop.ViewModel;
using Dukebox.Library;
using SimpleInjector;
using SimpleInjector.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Desktop
{
    public class DesktopContainer : IPackage
    {        
        private static Container container;

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
            container.RegisterSingleton<IMainWindowViewModel, MainWindowViewModel>();

            // menu view models
            container.RegisterSingleton<IFileMenuViewModel, FileMenuViewModel>();
            container.RegisterSingleton<IAudioCdMenuViewModel, AudioCdMenuViewModel>();
            container.RegisterSingleton<IPlaybackMenuViewModel, PlaybackMenuViewModel>();
            container.RegisterSingleton<IPlaylistMenuViewModel, PlaylistMenuViewModel>();
            container.RegisterSingleton<IHelpMenuViewModel, HelpMenuViewModel>();

            // screen view models
            container.RegisterSingleton<IPlaybackMonitorViewModel, PlaybackMonitorViewModel>();
            container.RegisterSingleton<IAlbumListingViewModel, AlbumListingViewModel>();
            container.RegisterSingleton<IArtistListingViewModel, ArtistListingViewModel>();
            container.RegisterSingleton<LibraryListingViewModel, LibraryListingViewModel>();
            container.RegisterSingleton<RecentlyPlayedListingViewModel, RecentlyPlayedListingViewModel>();
            container.RegisterSingleton<AudioCdViewModel, AudioCdViewModel>();

            // helpers
            container.RegisterSingleton<ImageToImageSourceConverter>();

            // services
            container.Register<Album>();

            // register external packages
            var assemblies = new List<Assembly>
            {
                Assembly.GetAssembly(typeof(LibraryPackage))
            };

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
    }
}
