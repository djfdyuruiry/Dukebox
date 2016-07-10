using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Desktop.Views;

namespace Dukebox.Desktop.ViewModel
{
    public class PlaylistMenuViewModel : ViewModelBase, IPlaylistMenuViewModel, IDisposable
    {
        private const string noCurrentTrackDialogTitle = "You must load some tracks into the current playlist before saving it to the database.";
        private const string noCurrentTrackMessage = "Dukebox - No Tracks Currently Loaded";
        private const string savePlaylistDialogTitle = "Save Current Playlist";
        private const string savePlaylistDialogPrompt = "The current playlist will now be saved to your music library. Please enter a name for your new playlist:";

        public const string PlaylistFileFilter = "Playlist Files |*.jpl";
                   
        private readonly IMusicLibraryImportService _musicLibraryImportService;
        private readonly IAudioPlaylist _audioPlaylist;

        private readonly OpenFileDialog _selectFileDialog;
        private readonly SaveFileDialog _saveFileDialog;

        private bool _saveToFileEnabled;

        public ICommand Clear { get; private set; }
        public ICommand SaveCurrentPlaylistToLibrary { get; private set; }
        public ICommand LoadFromFile { get; private set; }
        public ICommand SaveToFile { get; private set; }
        public ICommand ImportPlaylistToLibrary { get; private set; }

        public bool SaveToFileEnabled
        {
            get
            {
                return _saveToFileEnabled;
            }
            private set
            {
                _saveToFileEnabled = value;
                OnPropertyChanged("SaveToFileEnabled");
            }
        }

        public PlaylistMenuViewModel(IMusicLibraryImportService musicLibraryImportService, IAudioPlaylist audioPlaylist) : base()
        {
            _musicLibraryImportService = musicLibraryImportService;
            _audioPlaylist = audioPlaylist;

            _selectFileDialog = new OpenFileDialog();
            _saveFileDialog = new SaveFileDialog();

            _selectFileDialog.Filter = PlaylistFileFilter;
            _saveFileDialog.Filter = PlaylistFileFilter;

            // enable saving playlist when tracks are present
            _audioPlaylist.TrackListModified += (o, e) => SaveToFileEnabled = audioPlaylist.Tracks.Count > 0;

            Clear = new RelayCommand(_audioPlaylist.ClearPlaylist);
            SaveCurrentPlaylistToLibrary = new RelayCommand(DoSaveCurrentPlaylist);
            LoadFromFile = new RelayCommand(DoLoadFromFile);
            SaveToFile = new RelayCommand(DoSaveToFile);
            ImportPlaylistToLibrary = new RelayCommand(DoPlayListImport);
        }

        private void DoSaveCurrentPlaylist()
        {
            if (!_audioPlaylist.Tracks.Any())
            {
                System.Windows.MessageBox.Show(noCurrentTrackDialogTitle, noCurrentTrackMessage,
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var inputPromptViewModel = new InputPromptViewModel
            {
                Title = savePlaylistDialogTitle,
                Prompt = savePlaylistDialogPrompt
            };
            var inputPrompt = new InputPrompt
            {
                DataContext = inputPromptViewModel
            };

            var dialogResult = inputPrompt.ShowDialog();

            if (!dialogResult.HasValue || !dialogResult.Value)
            {
                return;
            }

            var name = inputPromptViewModel.Input;
            var files = _audioPlaylist.Tracks.Select(t => t.Song.FileName).Distinct().ToList();

            _musicLibraryImportService.AddPlaylist(name, files);
        }

        private void DoLoadFromFile()
        {
            var dialogResult = _selectFileDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            var fileName = _selectFileDialog.FileName;

            _audioPlaylist.LoadPlaylistFromFile(fileName);
        }

        private void DoSaveToFile()
        {
            var dialogResult = _saveFileDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            var fileName = _saveFileDialog.FileName;

            _audioPlaylist.SavePlaylistToFile(fileName);
        }

        private void DoPlayListImport()
        {
            var dialogResult = _selectFileDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            var fileName = _selectFileDialog.FileName;

            _musicLibraryImportService.AddPlaylistFiles(fileName);
        }

        protected virtual void Dispose(bool cleanAllResources)
        {
            if (cleanAllResources)
            {
                _saveFileDialog.Dispose();
                _selectFileDialog.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
