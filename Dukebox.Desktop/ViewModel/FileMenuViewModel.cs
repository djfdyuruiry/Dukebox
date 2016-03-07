using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using Dukebox.Audio;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;
using Dukebox.Model.Services;

namespace Dukebox.Desktop.ViewModel
{
    public class FileMenuViewModel : ViewModelBase, IFileMenuViewModel
    {
        public const string FolderBrowserPrompt = "Select folder to load music files from";

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

            _musicLibrary.AddDirectory(_selectFolderDialog.SelectedPath, true, 
                (o, a) => ImportStep(a), 
                (o, i) => ImportComplete(i));
        }

        private void ImportStep(AudioFileImportedEventArgs a)
        {

        }

        private void ImportComplete(int tracksAdded)
        {
        }
    }
}
