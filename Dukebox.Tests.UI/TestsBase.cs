using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;
using Dukebox.Tests.UI.Applciations;
using Dukebox.Tests.UI.Helpers;
using Dukebox.Tests.UI.Model;

namespace Dukebox.Tests.UI
{
    public class TestsBase : IDisposable
    {
        private const string appDataFolderName = "Dukebox";

        protected static readonly WindowScreenshotHelper _screenshotHelper;

        private readonly TestsBaseOptions _options;
        private readonly string _testClassName;

        private bool lastTestResult;

        protected readonly DukeboxApplication _dukeboxApp;

        static TestsBase()
        {
            var currentPath = Path.Combine(Environment.CurrentDirectory, "target/screenshots");

            if (Directory.Exists(currentPath))
            {
                Directory.Delete(currentPath, true);
            }

            _screenshotHelper = new WindowScreenshotHelper(currentPath);
        }

        public TestsBase() : this(new TestsBaseOptions())
        {
        }

        public TestsBase(TestsBaseOptions options)
        {            
            _options = options;

            _testClassName = GetType().FullName;

#if !DEBUG
            DeleteUserDataIfPresent();
#endif

            _dukeboxApp = new DukeboxApplication();
            _dukeboxApp.Launch(_options.DismissHotkeyWarningDialog);

            if (_options.SkipInitalImport)
            {
                _dukeboxApp.SkipInitalImport();
            }
        }

        private void DeleteUserDataIfPresent()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            appDataPath = Path.Combine(appDataPath, appDataFolderName);

            if (Directory.Exists(appDataPath))
            {
                Directory.Delete(appDataPath, true);
            }
        }

        public void Dispose()
        {
            _dukeboxApp.Dispose();
        }

        protected void AssertTrue(bool assertion, string message, [CallerMemberName] string testMethodName = null)
        {
            DoAssertWithSnaphot(() => Assert.True(assertion, message), testMethodName);
        }

        protected void AssertFalse(bool assertion, string message, [CallerMemberName] string testMethodName = null)
        {
            DoAssertWithSnaphot(() => Assert.False(assertion, message), testMethodName);
        }

        private void DoAssertWithSnaphot(Action assertionAction, string testMethodName)
        {
            try
            {
                assertionAction?.Invoke();

                if (_options.SaveScreenshotsForPassingTests)
                {
                    lastTestResult = true;
                    TakeScreenshot(testMethodName, true);
                }
            }
            catch (Exception)
            {
                lastTestResult = false;
                TakeScreenshot(testMethodName, false);
                throw;
            }
        }

        private void TakeScreenshot(string testMethodName, bool testPassed)
        {
            var testClassAndMethodName = $"{_testClassName}.{testMethodName}";

            try
            {
                var appWindows = _dukeboxApp.ApplicationHandle.GetWindows();

                if (_options.ScreenshotAllWindows)
                {
                    _screenshotHelper.TakeWindowScreenshots(appWindows, testClassAndMethodName);
                }
                else
                {
                    var appWindow = appWindows.First();
                    _screenshotHelper.TakeWindowScreenshot(appWindow, testClassAndMethodName);
                }

                var screenshotInfo = new CapturedUiScreenshotInfo
                {
                    Method = testClassAndMethodName,
                    Class = _testClassName
                };

                screenshotInfo.SetStatus(testPassed);

                ScreenshotCsvLogger.LogScreenshot(screenshotInfo);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occurred while saving screenshot for unit test {testClassAndMethodName}: {ex}");
            }
        }
    }
}
