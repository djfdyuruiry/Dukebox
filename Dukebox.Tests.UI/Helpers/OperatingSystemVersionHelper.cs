using System;

namespace Dukebox.Tests.UI.Helpers
{
    public static class OperatingSystemVersionHelper
    {
        public static double OsVersion { get; private set; }

        public static bool OsIsWindows8OrAbove
        {
            get
            {
                return OsVersion > 6.1f;
            }
        }
        public static bool OsIsWindows7OrBelow
        {
            get
            {
                return OsVersion < 6.2f;
            }
        }

        static OperatingSystemVersionHelper()
        {
            var osVersion = Environment.OSVersion.Version;
            var osVersionStr = $"{osVersion.Major}.{osVersion.Minor}";

            OsVersion = double.Parse(osVersionStr);
        }
    }
}
