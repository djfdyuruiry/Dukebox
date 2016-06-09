using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FakeItEasy;
using Xunit;
using Dukebox.Audio.Services;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using Dukebox.Tests.Utils;
using Dukebox.Configuration.Interfaces;

namespace Dukebox.Tests.Integration
{
    public class AudioCdRippingServiceTests
    {
        private readonly string _audioDir;

        public AudioCdRippingServiceTests()
        {
            _audioDir = string.Format(@".\{0}", "audioRipOut");

            AudioLibraryLoader.LoadBass();

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
            var musicLibrary = A.Fake<IMusicLibrary>();
            var settings = A.Fake<IDukeboxSettings>();

            var audioRipper = new AudioCdRippingService(metadataService, audioConverterService, settings);
            var viewUpdater = A.Fake<ICdRipViewUpdater>();

            await audioRipper.RipCdToFolder(AudioCdTestConstants.CdDrivePath, _audioDir, viewUpdater);

            var audioCdTrackCount = Directory.EnumerateFiles(AudioCdTestConstants.CdDrivePath).Count();
            var outputTrackCount = Directory.EnumerateFiles(_audioDir).Count();
            var allTracksPresentInOutput = audioCdTrackCount == outputTrackCount;

            Assert.True(allTracksPresentInOutput, string.Format("Failed to rip all Tracks from the audio CD ({0})", AudioCdTestConstants.CheckDriveMessage));
        }
    }
}
