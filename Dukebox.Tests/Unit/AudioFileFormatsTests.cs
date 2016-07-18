using System.Collections.Generic;
using Xunit;
using Dukebox.Audio;

namespace Dukebox.Tests.Unit
{
    public class AudioFileFormatsTests
    {
        private readonly AudioFileFormats _audioFileFormats;
        
        public AudioFileFormatsTests()
        {
            _audioFileFormats = new AudioFileFormats();

            var formats = new List<string> { ".mp3", ".m4a", ".wma" };
            _audioFileFormats.SupportedFormats.AddRange(formats);
            
            _audioFileFormats.SignalFormatsHaveBeenLoaded();
        }

        [Fact]
        public void Audio_File_Formats_FileFilter_Should_Return_All_Formats()
        {
            var fileFilter = _audioFileFormats.FileFilter;

            var fileFilterIsCorrect = fileFilter.Equals("*.mp3;*.m4a;*.wma");

            Assert.True(fileFilterIsCorrect, "AudioFileFormats failed to define all formats in FileFilter");
        }

        [Fact]
        public void Audio_File_Formats_FileDialogFilter_Should_Return_All_Formats()
        {
            var fileDialogFilter = _audioFileFormats.FileDialogFilter;

            var fileDialogFilterIsCorrect = fileDialogFilter.Equals("Audio Files |*.mp3;*.m4a;*.wma");

            Assert.True(fileDialogFilterIsCorrect, "AudioFileFormats failed to define all formats in FileDialogFilter");
        }

        [Fact]
        public void Audio_File_Formats_IsFileSupported_Should_Return_True_For_Supported_FileType()
        {
            var fileSupported = _audioFileFormats.FileSupported("C:/*.mp3");

            Assert.True(fileSupported, "AudioFileFormats incorrectly signaled registered file format is unsupported");
        }

        [Fact]
        public void Audio_File_Formats_IsFileSupported_Should_Return_False_For_Unsupported_FileType()
        {
            var fileSupported = _audioFileFormats.FileSupported("C:/*.flac");

            Assert.False(fileSupported, "AudioFileFormats incorrectly signaled unregistered file format is supported");
        }
    }
}
