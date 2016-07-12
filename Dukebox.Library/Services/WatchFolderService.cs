using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Dukebox.Audio;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services
{
    public class WatchFolderService : IWatchFolderService
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AudioFileFormats _audioFormats;
        private readonly IMusicLibraryImportService _importService;
        private readonly IMusicLibraryUpdateService _updateService;
        private readonly IMusicLibraryEventService _eventService;
        private Task _initalImportTask;

        private string _lastWatchFolderPath;
        private FileSystemWatcher _fileWatcher;

        public event EventHandler<DirectoryImportReport> ImportCompleted;
        public event EventHandler<AudioFileImportedInfo> FileEventProcessed;

        public WatchFolder WatchFolder { get; private set; }

        public WatchFolderService(WatchFolder watchFolder, AudioFileFormats audioFormats, IMusicLibraryImportService importService,
            IMusicLibraryUpdateService updateService, IMusicLibraryEventService eventService) 
            : this(watchFolder, audioFormats, importService, updateService, eventService, false)
        {
        }

        public WatchFolderService(WatchFolder watchFolder, AudioFileFormats audioFormats, IMusicLibraryImportService importService,
            IMusicLibraryUpdateService updateService, IMusicLibraryEventService eventService, bool skipInitalImport)
        {
            WatchFolder = watchFolder;

            _lastWatchFolderPath = watchFolder.FolderPath;

            _audioFormats = audioFormats;
            _importService = importService;
            _updateService = updateService;
            _eventService = eventService;

            _initalImportTask = skipInitalImport ? Task.CompletedTask : Task.Run(AddFileChangesSinceLastStart);

            _eventService.WatchFolderUpdated += (o, e) => ReloadServiceIfStarted(e);
        }

        private async Task AddFileChangesSinceLastStart()
        { 
            await _importService.AddSupportedFilesInDirectory(WatchFolder.FolderPath, true, 
                afi =>
                {
                    if (afi.JustProcessing)
                    {
                        Task.Run(() => FileEventProcessed?.Invoke(this, afi));
                    }
                },  
                dir =>
                {
                    Task.Run(() => ImportCompleted?.Invoke(this, dir));
                    logger.Info($"Inital import for folder '{WatchFolder.FolderPath}' has completed");

                    UpdateLastScannedDateTime();
                });
        }

        private void ReloadServiceIfStarted(WatchFolder watchFolder)
        {
            if (_fileWatcher == null || 
                watchFolder != WatchFolder ||
                watchFolder.FolderPath.Equals(_lastWatchFolderPath))
            {
                return;
            }

            StopWatching();

            _lastWatchFolderPath = WatchFolder.FolderPath;
            _initalImportTask = AddFileChangesSinceLastStart();

            StartWatching();
        }

        public async Task StartWatching()
        {
            if (!_initalImportTask.IsCompleted)
            {
                await _initalImportTask;
            }

            _fileWatcher = new FileSystemWatcher(WatchFolder.FolderPath);

            _fileWatcher.Changed += (o, e) => ProcessChangedFileEvent(e);
            _fileWatcher.Created += (o, e) => ProcessCreatedFileEvent(e);
            _fileWatcher.Deleted += (o, e) => ProcessDeletedFileEvent(e);
            _fileWatcher.Renamed += (o, e) => ProcessRenamedFileEvent(e);

            _fileWatcher.EnableRaisingEvents = true;

            logger.Info($"Started watching folder '{WatchFolder.FolderPath}' for events");
        }

        private void ProcessCreatedFileEvent(FileSystemEventArgs e)
        {
            SafeFileActionInvoke(() =>
            {
                _importService.AddFile(e.FullPath);
            }, e);
        }

        private void ProcessChangedFileEvent(FileSystemEventArgs e)
        {
            SafeFileActionInvoke(() =>
            {
                _updateService.RemoveSongByFilePath(e.FullPath);
                var song = _importService.AddFile(e.FullPath);

                _eventService.TriggerSongUpdated(song);
            }, e);
        }

        private void ProcessRenamedFileEvent(RenamedEventArgs e)
        {
            SafeFileActionInvoke(() =>
            {
                if (!_audioFormats.FileSupported(e.FullPath))
                {
                    _updateService.RemoveSongByFilePath(e.OldFullPath);
                    return;
                }

                var songWithFileNameExistedInDb = _updateService.UpdateSongFilePath(e.OldFullPath, e.FullPath).Result;

                if (!songWithFileNameExistedInDb)
                {
                    _importService.AddFile(e.FullPath);
                }                
            }, e, false);
        }

        private void ProcessDeletedFileEvent(FileSystemEventArgs e)
        {
            SafeFileActionInvoke(() =>
            {
                _updateService.RemoveSongByFilePath(e.FullPath);
            }, e);
        }
        
        private void SafeFileActionInvoke(Action actionToInvoke, FileSystemEventArgs eventArgs)
        {
            SafeFileActionInvoke(actionToInvoke, eventArgs, true);
        }

        private void SafeFileActionInvoke(Action actionToInvoke, FileSystemEventArgs eventArgs, bool doFileTypeCheck)
        {

            try
            {
                if (doFileTypeCheck && !_audioFormats.FileSupported(eventArgs.FullPath))
                {
                    return;
                }

                actionToInvoke?.Invoke();

                Task.Run(() => FileEventProcessed?.Invoke(this, new AudioFileImportedInfo
                {
                    FileAdded = eventArgs.FullPath,
                    TotalFilesThisImport = 1
                }));

                UpdateLastScannedDateTime();
            }
            catch (Exception ex)
            {
                logger.Error($"Error while processing event for file '{eventArgs.FullPath}' from watch folder '{WatchFolder.FolderPath}'", ex);
            }
        }

        private void UpdateLastScannedDateTime()
        {
            WatchFolder.LastScanDateTime = DateTime.UtcNow;
            _updateService.SaveWatchFolderChanges(WatchFolder);
        }

        public void StopWatching()
        {
            if (_fileWatcher == null)
            {
                return;
            }

            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher = null;

            logger.Info($"Stopped watching folder '{WatchFolder.FolderPath}' for events");
        }

        public void Dispose()
        {
            StopWatching();
        }
    }
}
