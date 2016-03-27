using Dukebox.Library;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using Dukebox.Library.Interfaces;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Xunit;
using Dukebox.Library.Model;

namespace Dukebox.Tests
{
    public class AlbumArtCacheServiceTests
    {
        private const string cachePath = "./cache";

        public AlbumArtCacheServiceTests()
        {
            try
            {
                Directory.Delete(cachePath);
            }
            catch (Exception ex)
            {
                // do nothing
            }
        }

        [Fact]
        public void BuildCachePath()
        {
            var settings = A.Fake<IDukeboxSettings>();
            A.CallTo(() => settings.AlbumArtCachePath).Returns(cachePath);

            Exception buildCachePathException = null;

            try
            {
                var albumArtCache = new AlbumArtCacheService(settings);
            }
            catch (Exception ex)
            {
                ex = buildCachePathException;
            }

            Assert.Null(buildCachePathException);
        }

        [Fact]
        public void AddSongToCache()
        {
            var settings = A.Fake<IDukeboxSettings>();
            A.CallTo(() => settings.AlbumArtCachePath).Returns(cachePath);

            var albumArtCache = new AlbumArtCacheService(settings);
            var albumId = 0;

            AddDummyImageToAlbumArtCache(albumArtCache, 0, albumId);

            var albumArtPath = Path.Combine(cachePath, albumId.ToString());
            var albumAdded = File.Exists(albumArtPath);

            Assert.True(albumAdded, "Album art was not successfully added to the cache");

            var albumArtFileInfo = new FileInfo(albumArtPath);
            var albumArtValid = albumArtFileInfo.Length > 0;

            Assert.True(albumAdded, "Album art image added to cache was empty");
        }

        [Fact]
        public void CheckCacheForAlbumArt()
        {
            var settings = A.Fake<IDukeboxSettings>();
            A.CallTo(() => settings.AlbumArtCachePath).Returns(cachePath);

            var albumArtCache = new AlbumArtCacheService(settings);
            var albumId = 0;

            AddDummyImageToAlbumArtCache(albumArtCache, 0, albumId);

            var existingAlbumArtFound = albumArtCache.CheckCacheForAlbum(albumId);

            Assert.True(existingAlbumArtFound, "Album art check returned false for album already in cache");

            var nonExistingAlbumArtFound = albumArtCache.CheckCacheForAlbum(-1);

            Assert.False(nonExistingAlbumArtFound, "Album art check returned true for album not in cache");
        }

        [Fact]
        public void GetAlbumArtFromCache()
        {
            var settings = A.Fake<IDukeboxSettings>();
            A.CallTo(() => settings.AlbumArtCachePath).Returns(cachePath);

            var albumArtCache = new AlbumArtCacheService(settings);
            var albumId = 0;

            var image = AddDummyImageToAlbumArtCache(albumArtCache, 0, albumId);

            var returnedImage = albumArtCache.GetAlbumArtFromCache(albumId);
            var imageLength = 0;
            var returnedImageLength = 0;

            using (var memStream = new MemoryStream())
            {
                image.Save(memStream, ImageFormat.Bmp);
                imageLength = memStream.ToArray().Length;
            }

            using (var memStream = new MemoryStream())
            {
                returnedImage.Save(memStream, ImageFormat.Bmp);
                returnedImageLength = memStream.ToArray().Length;
            }

            var imageSizeIsEqual = imageLength == returnedImageLength;

            Assert.True(imageSizeIsEqual, "Image returned from cache was different to image saved to cache");
        }

        [Fact]
        public void GetAlbumIdsFromCache()
        {
            var settings = A.Fake<IDukeboxSettings>();
            A.CallTo(() => settings.AlbumArtCachePath).Returns(cachePath);

            var albumArtCache = new AlbumArtCacheService(settings);
            var albumIds = new List<long> { 0, 2, 3, 5 };

            AddDummyImageToAlbumArtCache(albumArtCache, 0, albumIds[0]);
            AddDummyImageToAlbumArtCache(albumArtCache, 0, albumIds[1]);
            AddDummyImageToAlbumArtCache(albumArtCache, 0, albumIds[2]);
            AddDummyImageToAlbumArtCache(albumArtCache, 0, albumIds[3]);

            var idsInCache = albumArtCache.GetAlbumIdsFromCache();

            albumIds.Sort();
            idsInCache.Sort();

            var idsReturnedAreSameToAddedAlbums = idsInCache.SequenceEqual(albumIds);

            Assert.True(idsReturnedAreSameToAddedAlbums, "Album art IDs returned by cache are not equal to those added to the cache");
        }

        private Image AddDummyImageToAlbumArtCache(IAlbumArtCacheService albumArtCache, long songId, long albumId)
        {
            var songToAdd = new Song();
            var albumObj = new Album();
            var metadata = A.Fake<IAudioFileMetaData>();
            var blankImage = new Bitmap(128, 128, PixelFormat.Format32bppRgb);

            songToAdd.id = songId;
            albumObj.id = albumId;

            A.CallTo(() => metadata.HasAlbumArt).Returns(true);
            A.CallTo(() => metadata.HasFutherMetadataTag).Returns(true);
            A.CallTo(() => metadata.AlbumArt).Returns(blankImage);

            albumArtCache.AddSongToCache(songToAdd, metadata, albumObj);

            return blankImage;
        }
    }
}
