using Dukebox.Desktop.Interfaces;
using GalaSoft.MvvmLight.Ioc;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
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
            container.RegisterSingleton<ISongListingViewModel, LibraryListingViewModel>();
            container.RegisterSingleton<IAlbumListingViewModel, AlbumListingViewModel>();
            container.RegisterSingleton<IArtistListingViewModel, ArtistListingViewModel>();
            container.RegisterSingleton<ISearchControlViewModel, SearchControlViewModelDummy>();
            container.RegisterSingleton<LibraryListingViewModel, LibraryListingViewModel>();
            container.RegisterSingleton<RecentlyPlayedListingViewModel, RecentlyPlayedListingViewModel>();
            container.RegisterSingleton<AudioCdViewModel, AudioCdViewModel>();
        }

        public IMainWindowViewModel MainWindow
        {
            get
            {
                return container.GetInstance<IMainWindowViewModel>();
            }
        }

        public ISongListingViewModel ILibraryListing
        {
            get
            {
                return container.GetInstance<ISongListingViewModel>();
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

        public ISongListingViewModel LibraryListing
        {
            get
            {
                return container.GetInstance<LibraryListingViewModel>();
            }
        }

        public ISongListingViewModel RecentlyPlayedListing
        {
            get
            {
                return container.GetInstance<RecentlyPlayedListingViewModel>();
            }
        }

        public ISongListingViewModel AudioCdListing
        {
            get
            {
                return container.GetInstance<AudioCdViewModel>();
            }
        }        
    }
}
