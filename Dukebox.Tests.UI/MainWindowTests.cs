using System.Threading;
using Xunit;
using Dukebox.Tests.UI.Screens;

namespace Dukebox.Tests.UI
{
    public class MainWindowTests : TestsBase
    {
        [Fact]
        public void When_File_Menu_Exit_Item_Clicked_App_Should_Close()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();
             
            mainScreen.Exit();
            Thread.Sleep(250);

            var appIsClosed = _dukeboxApp.AppHasExited;
            Assert.True(appIsClosed, "App failed to close when file->exit was clicked");
        }

        [Fact]
        public void When_Album_Nav_Icon_Click_Album_Listing_Should_Be_Shown()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();
            
            mainScreen.AlbumsNavIcon.Click();

            var listingWasShown = mainScreen.AlbumListingControl.Visible;

            Assert.True(listingWasShown, "App failed to show album listing when nav icon was clicked");
        }

        [Fact]
        public void When_Artist_Nav_Icon_Click_Album_Listing_Should_Be_Shown()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();

            mainScreen.ArtistsNavIcon.Click();

            var listingWasShown = mainScreen.ArtistListingControl.Visible;

            Assert.True(listingWasShown, "App failed to show artist listing when nav icon was clicked");
        }

        [Fact]
        public void When_Playlist_Nav_Icon_Click_Album_Listing_Should_Be_Shown()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();

            mainScreen.PlaylistNavIcon.Click();

            var listingWasShown = mainScreen.PlaylistListingControl.Visible;

            Assert.True(listingWasShown, "App failed to show playlist listing when nav icon was clicked");
        }

        [Fact]
        public void When_AudioCd_Nav_Icon_Click_Album_Listing_Should_Be_Shown()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();

            mainScreen.AudioCdNavIcon.Click();

            var listingWasShown = mainScreen.AudioCdListingControl.Visible;

            Assert.True(listingWasShown, "App failed to show audio cd listing when nav icon was clicked");
        }
    }
}
