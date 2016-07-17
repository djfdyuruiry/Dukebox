using System.Linq;
using System.Threading;
using Xunit;
using Dukebox.Tests.UI.Screens;
using Dukebox.Tests.UI.Dialogs;

namespace Dukebox.Tests.UI
{
    public class FileMenuTests : TestsBase
    {
        [Fact]
        public void When_PlayFolder_MenuItem_Clicked_And_Folder_Is_Selected_Tracks_Should_Be_Loaded()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();
            var folderPath = @"C:\music";
            var trackCount = 6;

            var menuBar = mainScreen.GetMenuBar();
            menuBar.FileMenu.PlayFolder.Click();

            Thread.Sleep(250);
            
            var folderBrowser = _dukeboxApp.GetModal<SelectFolderDialog>();
          
            folderBrowser.SetFolderPathAndClickOk(folderPath);
            
            Thread.Sleep(250);

            var loadedTracks = mainScreen.CurrentlyPlayingListingControl.GetTracks();
            var distinctLoadedTitles = loadedTracks.Select(t => t.Title).Distinct().ToList();

            var allTracksLoaded = loadedTracks.Count == trackCount && distinctLoadedTitles.Count == trackCount;

            AssertTrue(allTracksLoaded, $"App failed to load all tracks when folder '${folderPath}' was selected in 'file -> play folder...'");
        }

        [Fact]
        public void When_Exit_MenuItem_Clicked_App_Should_Close()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();
            var menuBar = mainScreen.GetMenuBar();

            menuBar.FileMenu.Exit.Click();
            Thread.Sleep(250);

            var appIsClosed = _dukeboxApp.AppHasExited;
            AssertTrue(appIsClosed, "App failed to close when 'file -> exit' was clicked");
        }
    }
}
