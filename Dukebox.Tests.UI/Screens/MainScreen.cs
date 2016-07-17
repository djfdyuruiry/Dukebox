using TestStack.White.ScreenObjects;
using TestStack.White.UIItems;
using TestStack.White.UIItems.WindowItems;
using Dukebox.Tests.UI.Controls;

namespace Dukebox.Tests.UI.Screens
{
    public class MainScreen : AppScreen
    {
        private readonly MenuBarControl _menuBar;

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
            _menuBar = new MenuBarControl(window);
        }

        public virtual MenuBarControl GetMenuBar()
        {
            return _menuBar;
        }
    }
}
