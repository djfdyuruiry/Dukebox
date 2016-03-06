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
        public IMainWindowViewModel MainWindow
        {
            get
            {
                return DesktopContainer.GetInstance<IMainWindowViewModel>();
            }
        }

        public IPlaybackMonitorViewModel IPlaybackMonitor
        {
            get
            {
                return DesktopContainer.GetInstance<IPlaybackMonitorViewModel>();
            }
        }

        public ITrackListingViewModel ILibraryListing
        {
            get
            {
                return DesktopContainer.GetInstance<ITrackListingViewModel>();
            }
        }

        public IAlbumListingViewModel IAlbumListing
        {
            get
            {
                return DesktopContainer.GetInstance<IAlbumListingViewModel>();
            }
        }

        public IArtistListingViewModel IArtistListing
        {
            get
            {
                return DesktopContainer.GetInstance<IArtistListingViewModel>();
            }
        }

        public ISearchControlViewModel ISearchControl
        {
            get
            {
                return DesktopContainer.GetInstance<ISearchControlViewModel>();
            }
        }

        public ITrackListingViewModel LibraryListing
        {
            get
            {
                return DesktopContainer.GetInstance<LibraryListingViewModel>();
            }
        }

        public ITrackListingViewModel RecentlyPlayedListing
        {
            get
            {
                return DesktopContainer.GetInstance<RecentlyPlayedListingViewModel>();
            }
        }

        public ITrackListingViewModel AudioCdListing
        {
            get
            {
                return DesktopContainer.GetInstance<AudioCdViewModel>();
            }
        }        
    }
}
