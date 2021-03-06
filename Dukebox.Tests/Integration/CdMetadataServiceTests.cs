﻿using System.IO;
using System.Linq;
using FakeItEasy;
using Xunit;
using Dukebox.Audio.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using Dukebox.Tests.Utils;

namespace Dukebox.Tests.Integration
{
    public class CdMetadataServiceTests
    {
        private readonly ICdMetadataService _cdMetadataService;

        public CdMetadataServiceTests()
        {
            AudioLibraryLoader.LoadBass();

            var audioCdService = A.Fake<IAudioCdService>();
            A.CallTo(() => audioCdService.GetCdDriveIndex(A<char>.Ignored)).Returns(AudioCdTestConstants.CdDriveIndex);

            _cdMetadataService = new CdMetadataService(audioCdService);
        }

        [Fact]
        public void GetMetadataForCd()
        {
            var metadata = _cdMetadataService.GetMetadataForCd(AudioCdTestConstants.CdDriveLetter);
            
            var noAlbumReturned = string.IsNullOrEmpty(metadata.Album);

            Assert.False(noAlbumReturned, string.Format("No album name was returned in the metadata ({0})", AudioCdTestConstants.CheckDriveMessage));

            var noArtistReturned = string.IsNullOrEmpty(metadata.Artist);

            Assert.False(noArtistReturned, "No artist name was returned in the metadata");

            var audioCdTrackCount = Directory.EnumerateFiles(AudioCdTestConstants.CdDrivePath).Count();
            var metadataTrackCount = metadata.Tracks.Count;
            var trackCountCorrect = audioCdTrackCount == metadataTrackCount;

            Assert.True(trackCountCorrect, "Metadata was not returned for all tracks on the Audio CD");
        }

        [Fact]
        public void GetAudioFileMetaDataForCd()
        {
            var metadata = _cdMetadataService.GetAudioFileMetaDataForCd(AudioCdTestConstants.CdDriveLetter);

            var metadataIsEmpty = metadata.Any();

            Assert.False(metadataIsEmpty, string.Format("No metadata was found for the Audio CD ({0})", AudioCdTestConstants.CheckDriveMessage));

            var metadataCount = metadata.Count;
            var audioCdTrackCount = Directory.EnumerateFiles(AudioCdTestConstants.CdDrivePath).Count();
            var trackCountCorrect = metadataCount == audioCdTrackCount;

            Assert.True(trackCountCorrect, "Metadata was not returned for all tracks on the Audio CD");
        }
    }
}
