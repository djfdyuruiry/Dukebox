using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dukebox.Tests.UI.Helpers
{
    public static class ScreenshotCsvLogger
    {
        private static readonly string csvLogName;

        static ScreenshotCsvLogger()
        {
            var currentPath = Environment.CurrentDirectory;

            csvLogName = Path.Combine(currentPath, "testScreenshots.csv");

            if (File.Exists(csvLogName))
            {
                File.Delete(csvLogName);
            }
        }

        public static void LogScreenshots(List<string> screenshotPaths)
        {
            if (screenshotPaths == null)
            {
                throw new ArgumentNullException(nameof(screenshotPaths));
            }

            if (!screenshotPaths.Any())
            {
                return;
            }

            var pathsCsv = string.Join(",", screenshotPaths.ToArray());

            LogScreenshot(pathsCsv);
        }

        public static void LogScreenshot(string screenshotPath)
        {
            if (string.IsNullOrEmpty(screenshotPath))
            {
                throw new ArgumentNullException(nameof(screenshotPath));
            }

            var csvText = File.Exists(csvLogName) ? File.ReadAllText(csvLogName) : string.Empty;
            var csvValues = csvText.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

            csvValues.Add(screenshotPath);

            var newCsvText = string.Join(",", csvValues.ToArray());

            File.Delete(csvLogName);
            File.WriteAllText(csvLogName, newCsvText);
        }
    }
}
