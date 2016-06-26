using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using Dukebox.Audio;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Library.Services;
using System.Threading.Tasks;
using Dukebox.Desktop.Views;
using System.IO;
using Dukebox.Desktop.Model;
using System.Threading;
using System;
using Dukebox.Library.Factories;
using Dukebox.Configuration.Interfaces;

namespace Dukebox.Desktop.ViewModel
{
    public class FileMenuViewModel : ViewModelBase, IFileMenuViewModel, IDisposable
    {
        public const string FolderBrowserPrompt = "Select folder to load music files from";
        public const string AddToLibraryHeader = "Importing audio files into the library...";
        public const string AddToLibraryTitle = "Library Import";

        private readonly IMusicLibraryImportService _musicLibraryImportService;
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

        public FileMenuViewModel(IMusicLibraryImportService musicLibraryImportService, ITrackGeneratorService tracksGenerator, IAudioPlaylist audioPlaylist, 
            AudioFileFormats audioFileFormats, TrackFactory trackFactory) : base()
        {
            _musicLibraryImportService = musicLibraryImportService;
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

            var progressWindow = new ProgressMonitor();
            var progressViewModel = new ProgressMonitorViewModel();

            progressWindow.DataContext = progressViewModel;
            progressViewModel.Title = AddToLibraryTitle;
            progressViewModel.HeaderText = AddToLibraryHeader;
            progressViewModel.StatusText = "Searching for Audio Files...";

            progressWindow.Show();

            var filesAdded = 0;
            var pathToAdd = _selectFolderDialog.SelectedPath;

            try
            {
                _musicLibraryImportService.AddSupportedFilesInDirectory(pathToAdd, true,
                        (o, a) => ImportStep(progressViewModel, a, ref filesAdded),
                        (o, i) => progressWindow.Dispatcher.InvokeAsync(progressWindow.Close));
            }
            catch (Exception ex)
            {
                var errMsg = string.Format("Unable to add directory '{0}': {1}", pathToAdd, ex.Message);
                System.Windows.MessageBox.Show(errMsg, "Error Adding Directory", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ImportStep(ProgressMonitorViewModel viewModel, AudioFileImportedEventArgs fileImportedArgs, ref int filesAdded)
        {
            viewModel.MaximumProgressValue = (fileImportedArgs.TotalFilesThisImport * 2);

            var percentComplete = (filesAdded / ((float)fileImportedArgs.TotalFilesThisImport / 100)) / 2;

            viewModel.CurrentProgressValue++;

            viewModel.NotificationText = string.Format("{0}%", percentComplete);
            viewModel.StatusText = string.Format(@"{0} '{1}'", fileImportedArgs.Status, Path.GetFileName(fileImportedArgs.FileAdded));

            Interlocked.Increment(ref filesAdded);
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
