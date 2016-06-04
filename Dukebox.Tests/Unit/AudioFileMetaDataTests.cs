using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Xunit;
using Dukebox.Library.Services;

namespace Dukebox.Tests.Unit
{
    public class AudioFileMetaDataTests
    {
        private const string sampleMp3FileName = "sample.mp3";

        [Fact]
        public void BuildAudioFileMetdata()
        {
            var audioFileMetadata = AudioFileMetadata.BuildAudioFileMetaData(sampleMp3FileName);
            var metadataWasBuiltCorrectly = audioFileMetadata.HasFutherMetadataTag;

            Assert.True(metadataWasBuiltCorrectly, "Metdata was not extracted correctly from audio metadata tag");
        }

        [Fact]
        public void AlbumArtTest()
        {
            var blankImage = new Bitmap(128, 128, PixelFormat.Format32bppRgb);
            var audioFileMetadata = AudioFileMetadata.BuildAudioFileMetaData(sampleMp3FileName);

            var hasAlbumArt = audioFileMetadata.HasAlbumArt;

            Assert.True(hasAlbumArt, "Failed to extract album art from MP3 file");

            var albumArt = audioFileMetadata.AlbumArt;

            var imageLength = 0;
            var returnedImageLength = 0;

            using (var memStream = new MemoryStream())
            {
                blankImage.Save(memStream, ImageFormat.Bmp);
                imageLength = memStream.ToArray().Length;
            }

            using (var memStream = new MemoryStream())
            {
                albumArt.Save(memStream, ImageFormat.Bmp);
                returnedImageLength = memStream.ToArray().Length;
            }

            var imageSizeIsEqual = imageLength == returnedImageLength;

            Assert.True(imageSizeIsEqual, "Image extracted was not the same size as image stored in audio metdata tag");
        }

        [Fact]
        public void Album()
        {
            var audioFileMetadata = AudioFileMetadata.BuildAudioFileMetaData(sampleMp3FileName);
            var album = audioFileMetadata.Album;

            var albumIsCorrect = !string.IsNullOrEmpty(album) && (album == "sample album");

            Assert.True(albumIsCorrect, "Album name extracted was incorrect");
        }

        [Fact]
        public void Artist()
        {
            var audioFileMetadata = AudioFileMetadata.BuildAudioFileMetaData(sampleMp3FileName);
            var artist = audioFileMetadata.Artist;

            var artistIsCorrect = !string.IsNullOrEmpty(artist) && (artist == "sample artist");

            Assert.True(artistIsCorrect, "Artist name extracted was incorrect");
        }

        [Fact]
        public void Length()
        {
            var audioFileMetadata = AudioFileMetadata.BuildAudioFileMetaData(sampleMp3FileName);

            var audioLength = audioFileMetadata.Length;
            var audioLengthIsCorrect = audioLength == 153;

            Assert.True(audioLengthIsCorrect, "Audio playback length extract was incorrect");
        }

        [Fact]
        public void Title()
        {
            var audioFileMetadata = AudioFileMetadata.BuildAudioFileMetaData(sampleMp3FileName);
            var title = audioFileMetadata.Title;

            var titleIsCorrect = !string.IsNullOrEmpty(title) && (title == "sample title");

            Assert.True(titleIsCorrect, "Title name extracted was incorrect");
        }
    }
}
