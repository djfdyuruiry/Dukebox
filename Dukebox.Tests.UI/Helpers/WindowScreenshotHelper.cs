using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using TestStack.White.UIItems.WindowItems;
using Dukebox.Library.Helper;

namespace Dukebox.Tests.UI.Helpers
{
    public class WindowScreenshotHelper
    {
        public string OutputPath {get; private set;}
        public ImageFormat ScreenshotImageFormat { get; private set; }
        public string ScreenshotFileExtension { get; private set; }
        
        public WindowScreenshotHelper(ImageFormat imageFormat)
        {
            ScreenshotImageFormat = imageFormat;

            var imageCodec = ImageCodecInfo.GetImageEncoders()
                .First(x => x.FormatID == ScreenshotImageFormat.Guid);

            ScreenshotFileExtension = imageCodec.FilenameExtension
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .First()
                .Trim('*');
        }

        public WindowScreenshotHelper() : this(ImageFormat.Png)
        {
            OutputPath = Path.GetTempPath();
        }

        public WindowScreenshotHelper(string outputPath) : this(outputPath, ImageFormat.Png)
        {
        }

        public WindowScreenshotHelper(string outputDirectory, ImageFormat outputImageFormat) : this(outputImageFormat)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            Directory.CreateDirectory(outputDirectory);

            OutputPath = outputDirectory;
        }

        public List<string> TakeWindowScreenshots(List<Window> windows, string screenshotPrefix)
        {
            if (windows == null)
            {
                throw new ArgumentNullException(nameof(windows));
            }

            if (!windows.Any())
            {
                return new List<string>();
            }

            var screenshotPaths = windows.Select(w => TakeWindowScreenshot(w, screenshotPrefix)).ToList();

            return screenshotPaths;
        }

        public string TakeWindowScreenshot(Window window, string screenshotPrefix)
        {
            var windowTitle = string.IsNullOrEmpty(window.Title) ? Guid.NewGuid().ToString().Substring(0, 4) : window.Title;
            var fileName = $"{screenshotPrefix ?? string.Empty}_{windowTitle}_{DateTime.UtcNow}{ScreenshotFileExtension}";

            fileName = StringToFilenameConverter.ConvertStringToValidFileName(fileName);

            var filePath = Path.Combine(OutputPath, fileName);

            using (var windowImage = window.VisibleImage)
            {
                windowImage.Save(filePath, ScreenshotImageFormat);
            }

            return filePath;
        }
    }
}
