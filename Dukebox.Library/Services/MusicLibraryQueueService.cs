using System;
using Timer = System.Timers.Timer;
using System.Threading;
using Dukebox.Library.Interfaces;

namespace Dukebox.Library.Services
{
    public class MusicLibraryQueueService : IMusicLibraryQueueService
    {
        private const long librarySaveChangesTimeout = 250;

        private readonly IMusicLibrary _musicLibrary;
        private readonly SemaphoreSlim _timerMutex;

        private Timer _librarySaveChangesTimer;

        public event EventHandler DatabaseChangesSaved; 

        public MusicLibraryQueueService(IMusicLibrary musicLibrary)
        {
            _musicLibrary = musicLibrary;

            _timerMutex = new SemaphoreSlim(1, 1);

            _musicLibrary.DatabaseChangesSaved += (o, e) => DatabaseChangesSaved?.Invoke(this, EventArgs.Empty);
        }

        public void QueueMusicLibrarySaveChanges()
        {
            try
            {
                _timerMutex.Wait();

                if (_librarySaveChangesTimer != null)
                {
                    return;
                }

                _librarySaveChangesTimer = new Timer(librarySaveChangesTimeout)
                {
                    AutoReset = false
                };

                _librarySaveChangesTimer.Elapsed += (o, e) => DoLibrarySaveChanges();

                _librarySaveChangesTimer.Start();
            }
            finally
            {
                _timerMutex.Release();
            }
        }

        private async void DoLibrarySaveChanges()
        {
            try
            {
                await _timerMutex.WaitAsync();
                _librarySaveChangesTimer = null;
            }
            finally
            {
                _timerMutex.Release();
            }

            await _musicLibrary.SaveDbChanges(true);
        }
    }
}
