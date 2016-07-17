using System;
using System.IO;
using System.Linq;
using Xunit;
using Dukebox.Tests.UI.Applciations;
using Dukebox.Tests.UI.Helpers;
using Dukebox.Tests.UI.Model;

namespace Dukebox.Tests.UI
{
    public class TestsBase : IDisposable
    {
        private const string appDataFolderName = "Dukebox";

        protected static readonly WindowScreenshotHelper screenshotHelper;

        public DukeboxApplication DukeboxApp { get; private set; }
        public TestsBaseOptions Options { get; private set; }
        public string TestMethodName { get; protected set; }

        private readonly string _testClassName;

        private bool _lastTestResult;

        static TestsBase()
        {
            var currentPath = Path.Combine(Environment.CurrentDirectory, "target/screenshots");

            if (Directory.Exists(currentPath))
            {
                Directory.Delete(currentPath, true);
            }

            screenshotHelper = new WindowScreenshotHelper(currentPath);
        }

        public TestsBase() : this(new TestsBaseOptions())
        {
        }

        public TestsBase(TestsBaseOptions options)
        {            
            this.Options = options;

            _testClassName = GetType().FullName;

#if !DEBUG
            DeleteUserDataIfPresent();
#endif

            DukeboxApp = new DukeboxApplication();
            DukeboxApp.Launch(this.Options.DismissHotkeyWarningDialog);

            if (this.Options.SkipInitalImport)
            {
                DukeboxApp.SkipInitalImport();
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

        protected void AssertTrue(bool assertion, string message)
        {
            DoAssertWithSnaphot(() => Assert.True(assertion, message));
        }

        protected void AssertFalse(bool assertion, string message)
        {
            DoAssertWithSnaphot(() => Assert.False(assertion, message));
        }

        private void DoAssertWithSnaphot(Action assertionAction)
        {
            try
            {
                assertionAction?.Invoke();
                _lastTestResult = true;                                
            }
            catch (Exception)
            {
                _lastTestResult = false;
                throw;
            }
        }

        public void Dispose()
        {
            if (!_lastTestResult || Options.SaveScreenshotsForPassingTests)
            {
                TakeScreenshot(TestMethodName, _lastTestResult);
            }

            DukeboxApp.Dispose();
        }

        private void TakeScreenshot(string testMethodName, bool testPassed)
        {
            var testClassAndMethodName = $"{_testClassName}.{testMethodName}";

            try
            {
                var appWindows = DukeboxApp.ApplicationHandle.GetWindows();

                if (Options.ScreenshotAllWindows)
                {
                    screenshotHelper.TakeWindowScreenshots(appWindows, testClassAndMethodName);
                }
                else
                {
                    var appWindow = appWindows.First();
                    screenshotHelper.TakeWindowScreenshot(appWindow, testClassAndMethodName);
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
