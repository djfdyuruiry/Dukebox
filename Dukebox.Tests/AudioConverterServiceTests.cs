using Dukebox.Audio.Services;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Dukebox.Tests
{
    public class AudioConverterServiceTests
    {
        public AudioConverterServiceTests()
        {
            BassLoader.LoadBass();
        }

        [Fact]
        public void WriteCdaFileToWavFile()
        {
            var audioConverter = new AudioConverterService();
            var cdaFile = string.Format(@"{0}:\track01.cda", AudioCdTestConstants.CdDriveLetter);
            var wavFile = "track01.wav";

            audioConverter.WriteCdaFileToWavFile(cdaFile, wavFile, ProgressCallback);

            var wavFileExists = File.Exists(wavFile);

            Assert.True(wavFileExists, string.Format("Failed to generate wav file ({0})", AudioCdTestConstants.CheckDriveMessage));

            var wavFileInfo = new FileInfo(wavFile);
            var wavFileSize = wavFileInfo.Length;
            var wavFileIsNotEmpty = wavFileSize > 0;

            File.Delete(wavFile);

            Assert.True(wavFileIsNotEmpty, "Generated wav file was empty");
        }

        [Fact]
        public async void ConvertCdaFileToMp3()
        {

            var audioConverter = new AudioConverterService();
            var cdaFile = string.Format(@"{0}:\track01.cda", AudioCdTestConstants.CdDriveLetter);
            var mp3File = Path.Combine(Directory.GetCurrentDirectory(), "track01.mp3");

            await audioConverter.ConvertCdaFileToMp3(cdaFile, mp3File, ProgressCallback, true);

            var mp3FileExists = File.Exists(mp3File);

            Assert.True(mp3FileExists, string.Format("Failed to generate mp3 file ({0})", AudioCdTestConstants.CheckDriveMessage));

            var mp3FileInfo = new FileInfo(mp3File);
            var mp3FileSize = mp3FileInfo.Length;
            var mp3FileIsNotEmpty = mp3FileSize > 0;

            Assert.True(mp3FileIsNotEmpty, "Generated mp3 file was empty");
        }

        private void ProgressCallback(long bytesTotal, long bytesDone)
        {
            Debug.WriteLine("{0}/{1}", bytesDone, bytesTotal);
        }
    }
}
