﻿using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using Dukebox.Audio;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Model.Services;
using System.Threading.Tasks;
using Dukebox.Desktop.Views;
using System.IO;

namespace Dukebox.Desktop.ViewModel
{
    public class FileMenuViewModel : ViewModelBase, IFileMenuViewModel
    {
        public const string FolderBrowserPrompt = "Select folder to load music files from";
        public const string AddToLibraryHeader = "Importing audio files into the library...";
        public const string AddToLibraryTitle = "Library Import";

        private readonly IMusicLibrary _musicLibrary;
        private readonly IAudioPlaylist _audioPlaylist;

        private OpenFileDialog _selectFileDialog;
        private FolderBrowserDialog _selectFolderDialog;

        public ICommand PlayFile { get; private set; }
        public ICommand PlayFolder { get; private set; }
        public ICommand AddFilesToLibrary { get; private set; }
        public ICommand ExportLibrary { get; private set; }
        public ICommand ImportLibrary { get; private set; }
        public ICommand Exit { get; private set; }

        public FileMenuViewModel(IMusicLibrary musicLibrary, IAudioPlaylist audioPlaylist, AudioFileFormats audioFileFormats) : base()
        {
            _musicLibrary = musicLibrary;
            _audioPlaylist = audioPlaylist;

            _selectFileDialog = new OpenFileDialog();
            _selectFolderDialog = new FolderBrowserDialog();

            _selectFileDialog.Filter = audioFileFormats.FileDialogFilter;
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
            var track = _musicLibrary.GetTrackFromFile(fileName);

            _audioPlaylist.LoadPlaylistFromList(new List<Track> { track });
        }

        private void DoPlayFolder()
        {
            var dialogResult = _selectFolderDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }
            
            var tracks = _musicLibrary.GetTracksForDirectory(_selectFolderDialog.SelectedPath, false);
            _audioPlaylist.LoadPlaylistFromList(tracks);
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

            Task.Run(() =>
            {
                _musicLibrary.AddDirectory(_selectFolderDialog.SelectedPath, true,
                    (o, a) => ImportStep(progressViewModel, a),
                    (o, i) => progressWindow.Dispatcher.InvokeAsync(progressWindow.Close));
            });

            progressWindow.Show();
        }

        private void ImportStep(ProgressMonitorViewModel viewModel, AudioFileImportedEventArgs fileImportedArgs)
        {
            if (viewModel.MaximumProgressValue != fileImportedArgs.TotalFilesThisImport)
            {
                viewModel.MaximumProgressValue = fileImportedArgs.TotalFilesThisImport;
            }

            viewModel.CurrentProgressValue++;
            viewModel.StatusText = string.Format(@"{0} '...\{1}\{2}'", fileImportedArgs.Status, 
                Path.GetDirectoryName(fileImportedArgs.FileAdded), Path.GetFileName(fileImportedArgs.FileAdded));
        }
    }
}
