using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using FakeItEasy;
using Xunit;
using Dukebox.Library.Factories;
using Dukebox.Library.Interfaces;
using Dukebox.Audio.Interfaces;

namespace Dukebox.Tests.Unit
{
    public class AudioFileMetaDataTests
    {
        public const string SampleMp3FileName = "sample.mp3";
        public const string SampleMidiFileName = "artist - title.mid";
        public const string SampleWithEmptyTagMp3FileName = "artist - title.mp3";
        public const string SampleWithNoTagMp3FileName = "artist1 - title1.mp3";
        public const string SampleWithNoAlbumArtMp3FileName = "sampleWithoutAlbumArt.mp3";
        private const string sampleForEditingMp3FileName = "sample_for_editing.mp3";
        private readonly AudioFileMetadataFactory _audioFileMetadataFactory;

        public AudioFileMetaDataTests()
        {
            _audioFileMetadataFactory = new AudioFileMetadataFactory(A.Fake<ICdMetadataService>(), A.Fake<IAudioCdService>());
        }

        [Fact]
        public void When_File_Has_Tag_Metadata_Should_Be_Extracted()
        {
            var audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(SampleMp3FileName);
            var metadataWasBuiltCorrectly = audioFileMetadata.HasFutherMetadataTag;

            Assert.True(metadataWasBuiltCorrectly, "Metdata was not extracted correctly from audio metadata tag");
        }

        [Fact]
        public void When_File_Tag_Has_AlbumArt_Correct_Metadata_Should_Be_Extracted()
        {
            VerifyNonEmptyTagMetadata(SampleMp3FileName, "Extracted metadata for tag with album art was incorrect");
        }

        [Fact]
        public void When_File_Tag_Has_NoAlbumArt_Correct_Metadata_Should_Be_Extracted()
        {
            VerifyNonEmptyTagMetadata(SampleWithNoAlbumArtMp3FileName, "Extracted metadata for tag with no album art was incorrect");
        }

        private void VerifyNonEmptyTagMetadata(string sampleFileName, string failureMessage)
        {
            var audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(sampleFileName);
            var extendedMetadata = audioFileMetadata.ExtendedMetadata;

            var albumIsCorrect = !string.IsNullOrEmpty(audioFileMetadata.Album) && (audioFileMetadata.Album == "sample album");
            var artistIsCorrect = !string.IsNullOrEmpty(audioFileMetadata.Artist) && (audioFileMetadata.Artist == "sample artist");
            var audioLengthIsCorrect = audioFileMetadata.Length == 153;
            var titleIsCorrect = !string.IsNullOrEmpty(audioFileMetadata.Title) && (audioFileMetadata.Title == "sample title");
            var extendedMetadataIsCorrect = extendedMetadata.Any() && string.Equals(extendedMetadata["Year"]?.First(), "2016", StringComparison.Ordinal);

            var extractedMetadataWasCorrect = albumIsCorrect && artistIsCorrect && audioLengthIsCorrect && titleIsCorrect && extendedMetadataIsCorrect;

            Assert.True(extractedMetadataWasCorrect, failureMessage);
        }

        [Fact]
        public void When_File_Tag_Is_Empty_Correct_Metadata_Should_Be_Extracted_By_Filename()
        {
            VerifyFileNameMetadata(SampleWithEmptyTagMp3FileName, "artist", "title", "Extracted metadata for file with empty tag was incorrect");
        }

        [Fact]
        public void When_File_Tag_Is_Missing_Correct_Metadata_Should_Be_Extracted_By_Filename()
        {
            VerifyFileNameMetadata(SampleWithNoTagMp3FileName, "artist1", "title1", "Extracted metadata for file with empty tag was incorrect");
        }

        [Fact]
        public void When_File_Type_Is_Not_Supported_By_Tags_Correct_Metadata_Should_Be_Extracted_By_Filename()
        {
            VerifyFileNameMetadata(SampleMidiFileName, "artist", "title", "Extracted metadata for file with empty tag was incorrect");
        }

        private void VerifyFileNameMetadata(string sampleFileName, string extectedArtist, string expectedTitle, string failureMessage)
        {
            var audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(sampleFileName);

            var albumIsCorrect = !string.IsNullOrEmpty(audioFileMetadata.Album) && (audioFileMetadata.Album == "Unknown Album");
            var artistIsCorrect = !string.IsNullOrEmpty(audioFileMetadata.Artist) && (audioFileMetadata.Artist == extectedArtist);
            var titleIsCorrect = !string.IsNullOrEmpty(audioFileMetadata.Title) && (audioFileMetadata.Title == expectedTitle);

            var extractedMetadataWasCorrect = albumIsCorrect && artistIsCorrect && titleIsCorrect;

            Assert.True(extractedMetadataWasCorrect, failureMessage);
        }

        [Fact]
        public void When_File_Tag_Has_AlbumArt_Correct_AlbumArt_Should_Be_Extracted()
        {
            var audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(SampleMp3FileName);

            var hasAlbumArt = audioFileMetadata.HasAlbumArt;

            Assert.True(hasAlbumArt, "Failed to extract album art from MP3 file");

            var albumArt = audioFileMetadata.GetAlbumArt();

            var imageLength = 0;
            var returnedImageLength = 0;

            using (var blankImage = new Bitmap(128, 128, PixelFormat.Format32bppRgb))
            {
                using (var memStream = new MemoryStream())
                {
                    blankImage.Save(memStream, ImageFormat.Bmp);
                    imageLength = memStream.ToArray().Length;
                }
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
        public void When_File_Metadata_Is_Saved_Correct_Details_Should_Be_Written_To_Tag()
        {
            File.Copy(SampleMp3FileName, sampleForEditingMp3FileName, true);
            var audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(sampleForEditingMp3FileName);

            var newTitle = "UnqiueTitle";
            var newArtist = "UniqueArtist";
            var newAlbum = "UnqiueAlbum";
            var newYear = "1999";

            audioFileMetadata.Title = newTitle;
            audioFileMetadata.Artist = newArtist;
            audioFileMetadata.Album = newAlbum;
            audioFileMetadata.ExtendedMetadata["Year"] = new List<string> { newYear };

            audioFileMetadata.SaveMetadataToFileTag();

            audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(sampleForEditingMp3FileName);

            var metadataCorrect = audioFileMetadata.Title == newTitle && audioFileMetadata.Artist == newArtist
                && audioFileMetadata.Album == newAlbum && audioFileMetadata.ExtendedMetadata["Year"].First().Equals(newYear);

            Assert.True(metadataCorrect, "Failed to save and retrieve correct audio file metadata");
        }
    }
}
