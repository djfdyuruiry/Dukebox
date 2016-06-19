using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;
using Dukebox.Library.Services;
using Dukebox.Library.Interfaces;

namespace Dukebox.Tests.Unit
{
    public class MusicLibraryQueueServiceTests
    {
        private readonly IMusicLibrary _musicLibrary;

        public MusicLibraryQueueServiceTests()
        {
            _musicLibrary = A.Fake<IMusicLibrary>();

            A.CallTo(() => _musicLibrary.SaveDbChanges(true)).ReturnsLazily(() =>
            {
                _musicLibrary.DatabaseChangesSaved += Raise.WithEmpty();
                return Task.FromResult(true);
            });
        }

        [Fact]
        public void QueueMusicLibrarySaveChanges()
        {
            var musicLibraryQueueService = new MusicLibraryQueueService(_musicLibrary);
            var signalEvent = new ManualResetEvent(false);
            var databaseChangesSavedCalled = false;
            var numDatabaseSaves = 0;

            musicLibraryQueueService.DatabaseChangesSaved += (o, e) =>
            {
                databaseChangesSavedCalled = true;
                numDatabaseSaves++;
                signalEvent.Set();
            };

            musicLibraryQueueService.QueueMusicLibrarySaveChanges();
            musicLibraryQueueService.QueueMusicLibrarySaveChanges();
            musicLibraryQueueService.QueueMusicLibrarySaveChanges();
            musicLibraryQueueService.QueueMusicLibrarySaveChanges();
            musicLibraryQueueService.QueueMusicLibrarySaveChanges();

            signalEvent.WaitOne(1000);

            var oneDatabaseSaveCallWasMade = numDatabaseSaves == 1;

            Assert.True(databaseChangesSavedCalled, "Database changes were not saved by queue service");
            Assert.True(oneDatabaseSaveCallWasMade, string.Format("Multiple ({0}) database saves  were incorrectly called by the queue service", numDatabaseSaves));
        }
    }
}
