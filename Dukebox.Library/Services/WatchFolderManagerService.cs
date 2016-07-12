using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Dukebox.Audio;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Library.Services
{
    public class WatchFolderManagerService : IWatchFolderManagerService
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMusicLibraryImportService _importService;
        private readonly IMusicLibraryUpdateService _updateService;
        private readonly AudioFileFormats _audioFormats;
        private readonly IMusicLibraryEventService _eventService;

        public event EventHandler<WatchFolderEvent> WatchFolderServiceProcessedEvent;

        public List<IWatchFolderService> WatchFolders { get; private set; }
        public WatchFolder LastWatchFolderUpdated
        {
            get
            {
                return WatchFolders
                    .Select(wfs => wfs.WatchFolder)
                    .OrderByDescending(wf => wf.LastScanDateTime)
                    .FirstOrDefault();
            }
        }

        public WatchFolderManagerService(AudioFileFormats audioFormats, IMusicLibraryRepository musicRepo, IMusicLibraryImportService importService, 
            IMusicLibraryUpdateService updateService, IMusicLibraryEventService eventService)
        {
            _audioFormats = audioFormats;
            _importService = importService;
            _updateService = updateService;
            _eventService = eventService;

            // Import files only when we can validate if files are supported or not.
            audioFormats.FormatsLoaded += (o, e) =>
            {
                WatchFolders = musicRepo
                    .GetWatchFolders()
                    .Select(w => BuildWatchFolderService(w)).Cast<IWatchFolderService>().ToList();
                
                WatchFolders.ForEach(w => Task.Run(w.StartWatching));
            };

            _eventService.WatchFolderDeleted += (o, e) => RemoveWatchFolder(e);
        }

        private void RemoveWatchFolder(WatchFolder watchFolder)
        {
            var watchFolderToRemove = WatchFolders.FirstOrDefault(w => w.WatchFolder == watchFolder);

            if (watchFolderToRemove == null)
            {
                return;
            }

            watchFolderToRemove.StopWatching();
            WatchFolders.Remove(watchFolderToRemove);
        }

        public async Task ManageWatchFolder(WatchFolder watchFolder)
        {
            var dbWatchFolder = await _importService.AddWatchFolder(watchFolder);
            var watchFolderService = BuildWatchFolderService(dbWatchFolder);

            logger.Info($"Added folder '{dbWatchFolder.FolderPath}' to database");

            watchFolderService.FileEventProcessed += (o, e) => WatchFolderServiceProcessedEvent?.Invoke(this, new WatchFolderEvent
            {
                EventType = WatchFolderEventType.AudioFileImport,
                AudioFileImportInfo = e
            });
            watchFolderService.ImportCompleted += (o, e) => WatchFolderServiceProcessedEvent?.Invoke(this, new WatchFolderEvent
            {
                EventType = WatchFolderEventType.DirectoryImport,
                ImportReport = e
            });

            watchFolderService.StartWatching();

            WatchFolders.Add(watchFolderService);
        }

        private WatchFolderService BuildWatchFolderService(WatchFolder watchFolder)
        {
            return new WatchFolderService(watchFolder, _audioFormats, _importService, _updateService, _eventService);
        }

        public async Task StopManagingWatchFolder(WatchFolder watchFolder)
        {
            var watchFolderService = WatchFolders.FirstOrDefault(wfs => wfs.WatchFolder == watchFolder);

            if (watchFolderService == null)
            {
                return;
            }

            watchFolderService.StopWatching();

            WatchFolders.Remove(watchFolderService);
            await _updateService.RemoveWatchFolder(watchFolder);

            logger.Info($"Removed folder '{watchFolder.FolderPath}' from database");
        }
    }
}
