using System.Threading;
using Xunit;
using Dukebox.Tests.UI.Screens;

namespace Dukebox.Tests.UI
{
    public class ToolbarTests : TestsBase
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
    }
}
