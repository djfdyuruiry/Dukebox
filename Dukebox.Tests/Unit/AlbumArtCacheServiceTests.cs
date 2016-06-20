using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using FakeItEasy;
using Xunit;
using Dukebox.Library.Model;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Services;
using Dukebox.Configuration.Interfaces;

namespace Dukebox.Tests.Unit
{
    public class AlbumArtCacheServiceTests
    {
        private const string cachePath = "./cache";

        static AlbumArtCacheServiceTests()
        {
            try
            {
                Directory.Delete(cachePath, true);
            }
            catch (Exception)
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
                new AlbumArtCacheService(settings);
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
            var album = new Album("99");

            AddDummyImageToAlbumArtCache(albumArtCache, album);

            var albumArtPath = Path.Combine(cachePath, album.Id);
            var albumAdded = File.Exists(albumArtPath);

            Assert.True(albumAdded, "Album art was not successfully added to the cache");

            var albumArtFileInfo = new FileInfo(albumArtPath);
            var albumArtValid = albumArtFileInfo.Length > 0;

            Assert.True(albumArtValid, "Album art image added to cache was empty");
        }

        [Fact]
        public void CheckCacheForAlbumArt()
        {
            var settings = A.Fake<IDukeboxSettings>();
            A.CallTo(() => settings.AlbumArtCachePath).Returns(cachePath);

            var albumArtCache = new AlbumArtCacheService(settings);
            var album = new Album("66");

            AddDummyImageToAlbumArtCache(albumArtCache, album);

            var existingAlbumArtFound = albumArtCache.CheckCacheForAlbum(album.Id);

            Assert.True(existingAlbumArtFound, "Album art check returned false for album already in cache");

            var nonExistingAlbumArtFound = albumArtCache.CheckCacheForAlbum("-1");

            Assert.False(nonExistingAlbumArtFound, "Album art check returned true for album not in cache");
        }

        [Fact]
        public void GetAlbumArtFromCache()
        {
            var settings = A.Fake<IDukeboxSettings>();
            A.CallTo(() => settings.AlbumArtCachePath).Returns(cachePath);

            var albumArtCache = new AlbumArtCacheService(settings);
            var album = new Album("55");

            var image = AddDummyImageToAlbumArtCache(albumArtCache, album);

            var returnedImage = albumArtCache.GetAlbumArtFromCache(album.Id);
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
            var albums = new List<Album> { new Album("1"), new Album("22"), new Album("33"), new Album("44") };

            albums.ForEach(id => AddDummyImageToAlbumArtCache(albumArtCache, id));

            var albumIds = albums.Select(a => a.Id).ToList();
            var idsInCache = albumArtCache.GetAlbumIdsFromCache();

            albumIds.Sort();
            idsInCache.Sort();

            var idsReturnedAreSameToAddedAlbums = idsInCache.Intersect(albumIds).SequenceEqual(albumIds);

            Assert.True(idsReturnedAreSameToAddedAlbums, "Album art IDs returned by cache are not equal to those added to the cache");
        }

        private Image AddDummyImageToAlbumArtCache(IAlbumArtCacheService albumArtCache, Album album)
        {
            var metadata = A.Fake<IAudioFileMetadata>();
            var blankImage = new Bitmap(128, 128, PixelFormat.Format32bppRgb);

            A.CallTo(() => metadata.HasAlbumArt).Returns(true);
            A.CallTo(() => metadata.GetAlbumArt()).Returns(blankImage);
            A.CallTo(() => metadata.GetAlbumArt(A<Action<Image>>.Ignored)).WithAnyArguments().Invokes(e =>
            {
                var action = e.Arguments[0] as Action<Image>;
                action(blankImage);
            });

            File.Delete(Path.Combine(cachePath, album.Id));

            albumArtCache.AddAlbumToCache(album, metadata);

            return blankImage;
        }
    }
}
