using System;
using Xunit;
using Dukebox.Tests.UI.Applciations;
using Dukebox.Tests.UI.Helpers;
using Dukebox.Tests.UI.Model;
using System.IO;
using System.Runtime.CompilerServices;

namespace Dukebox.Tests.UI
{
    public class TestsBase : IDisposable
    {
        protected static readonly WindowScreenshotHelper _screenshotHelper;

        private readonly TestsBaseOptions _options;
        protected readonly DukeboxApplication _dukeboxApp;

        public TestsBase() : this(new TestsBaseOptions())
        {
        }

        static TestsBase()
        {
            var currentPath = Path.Combine(Environment.CurrentDirectory, "screenshots");

            if (Directory.Exists(currentPath))
            {
                Directory.Delete(currentPath, true);
            }

            _screenshotHelper = new WindowScreenshotHelper(currentPath);
        }

        public TestsBase(TestsBaseOptions options)
        {            
            _options = options;

            _dukeboxApp = new DukeboxApplication();
            _dukeboxApp.Launch(_options.DismissHotkeyWarningDialog);

            if (_options.SkipInitalImport)
            {
                _dukeboxApp.SkipInitalImport();
            }
        }

        public void Dispose()
        {
            _dukeboxApp.Dispose();
        }

        protected void AssertTrue(bool assertion, string message, [CallerMemberName] string testMethodName = null)
        {
            if (!assertion || _options.SaveScreenshotsForPassingTests)
            {
                TakeScreenshots(testMethodName);
            }

            Assert.True(assertion, message);
        }

        protected void AssertFalse(bool assertion, string message, [CallerMemberName] string testMethodName = null)
        {
            if (assertion || _options.SaveScreenshotsForPassingTests)
            {
                TakeScreenshots(testMethodName);
            }

            Assert.False(assertion, message);
        }

        private void TakeScreenshots(string testMethodName)
        {
            var appWindows = _dukeboxApp.ApplicationHandle.GetWindows();
            var screenshotPaths = _screenshotHelper.TakeWindowScreenshots(appWindows, testMethodName);

            screenshotPaths.ForEach(ScreenshotCsvLogger.LogScreenshot);
        }
    }
}
