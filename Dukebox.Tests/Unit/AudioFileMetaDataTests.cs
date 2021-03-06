﻿using System;
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
        private const string sampleForEditingMp3FileName = "sample_for_editing.mp3";
        private readonly AudioFileMetadataFactory _audioFileMetadataFactory;

        public AudioFileMetaDataTests()
        {
            _audioFileMetadataFactory = new AudioFileMetadataFactory(A.Fake<ICdMetadataService>(), A.Fake<IAudioCdService>());
        }

        [Fact]
        public void BuildAudioFileMetdata()
        {
            var audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(SampleMp3FileName);
            var metadataWasBuiltCorrectly = audioFileMetadata.HasFutherMetadataTag;

            Assert.True(metadataWasBuiltCorrectly, "Metdata was not extracted correctly from audio metadata tag");
        }

        [Fact]
        public void AlbumArtTest()
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
        public void Album()
        {
            var audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(SampleMp3FileName);
            var album = audioFileMetadata.Album;

            var albumIsCorrect = !string.IsNullOrEmpty(album) && (album == "sample album");

            Assert.True(albumIsCorrect, "Album name extracted was incorrect");
        }

        [Fact]
        public void Artist()
        {
            var audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(SampleMp3FileName);
            var artist = audioFileMetadata.Artist;

            var artistIsCorrect = !string.IsNullOrEmpty(artist) && (artist == "sample artist");

            Assert.True(artistIsCorrect, "Artist name extracted was incorrect");
        }

        [Fact]
        public void Length()
        {
            var audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(SampleMp3FileName);

            var audioLength = audioFileMetadata.Length;
            var audioLengthIsCorrect = audioLength == 153;

            Assert.True(audioLengthIsCorrect, "Audio playback length extract was incorrect");
        }

        [Fact]
        public void Title()
        {
            var audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(SampleMp3FileName);
            var title = audioFileMetadata.Title;

            var titleIsCorrect = !string.IsNullOrEmpty(title) && (title == "sample title");

            Assert.True(titleIsCorrect, "Title name extracted was incorrect");
        }

        [Fact]
        public void ExtendedMetadata()
        {
            var audioFileMetadata = _audioFileMetadataFactory.BuildAudioFileMetadataInstance(SampleMp3FileName);
            var extendedMetadata = audioFileMetadata.ExtendedMetadata;

            var extendedMetadataIsCorrect = extendedMetadata.Any() && string.Equals(extendedMetadata["Year"]?.First(), "2016", StringComparison.Ordinal);

            Assert.True(extendedMetadataIsCorrect, "Extended metadata extracted was incorrect");
        }

        [Fact]
        public void SaveMetadataToFileTag()
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
