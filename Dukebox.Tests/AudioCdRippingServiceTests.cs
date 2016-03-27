using Dukebox.Audio.Interfaces;
using Dukebox.Audio.Services;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using Dukebox.Library.Services;
using FakeItEasy;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Dukebox.Tests
{
    public class AudioCdRippingServiceTests
    {
        private readonly string _audioDir;

        public AudioCdRippingServiceTests()
        {
            _audioDir = string.Format(@".\{0}", "audioRipOut");

            BassLoader.LoadBass();

            try
            {
                Directory.Delete(_audioDir, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            Directory.CreateDirectory(_audioDir);
        }

        [Fact]
        public async void RipAudioCd()
        {
            var metadataService = new CdMetadataService(new AudioCdService());
            var audioConverterService = new AudioConverterService();

            var audioRipper = new AudioCdRippingService(metadataService, audioConverterService);
            var viewUpdater = A.Fake<ICdRipViewUpdater>();

            await audioRipper.RipCdToFolder(AudioCdTestConstants.CdDrivePath, _audioDir, viewUpdater);

            var audioCdTrackCount = Directory.EnumerateFiles(AudioCdTestConstants.CdDrivePath).Count();
            var outputTrackCount = Directory.EnumerateFiles(_audioDir).Count();
            var allTracksPresentInOutput = audioCdTrackCount == outputTrackCount;

            Assert.True(allTracksPresentInOutput, string.Format("Failed to rip all Tracks from the audio CD ({0})", AudioCdTestConstants.CheckDriveMessage));
        }
    }
}
