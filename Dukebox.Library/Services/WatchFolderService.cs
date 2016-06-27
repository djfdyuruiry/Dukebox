﻿using System;
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

        private FileSystemWatcher _fileWatcher;

        public event EventHandler FileEventProcessed;

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

            _audioFormats = audioFormats;
            _importService = importService;
            _updateService = updateService;
            _eventService = eventService;

            _initalImportTask = AddFileChangesSinceLastStart();

            _eventService.WatchFolderUpdated += (o, e) => ReloadServiceIfStarted(e);
        }

        private async Task AddFileChangesSinceLastStart()
        {
            await _importService.AddSupportedFilesInDirectory(WatchFolder.FolderPath, true, 
                null, (o, i) => logger.Info($"Inital import for folder '{WatchFolder.FolderPath}' has completed"));
        }

        private void ReloadServiceIfStarted(WatchFolder watchFolder)
        {
            if (watchFolder != WatchFolder || _fileWatcher == null)
            {
                return;
            }

            StopWatching();

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
            }, e.FullPath);
        }

        private void ProcessChangedFileEvent(FileSystemEventArgs e)
        {
            SafeFileActionInvoke(() =>
            {
                _updateService.RemoveSongByFilePath(e.FullPath);
                var song = _importService.AddFile(e.FullPath);

                _eventService.TriggerSongUpdated(song);
            }, e.FullPath);
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
            }, e.FullPath, false);
        }

        private void ProcessDeletedFileEvent(FileSystemEventArgs e)
        {
            SafeFileActionInvoke(() =>
            {
                _updateService.RemoveSongByFilePath(e.FullPath);
            }, e.FullPath);
        }
        
        private void SafeFileActionInvoke(Action actionToInvoke, string filePath)
        {
            SafeFileActionInvoke(actionToInvoke, filePath, true);
        }

        private void SafeFileActionInvoke(Action actionToInvoke, string filePath, bool doFileTypeCheck)
        {

            try
            {
                if (doFileTypeCheck && !_audioFormats.FileSupported(filePath))
                {
                    return;
                }

                actionToInvoke?.Invoke();

                Task.Run(() => FileEventProcessed?.Invoke(this, EventArgs.Empty));
            }
            catch (Exception ex)
            {
                logger.Error($"Error while processing event for file '{filePath}' from watch folder '{WatchFolder.FolderPath}'", ex);
            }
        }

        public void StopWatching()
        {
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