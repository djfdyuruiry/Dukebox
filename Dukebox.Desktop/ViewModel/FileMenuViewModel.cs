using System;
using System.Collections.Generic;
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
using System.Linq;

namespace Dukebox.Desktop.ViewModel
{
    public class FileMenuViewModel : ViewModelBase, IFileMenuViewModel, IDisposable
    {
        public const string FolderBrowserPrompt = "Select folder to load music files from";
        public const string AddToLibraryHeader = "Importing audio files into the library...";
        public const string AddToLibraryTitle = "Library Import";
        public const string DbFolderBrowserPrompt = "Select folder to save the music library export in";
        public const string DbFileFilter = "Music Library (*.s3db)|*.s3db";

        private readonly IMusicLibraryImportService _musicLibraryImportService;
        private readonly IWatchFolderManagerService _watchFolderService;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly TrackFactory _trackFactory;
        private readonly IMusicLibraryDbContextFactory _dbContextFactory;
        private readonly ITrackGeneratorService _tracksGenerator;

        private readonly OpenFileDialog _selectAudioFileDialog;
        private readonly FolderBrowserDialog _selectMusicFolderDialog;
        private readonly OpenFileDialog _selectDbFileDialog;
        private readonly SaveFileDialog _saveDbFileDialog;

        public ICommand PlayFile { get; private set; }
        public ICommand PlayFolder { get; private set; }
        public ICommand AddFilesToLibrary { get; private set; }
        public ICommand ExportLibrary { get; private set; }
        public ICommand ImportLibrary { get; private set; }
        public ICommand Exit { get; private set; }

        public FileMenuViewModel(IMusicLibraryImportService musicLibraryImportService, IWatchFolderManagerService watchFolderService,
            ITrackGeneratorService tracksGenerator, IAudioPlaylist audioPlaylist, 
            AudioFileFormats audioFileFormats, TrackFactory trackFactory, 
            IMusicLibraryDbContextFactory dbContextFactory) : base()
        {
            _musicLibraryImportService = musicLibraryImportService;
            _watchFolderService = watchFolderService;
            _audioPlaylist = audioPlaylist;
            _trackFactory = trackFactory;
            _dbContextFactory = dbContextFactory;
            _tracksGenerator = tracksGenerator;

            _selectAudioFileDialog = new OpenFileDialog();
            _selectMusicFolderDialog = new FolderBrowserDialog();

            audioFileFormats.FormatsLoaded += (o, e) => _selectAudioFileDialog.Filter = audioFileFormats.FileDialogFilter;
            _selectMusicFolderDialog.Description = FolderBrowserPrompt;

            _selectDbFileDialog = new OpenFileDialog();
            _saveDbFileDialog = new SaveFileDialog();

            _selectDbFileDialog.Filter = DbFileFilter;
            _saveDbFileDialog.Filter = DbFileFilter;

            PlayFile = new RelayCommand(DoPlayFile);
            PlayFolder = new RelayCommand(DoPlayFolder);
            AddFilesToLibrary = new RelayCommand(DoAddFilesToLibrary);
            ExportLibrary = new RelayCommand(DoExportLibrary);
            ImportLibrary = new RelayCommand(DoImportLibrary);

            Exit = new RelayCommand(() => Application.Current.Shutdown());
        }

        private void DoPlayFile()
        {
            var dialogResult = _selectAudioFileDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            var fileName = _selectAudioFileDialog.FileName;
            var track = _trackFactory.BuildTrackInstance(fileName);

            _audioPlaylist.LoadPlaylistFromList(new List<string> { track.Song.FileName });

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void DoPlayFolder()
        {
            var dialogResult = _selectMusicFolderDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }
            
            var tracks = _tracksGenerator.GetTracksForDirectory(_selectMusicFolderDialog.SelectedPath, false);
            _audioPlaylist.LoadPlaylistFromList(tracks.Select(t => t.Song.FileName).ToList());

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void DoAddFilesToLibrary()
        {
            var dialogResult = _selectMusicFolderDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            var pathToAdd = _selectMusicFolderDialog.SelectedPath;

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

        private void DoExportLibrary()
        {
            var dialogResult = _saveDbFileDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            var pathToExportTo = _saveDbFileDialog.FileName;

            try
            {
                _dbContextFactory.ExportCurrentLibraryFile(pathToExportTo);
            }
            catch (Exception ex)
            {
                var errMsg = $"Unable export library to file '{pathToExportTo}': {ex.Message}";
                System.Windows.MessageBox.Show(errMsg, "Error Exporting Library",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void DoImportLibrary()
        {
            var dialogResult = _selectDbFileDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            var libraryFileToImport = _selectDbFileDialog.FileName;

            try
            {
                var backupPath = _dbContextFactory.ImportLibraryFile(libraryFileToImport);

                System.Windows.MessageBox.Show(
                    $"Dukebox will now restart to complete the library import. A backup of your old library has been saved at '{backupPath}'.", "Music Library Import",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                var errMsg = $"Unable importing library from file '{libraryFileToImport}': {ex.Message}";
                System.Windows.MessageBox.Show(errMsg, "Error Importing Library",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        protected virtual void Dispose(bool cleanAllResources)
        {
            if (cleanAllResources)
            {
                _selectAudioFileDialog.Dispose();
                _selectMusicFolderDialog.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
