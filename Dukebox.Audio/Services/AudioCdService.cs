using Dukebox.Audio.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Audio.Services
{
    public class AudioCdService : IAudioCdService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <returns></returns>
        public int GetCdDriveIndex(char driveLetter)
        {
            driveLetter = Char.ToLower(driveLetter);

            DriveInfo[] drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom).ToArray();
            DriveInfo drive = null;

            int driveIndex = 0;

            for (; driveIndex < drives.Length; driveIndex++)
            {
                if (drives[driveIndex].Name.ToLower() == driveLetter + @":\")
                {
                    drive = drives[driveIndex];
                    break;
                }
            }

            // Drive does not exist or is not a CD drive.
            if (drive == null)
            {
                throw new Exception("Drive '" + driveLetter + "' does not exist or is not a CD drive!");
            }
            else if (!IsAudioCd(drive))
            {
                throw new Exception("Drive '" + driveLetter + "' does not contain an audio CD!");
            }

            return driveIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        private bool IsAudioCd(DriveInfo drive)
        {
            bool result = false;
            string[] driveContents = Directory.GetFiles(drive.RootDirectory.FullName);

            result = (driveContents.Count() == driveContents.Where(f => (new FileInfo(f)).Extension == ".cda").Count());

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_fileName"></param>
        /// <returns></returns>
        public int GetTrackNumberFromCdaFilename(string fileName)
        {
            return Int32.Parse(fileName.Split('\\').LastOrDefault().Substring(5, 2)) - 1;
        }
    }
}
