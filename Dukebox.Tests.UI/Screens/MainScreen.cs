﻿using System.Linq;
using TestStack.White.ScreenObjects;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.UIItems.WindowStripControls;
using Dukebox.Tests.UI.Controls;

namespace Dukebox.Tests.UI.Screens
{
    public class MainScreen : AppScreen
    {
        public MenuBar ToolbarMenu;

        public Button CurrentlyPlayingNavIcon;
        public Button LibraryNavIcon;
        public Button AlbumsNavIcon;
        public Button ArtistsNavIcon;
        public Button RecentlyPlayedNavIcon;
        public Button PlaylistNavIcon;
        public Button AudioCdNavIcon;

        public TrackListingControl CurrentlyPlayingListingControl;
        public TrackListingControl LibraryListingControl;
        public AlbumListingControl AlbumListingControl;
        public ArtistListingControl ArtistListingControl;
        public PlaylistListingControl PlaylistListingControl;
        public TrackListingControl RecentlyPlayedListingControl;
        public AudioCdListingControl AudioCdListingControl;
        public TrackListingControl TrackListingPreviewControl;

        public MainScreen(Window window, ScreenRepository screenRepository) : base(window, screenRepository)
        {
        }

        public virtual void Exit()
        {
            var fileMenu = ToolbarMenu.MenuItemBy(SearchCriteria.ByAutomationId("FileMenu"));

            fileMenu
                .ChildMenus
                .First(m => m.AutomationElement.Current.AutomationId == "ExitMenuItem")
                .Click();
        }
    }
}
