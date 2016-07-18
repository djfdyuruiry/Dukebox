using System;

namespace Dukebox.Library.Helper
{
    public static class UnixTimestampHelper
    {
        public static DateTime UnixTimestampToDateTime(long unixTimestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
        }

        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            DateTimeOffset offset = dateTime;
            return offset.ToUnixTimeSeconds();
        }
    }
}
