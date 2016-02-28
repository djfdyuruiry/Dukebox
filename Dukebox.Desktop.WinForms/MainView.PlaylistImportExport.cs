using Dukebox.Library.Repositories;
using System;
using System.Windows.Forms;
using System.Linq;

namespace Dukebox.Desktop
{
    public partial class MainView : Form
    {
        private void saveCurrentPlaylistToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_playlistExportBrowser.ShowDialog() == DialogResult.OK)
            {
                var filename = _playlistImportBrowser.FileName;

                try
                {
                    _currentPlaylist.SavePlaylistToFile(filename);
                }
                catch (Exception ex)
                {
                    HandleNonFatalException(string.Format("Error saving current playlist to file '{0}'", filename), ex);
                }
            }
        }

        private void saveToDbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var playlistName = string.Empty;

            if (InputBox.Show("Enter a name for the playlist", "Dukebox - Save Playlist to Library", ref playlistName) == DialogResult.OK)
            {
                try
                {
                    MusicLibrary.GetInstance().AddPlaylist(playlistName, _currentPlaylist.Tracks.Select(t => t.Song.filename));
                    RefreshUI(true);
                }
                catch (Exception ex)
                {
                    HandleNonFatalException(string.Format("Error saving playlist '{0}' to Library", playlistName), ex);
                }
            }
        }   

        private void loadPlaylistFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_playlistImportBrowser.ShowDialog() == DialogResult.OK)
            {
                var filename = _playlistImportBrowser.FileName;

                try
                {
                    var tracksLoaded = _currentPlaylist.LoadPlaylistFromFile(filename);
                    lstPlaylist.Items.Clear();

                    if (tracksLoaded > 0)
                    {
                        _currentPlaylist.Tracks.ForEach(t => lstPlaylist.Items.Add(t));
                        _currentPlaylist.StartPlaylistPlayback();
                    }
                }
                catch (Exception ex)
                {
                    HandleNonFatalException(string.Format("Error loading playlist file '{0}'", filename), ex);
                }
            }
        }

        private void importPlaylistFileToLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_playlistImportBrowser.ShowDialog() == DialogResult.OK)
            {
                var filename = _playlistImportBrowser.FileName;

                try
                {
                    MusicLibrary.GetInstance().AddPlaylistFile(filename);
                }
                catch (Exception ex)
                {
                    HandleNonFatalException(string.Format("Error importing playlist file '{0}' into library", filename), ex);
                }
            }
        }
    }
}
