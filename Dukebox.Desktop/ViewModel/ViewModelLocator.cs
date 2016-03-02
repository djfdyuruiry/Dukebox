using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library;
using GalaSoft.MvvmLight.Ioc;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dukebox.Desktop.ViewModel
{
    public class ViewModelLocator
    {
        private static Container container;

        static ViewModelLocator()
        {
            container = new Container();

            container.RegisterSingleton<IMainWindowViewModel, MainWindowViewModel>();
            container.RegisterSingleton<IPlaybackMonitorViewModel, PlaybackMonitorViewModel>();
            container.RegisterSingleton<ITrackListingViewModel, LibraryListingViewModel>();
            container.RegisterSingleton<IAlbumListingViewModel, AlbumListingViewModel>();
            container.RegisterSingleton<IArtistListingViewModel, ArtistListingViewModel>();
            container.RegisterSingleton<ISearchControlViewModel, SearchControlViewModelDummy>();
            container.RegisterSingleton<LibraryListingViewModel, LibraryListingViewModel>();
            container.RegisterSingleton<RecentlyPlayedListingViewModel, RecentlyPlayedListingViewModel>();
            container.RegisterSingleton<AudioCdViewModel, AudioCdViewModel>();
            container.RegisterSingleton<ImageToImageSourceConverter>();

            var assemblies = new List<Assembly>
            {
                Assembly.GetAssembly(typeof(LibraryPackage))
            };

            container.RegisterPackages(assemblies);
        }

        public IMainWindowViewModel MainWindow
        {
            get
            {
                return container.GetInstance<IMainWindowViewModel>();
            }
        }

        public IPlaybackMonitorViewModel IPlaybackMonitor
        {
            get
            {
                return container.GetInstance<IPlaybackMonitorViewModel>();
            }
        }

        public ITrackListingViewModel ILibraryListing
        {
            get
            {
                return container.GetInstance<ITrackListingViewModel>();
            }
        }

        public IAlbumListingViewModel IAlbumListing
        {
            get
            {
                return container.GetInstance<IAlbumListingViewModel>();
            }
        }

        public IArtistListingViewModel IArtistListing
        {
            get
            {
                return container.GetInstance<IArtistListingViewModel>();
            }
        }

        public ISearchControlViewModel ISearchControl
        {
            get
            {
                return container.GetInstance<ISearchControlViewModel>();
            }
        }

        public ITrackListingViewModel LibraryListing
        {
            get
            {
                return container.GetInstance<LibraryListingViewModel>();
            }
        }

        public ITrackListingViewModel RecentlyPlayedListing
        {
            get
            {
                return container.GetInstance<RecentlyPlayedListingViewModel>();
            }
        }

        public ITrackListingViewModel AudioCdListing
        {
            get
            {
                return container.GetInstance<AudioCdViewModel>();
            }
        }        
    }
}
