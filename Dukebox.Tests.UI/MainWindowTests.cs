using System.Threading;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.ScreenObjects;
using Xunit;
using Dukebox.Tests.UI.Screens;

namespace Dukebox.Tests.UI
{
    public class MainWindowTests
    {
        private readonly Application _application;
        private readonly ScreenRepository _screenRepository;

        public MainWindowTests()
        {
            _application = Application.Launch(@"..\..\..\..\Dukebox.Desktop\bin\x64\Debug\Dukebox.exe");
            _screenRepository = new ScreenRepository(_application.ApplicationSession);

            var initalImportScreen = _screenRepository.Get<InitalImportScreen>("Dukebox - Welcome", InitializeOption.NoCache);
            initalImportScreen.SkipImport();
        }

        [Fact]
        public void When_File_Menu_Exit_Item_Clicked_App_Should_Close()
        {
            var mainScreen = _screenRepository.Get<MainScreen>("Dukebox", InitializeOption.NoCache);
            mainScreen.Exit();

            Thread.Sleep(250);

            var appIsClosed = _application.ApplicationSession.Application.HasExited;

            Assert.True(appIsClosed);
        }
    }
}
