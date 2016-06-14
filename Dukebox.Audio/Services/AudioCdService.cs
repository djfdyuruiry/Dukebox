using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Dukebox.Audio.Interfaces;

namespace Dukebox.Audio.Services
{
    public class AudioCdService : IAudioCdService
    {
        public int GetCdDriveIndex(char driveLetter)
        {
            var driveLetterAsString = char.ToLower(driveLetter).ToString();
            
            var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom).ToArray();
            var drive = drives.FirstOrDefault(d => d.Name.ToLower(CultureInfo.InvariantCulture).StartsWith(driveLetterAsString));

            // Drive does not exist or is not a CD drive.
            if (drive == null)
            {
                throw new Exception(string.Format("Drive '{0}' does not exist or is not a CD drive!", driveLetterAsString));
            }

            var driveIndex = Array.IndexOf(drives, drive);

            return driveIndex;
        }

        public bool IsAudioCd(char driveLetter)
        {
            var driveLetterAsString = char.ToLower(driveLetter).ToString();
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name.ToLower(CultureInfo.InvariantCulture).StartsWith(driveLetterAsString));
            var result = false;

            if (!drive.IsReady)
            {
                return result;
            }

            var driveContents = Directory.GetFiles(drive.RootDirectory.FullName);

            result = driveContents.Select(f => new FileInfo(f)).Any(f => f.Extension == ".cda");

            return result;
        }

        public int GetTrackNumberFromCdaFilename(string fileName)
        {
            return Int32.Parse(fileName.Split('\\').LastOrDefault().Substring(5, 2)) - 1;
        }
    }
}
