using System;
using System.IO;
using Newtonsoft.Json;
using Dukebox.Tests.UI.Model;

namespace Dukebox.Tests.UI.Helpers
{
    public static class ScreenshotCsvLogger
    {
        private static readonly string csvLogName;

        static ScreenshotCsvLogger()
        {
            var currentPath = Environment.CurrentDirectory;

            csvLogName = Path.Combine(currentPath, "target/teststream.txt");

            if (File.Exists(csvLogName))
            {
                File.Delete(csvLogName);
            }
        }
        
        public static void LogScreenshot(CapturedUiScreenshotInfo screenshotInfo)
        {
            if (screenshotInfo == null)
            {
                throw new ArgumentNullException(nameof(screenshotInfo));
            }

            var screenshotInfoJson = JsonConvert.SerializeObject(screenshotInfo);

            File.AppendAllText(csvLogName, $"{screenshotInfoJson}\n");
        }
    }
}
