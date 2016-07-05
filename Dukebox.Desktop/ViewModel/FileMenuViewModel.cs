
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Application = System.Windows.Application;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Audio;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library.Factories;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Desktop.ViewModel
{
    public class FileMenuViewModel : ViewModelBase, IFileMenuViewModel, IDisposable
    {
        public const string FolderBrowserPrompt = "Select folder to load music files from";
        public const string AddToLibraryHeader = "Importing audio files into the library...";
        public const string AddToLibraryTitle = "Library Import";

        private readonly IMusicLibraryImportService _musicLibraryImportService;
        private readonly IWatchFolderManagerService _watchFolderService;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly TrackFactory _trackFactory;
        private readonly ITrackGeneratorService _tracksGenerator;

        private readonly OpenFileDialog _selectFileDialog;
        private readonly FolderBrowserDialog _selectFolderDialog;

        public ICommand PlayFile { get; private set; }
        public ICommand PlayFolder { get; private set; }
        public ICommand AddFilesToLibrary { get; private set; }
        public ICommand ExportLibrary { get; private set; }
        public ICommand ImportLibrary { get; private set; }
        public ICommand Exit { get; private set; }

        public FileMenuViewModel(IMusicLibraryImportService musicLibraryImportService, IWatchFolderManagerService watchFolderService,
            ITrackGeneratorService tracksGenerator, IAudioPlaylist audioPlaylist, 
            AudioFileFormats audioFileFormats, TrackFactory trackFactory) : base()
        {
            _musicLibraryImportService = musicLibraryImportService;
            _watchFolderService = watchFolderService;
            _audioPlaylist = audioPlaylist;
            _trackFactory = trackFactory;
            _tracksGenerator = tracksGenerator;

            _selectFileDialog = new OpenFileDialog();
            _selectFolderDialog = new FolderBrowserDialog();

            audioFileFormats.FormatsLoaded += (o, e) => _selectFileDialog.Filter = audioFileFormats.FileDialogFilter;
            _selectFolderDialog.Description = FolderBrowserPrompt;

            PlayFile = new RelayCommand(DoPlayFile);
            PlayFolder = new RelayCommand(DoPlayFolder);
            AddFilesToLibrary = new RelayCommand(DoAddFilesToLibrary);

            // todo: add import/export library routines

            Exit = new RelayCommand(() => Application.Current.Shutdown());
        }

        private void DoPlayFile()
        {
            var dialogResult = _selectFileDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            var fileName = _selectFileDialog.FileName;
            var track = _trackFactory.BuildTrackInstance(fileName);

            _audioPlaylist.LoadPlaylistFromList(new List<ITrack> { track });

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void DoPlayFolder()
        {
            var dialogResult = _selectFolderDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }
            
            var tracks = _tracksGenerator.GetTracksForDirectory(_selectFolderDialog.SelectedPath, false);
            _audioPlaylist.LoadPlaylistFromList(tracks);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void DoAddFilesToLibrary()
        {
            var dialogResult = _selectFolderDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            var pathToAdd = _selectFolderDialog.SelectedPath;

            try
            {
                _watchFolderService.ManageWatchFolder(new WatchFolder { FolderPath = pathToAdd });
            }
            catch (Exception ex)
            {
                var errMsg = string.Format("Unable to add directory '{0}': {1}", pathToAdd, ex.Message);
                System.Windows.MessageBox.Show(errMsg, "Error Adding Directory", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        
        protected virtual void Dispose(bool cleanAllResources)
        {
            if (cleanAllResources)
            {
                _selectFileDialog.Dispose();
                _selectFolderDialog.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
