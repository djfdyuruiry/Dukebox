using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using TestStack.White.UIItems.WindowItems;

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

        public string TakeWindowScreenshots(List<Window> windows, string screenshotName)
        {
            if (windows == null)
            {
                throw new ArgumentNullException(nameof(windows));
            }

            if (!windows.Any())
            {
                throw new InvalidOperationException($"Parameter list {nameof(windows)} is empty");
            }

            var width = windows.Max(w => w.Bounds.Width);
            var height = windows.Sum(w => w.Bounds.Height);

            using (var image = new Bitmap((int)width, (int)height))
            {
                var imageGraphics = Graphics.FromImage(image);
                var currentY = 0;

                foreach (var window in windows)
                {
                    var windowImage = window.VisibleImage;

                    imageGraphics.DrawImage(windowImage, 0, currentY);

                    currentY += windowImage.Height;
                }

                return SaveWindowScreenshot(image, screenshotName);
            }
        }

        private string SaveWindowScreenshot(Image image, string screenshotName)
        {
            if (string.IsNullOrEmpty(screenshotName))
            {
                throw new ArgumentNullException(nameof(screenshotName));
            }

            var filePath = $"{screenshotName}{ScreenshotFileExtension}";
            filePath = Path.Combine(OutputPath, filePath);

            image.Save(filePath, ScreenshotImageFormat);

            return filePath;
        }

        public string TakeWindowScreenshot(Window window, string screenshotName)
        {
            using (var windowImage = window.VisibleImage)
            {
                return SaveWindowScreenshot(windowImage, screenshotName);
            }            
        }
    }
}
