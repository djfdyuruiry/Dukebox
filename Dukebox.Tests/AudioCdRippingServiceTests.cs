using Dukebox.Audio.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dukebox.Tests
{
    public class AudioCdRippingServiceTests
    {
        [Fact]
        public void RipAudioCd()
        {
            var musicLibrary = A.Fake<IMusicLibrary>();
            var metadataService = A.Fake<ICdMetadataService>();
            var audioConverterService = A.Fake<IAudioConverterService>();

            var audioRipper = new AudioCdRippingService(musicLibrary, metadataService, audioConverterService);

            // TODO: mock calls to dependencies
        }
    }
}
