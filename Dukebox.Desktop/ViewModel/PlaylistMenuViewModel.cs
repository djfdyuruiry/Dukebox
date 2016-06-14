using System;
using System.Windows.Forms;
using System.Windows.Input;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
namespace Dukebox.Desktop.ViewModel
{
    public class PlaylistMenuViewModel : ViewModelBase, IPlaylistMenuViewModel, IDisposable
    {
        public const string PlaylistFileFilter = "Playlist Files |*.jpl";
        
        private readonly IMusicLibrary _musicLibrary;
        private readonly IAudioPlaylist _audioPlaylist;

        private readonly OpenFileDialog _selectFileDialog;
        private readonly SaveFileDialog _saveFileDialog;

        private bool _saveToFileEnabled;

        public ICommand Clear { get; private set; }
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

        public PlaylistMenuViewModel(IMusicLibrary musicLibrary, IAudioPlaylist audioPlaylist) : base()
        {
            _musicLibrary = musicLibrary;
            _audioPlaylist = audioPlaylist;

            _selectFileDialog = new OpenFileDialog();
            _saveFileDialog = new SaveFileDialog();

            _selectFileDialog.Filter = PlaylistFileFilter;
            _saveFileDialog.Filter = PlaylistFileFilter;

            // enable saving playlist when tracks are present
            _audioPlaylist.TrackListModified += (o, e) => SaveToFileEnabled = audioPlaylist.Tracks.Count > 0;

            Clear = new RelayCommand(_audioPlaylist.ClearPlaylist);
            LoadFromFile = new RelayCommand(DoLoadFromFile);
            SaveToFile = new RelayCommand(DoSaveToFile);
            ImportPlaylistToLibrary = new RelayCommand(DoPlayListImport);
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

            _musicLibrary.AddPlaylistFiles(fileName);
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
