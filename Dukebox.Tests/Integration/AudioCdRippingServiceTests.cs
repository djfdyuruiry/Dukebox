using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;
using Dukebox.Audio.Services;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using Dukebox.Tests.Utils;
using Dukebox.Configuration.Interfaces;
using Dukebox.Library.Factories;

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
        public async Task RipAudioCd()
        {
            var audioCdService = new AudioCdService();
            var metadataService = new CdMetadataService(audioCdService);
            var audioConverterService = new AudioConverterService();
            var audioFileMetadataFactory = new AudioFileMetadataFactory(metadataService, audioCdService);

            var settings = A.Fake<IDukeboxSettings>();
            A.CallTo(() => settings.TrackDisplayFormat).Returns("{artist} - {title}");

            var trackFactory = new TrackFactory(settings, audioFileMetadataFactory);
            var audioRipper = new AudioCdRippingService(metadataService, audioConverterService, trackFactory, audioFileMetadataFactory);
            var viewUpdater = A.Fake<ICdRipViewUpdater>();

            await audioRipper.RipCdToFolder(AudioCdTestConstants.CdDrivePath, _audioDir, viewUpdater);

            var outputFiles = Directory.EnumerateFiles(_audioDir).ToList();

            var audioCdTrackCount = Directory.EnumerateFiles(AudioCdTestConstants.CdDrivePath).Count();
            var outputTrackCount = outputFiles.Count;
            var allTracksPresentInOutput = audioCdTrackCount == outputTrackCount;

            var metadataForFiles = outputFiles.Select(f => audioFileMetadataFactory.BuildAudioFileMetadataInstance(f)).ToList();
            var validMetadataSavedToDisk = metadataForFiles.All(m => m.Artist != "Unknown Artist" && m.Album != "Unknown Album" && m.Title != "Unknown Title");

            Assert.True(allTracksPresentInOutput, string.Format("Failed to rip all Tracks from the audio CD ({0})", AudioCdTestConstants.CheckDriveMessage));
            Assert.True(validMetadataSavedToDisk, string.Format("Failed to write valid metadata to disk for all Tracks from the audio CD ({0})", AudioCdTestConstants.CheckDriveMessage));
        }
    }
}
