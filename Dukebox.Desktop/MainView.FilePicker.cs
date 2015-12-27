using Dukebox.Audio;
using Dukebox.Library.Repositories;
using Dukebox.Model.Services;
using System;
using System.Windows.Forms;

namespace Dukebox.Desktop
{
    public partial class MainView : Form
    {
        private void InitaliseFileSelectorDialogs()
        {
            _folderBrowserDialog = new FolderBrowserDialog();

            _fileBrowserDialog = new OpenFileDialog();
            _fileBrowserDialog.Filter = AudioFileFormats.FileDialogFilter;
            _fileBrowserDialog.Multiselect = false;

            _playlistExportBrowser = new SaveFileDialog();
            _playlistExportBrowser.Filter = "Playlist Files|*.jpl";

            _playlistImportBrowser = new OpenFileDialog();
            _playlistImportBrowser.Filter = "Playlist Files|*.jpl";
            _playlistImportBrowser.Multiselect = false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = _fileBrowserDialog.FileName;
                Track track = MusicLibrary.GetInstance().GetTrackFromFile(fileName);

                if (track != null)
                {
                    _currentPlaylist.StopPlaylistPlayback();
                    _currentPlaylist.Tracks.Clear();

                    _currentPlaylist.Tracks.Add(track);
                    _currentPlaylist.StartPlaylistPlayback();

                    RefreshUI();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuPlayFolder_Click(object sender, EventArgs e)
        {
            _folderBrowserDialog.Description = "Select folder to load music files from";

            if (_folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                _currentPlaylist.StopPlaylistPlayback();

                _currentPlaylist.Tracks = MusicLibrary.GetInstance().GetTracksForDirectory(_folderBrowserDialog.SelectedPath, false);

                RefreshUI();
                _currentPlaylist.StartPlaylistPlayback();
            }
        }
    }
}
