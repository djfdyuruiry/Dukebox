using System;
using System.Threading;
using Xunit;
using Dukebox.Tests.UI.Screens;
using Dukebox.Tests.UI.Applciations;

namespace Dukebox.Tests.UI
{
    public class MainWindowTests : IDisposable
    {
        private readonly DukeboxApplication _dukeboxApp;

        public MainWindowTests()
        {
            _dukeboxApp = new DukeboxApplication();
            _dukeboxApp.Launch();

            _dukeboxApp.SkipInitalImport();            
        }

        public void Dispose()
        {
            _dukeboxApp.Dispose();
        }

        [Fact]
        public void When_File_Menu_Exit_Item_Clicked_App_Should_Close()
        {
            var mainScreen = _dukeboxApp.GetScreen<MainScreen>();
             
            mainScreen.Exit();
            Thread.Sleep(250);

            var appIsClosed = _dukeboxApp.AppHasExited;
            Assert.True(appIsClosed);
        }
    }
}
