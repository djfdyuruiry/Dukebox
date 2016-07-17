using Xunit;
using Dukebox.Tests.UI.Screens;

namespace Dukebox.Tests.UI
{
    public class NavigationTests : TestsBase
    {
        [Fact]
        public void When_CurrentlyPlaying_Nav_Icon_Clicked_CurrentlyPlaying_Listing_Should_Be_Shown()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();

            mainScreen.CurrentlyPlayingNavIcon.Click();

            var listingWasShown = mainScreen.CurrentlyPlayingListingControl.Visible;

            AssertTrue(listingWasShown, "App failed to show currently playing listing when nav icon was clicked");
        }

        [Fact]
        public void When_LibraryListing_Nav_Icon_Clicked_LibraryListing_Listing_Should_Be_Shown()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();

            mainScreen.LibraryNavIcon.Click();

            var listingWasShown = mainScreen.LibraryListingControl.Visible;

            AssertTrue(listingWasShown, "App failed to show library listing when nav icon was clicked");
        }

        [Fact]
        public void When_Album_Nav_Icon_Clicked_Album_Listing_Should_Be_Shown()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();

            mainScreen.AlbumsNavIcon.Click();

            var listingWasShown = mainScreen.AlbumListingControl.Visible;

            AssertTrue(listingWasShown, "App failed to show album listing when nav icon was clicked");
        }

        [Fact]
        public void When_Artist_Nav_Icon_Clicked_Album_Listing_Should_Be_Shown()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();

            mainScreen.ArtistsNavIcon.Click();

            var listingWasShown = mainScreen.ArtistListingControl.Visible;

            AssertTrue(listingWasShown, "App failed to show artist listing when nav icon was clicked");
        }

        [Fact]
        public void When_Playlist_Nav_Icon_Clicked_Album_Listing_Should_Be_Shown()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();

            mainScreen.PlaylistNavIcon.Click();

            var listingWasShown = mainScreen.PlaylistListingControl.Visible;

            AssertTrue(listingWasShown, "App failed to show playlist listing when nav icon was clicked");
        }

        [Fact]
        public void When_RecentlyPlayed_Nav_Icon_Clicked_RecentlyPlayed_Listing_Should_Be_Shown()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();

            mainScreen.RecentlyPlayedNavIcon.Click();

            var listingWasShown = mainScreen.RecentlyPlayedListingControl.Visible;

            AssertTrue(listingWasShown, "App failed to show recently playing listing when nav icon was clicked");
        }

        [Fact]
        public void When_AudioCd_Nav_Icon_Clicked_Album_Listing_Should_Be_Shown()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();

            mainScreen.AudioCdNavIcon.Click();

            var listingWasShown = mainScreen.AudioCdListingControl.Visible;

            AssertTrue(listingWasShown, "App failed to show audio cd listing when nav icon was clicked");
        }
    }
}
