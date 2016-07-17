namespace Dukebox.Tests.UI.Model
{
    public class TestsBaseOptions
    {
        public bool SkipInitalImport { get; set; }
        public bool DismissHotkeyWarningDialog { get; set; }
        public bool SaveScreenshotsForPassingTests { get; set; }
        public bool ScreenshotAllWindows { get; set; }

        public TestsBaseOptions()
        {
            SkipInitalImport = true;
            DismissHotkeyWarningDialog = true;
            SaveScreenshotsForPassingTests = true;
            ScreenshotAllWindows = true;
        }
    }
}
