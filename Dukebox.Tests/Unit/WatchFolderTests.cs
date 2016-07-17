using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;
using Dukebox.Audio;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Services;

namespace Dukebox.Tests.Unit
{
    public class WatchFolderTests
    {
        private const string watchFolderDirectory = "watchFolderDir";
        public const string SampleMp3FileName = "sample.mp3";
        
        public WatchFolderTests()
        {
            try
            {
                Directory.Delete(watchFolderDirectory, true);
            }
            catch (Exception)
            {
                // do nothing
            }

            Directory.CreateDirectory(watchFolderDirectory);
        }

        [Fact]
        public void When_New_Folder_Service_Is_Created_Inital_Import_Should_Run()
        {
            var signalEvent = new ManualResetEvent(false);
            var watchFolderService = BuildWatchFolderService(false);
            var importEventFired = false;

            watchFolderService.ImportCompleted += (o, e) =>
            {
                importEventFired = true;
                signalEvent.Set();
            };

            signalEvent.WaitOne(2000);

            Assert.True(importEventFired, "Watch folder service failed to do an inital import when created");
        }

        [Fact]
        public void When_Monitoring_Folder_File_Creation_Should_Be_Detected()
        {
            var fileEventFired = TestWatchFolderFileEvent(() => File.Copy(SampleMp3FileName, Path.Combine(watchFolderDirectory, SampleMp3FileName)));

            Assert.True(fileEventFired, "Watch folder service failed to fire event when file was created");
        }

        [Fact]
        public void When_Monitoring_Folder_File_Deletion_Should_Be_Detected()
        {
            var newFilePath = Path.Combine(watchFolderDirectory, SampleMp3FileName);

            File.Copy(SampleMp3FileName, newFilePath);

            var fileEventFired = TestWatchFolderFileEvent(() => File.Delete(newFilePath));

            Assert.True(fileEventFired, "Watch folder service failed to fire event when file was deleted");
        }

        [Fact]
        public void When_Monitoring_Folder_File_Renaming_Should_Be_Detected()
        {
            var newFilePath = Path.Combine(watchFolderDirectory, SampleMp3FileName);
            var renamedNewFilePath = Path.Combine(watchFolderDirectory, "renamed.mp3");

            File.Copy(SampleMp3FileName, newFilePath);

            var fileEventFired = TestWatchFolderFileEvent(() => File.Move(newFilePath, renamedNewFilePath));

            Assert.True(fileEventFired, "Watch folder service failed to fire event when file was renamed");
        }

        private bool TestWatchFolderFileEvent(Action fileOperation)
        {
            var watchFolderService = BuildWatchFolderService();
            watchFolderService.StartWatching();

            var signalEvent = new ManualResetEvent(false);
            var fileEventFired = false;

            watchFolderService.FileEventProcessed += (o, e) =>
            {
                fileEventFired = true;
                signalEvent.Set();
            };

            fileOperation?.Invoke();
            
            signalEvent.WaitOne(500);

            watchFolderService.StopWatching();

            return fileEventFired;
        }

        private IWatchFolderService BuildWatchFolderService(bool skipInitalImport = true)
        {
            var audioFormats = new AudioFileFormats();
            var importService = A.Fake<IMusicLibraryImportService>();
            var updateService = A.Fake<IMusicLibraryUpdateService>();
            var eventService = A.Fake<IMusicLibraryEventService>();

            audioFormats.SupportedFormats.Add(".mp3");
            audioFormats.SignalFormatsHaveBeenLoaded();

            A.CallTo(importService).Where(c => c.Method.Name == "AddSupportedFilesInDirectory").WithReturnType<Task>().ReturnsLazily(async e =>
            {
                var completeAction = e.Arguments[3] as Action<DirectoryImportReport>;

                Thread.Sleep(250);

                completeAction?.Invoke(new DirectoryImportReport());

                await Task.CompletedTask;
            });

            return new WatchFolderService(new WatchFolder { FolderPath = watchFolderDirectory },
                audioFormats, importService, updateService, eventService, skipInitalImport);
        }
    }
}
