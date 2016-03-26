namespace Dukebox.Tests
{
    public static class AudioCdTestConstants
    {
        public const char CdDriveLetter = 'v';
        public const int CdDriveIndex = 2;
        public const string CheckDriveMessage = "Please check that an AudioCD is in drive 'v'";
        public readonly static string CdDrivePath;

        static AudioCdTestConstants()
        {
            CdDrivePath = string.Format(@"{0}:\", CdDriveLetter);
        }
    }
}
