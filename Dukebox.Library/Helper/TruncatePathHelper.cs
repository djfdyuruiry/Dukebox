using System;

namespace Dukebox.Library.Helper
{
    public static class TruncatePathHelper
    {
        public const int TrailingReportCharsToPrint = 50;

        public static string TruncatePath(string path)
        {
            var truncatedpath = path.Substring(Math.Max(0, path.Length - TrailingReportCharsToPrint));

            return path.Length > truncatedpath.Length ? $"...{truncatedpath}" : path;
        }
    }
}
