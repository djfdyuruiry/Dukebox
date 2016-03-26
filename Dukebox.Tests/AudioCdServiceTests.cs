using Dukebox.Audio.Services;
using Xunit;

namespace Dukebox.Tests
{
    public class AudioCdServiceTests
    {
        [Fact]
        public void GetCdDriveIndex()
        {
            var audioCdService = new AudioCdService();
            var driveIndex = audioCdService.GetCdDriveIndex(AudioCdTestConstants.CdDriveLetter);

            var driveIndexIsCorrect = driveIndex == AudioCdTestConstants.CdDriveIndex;

            Assert.True(driveIndexIsCorrect, "Failed to get correct drive index for drive letter");
        }

        [Fact]
        public void IsAudioCd()
        {
            var audioCdService = new AudioCdService();
            var isAudioCd = audioCdService.IsAudioCd(AudioCdTestConstants.CdDriveLetter);

            Assert.True(isAudioCd, string.Format("Failed to identify CD drive as an Audio CD ({0})", AudioCdTestConstants.CheckDriveMessage));
        }

        [Fact]
        public void GetTrackNumberFromCdaFilename()
        {
            var audioCdService = new AudioCdService();
            var trackNumber = audioCdService.GetTrackNumberFromCdaFilename("track01.cda");

            var trackNumberIsCorrect = trackNumber == 0;

            Assert.True(trackNumberIsCorrect, "Failed to get correct track number");
        }
    }
}
