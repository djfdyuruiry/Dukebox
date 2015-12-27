using Dukebox.Library.Model;
using Dukebox.Library.Repositories;
using Dukebox.Model.Services;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Dukebox.Desktop
{
    public partial class MainView : Form
    {
        private void addFilesToLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _folderBrowserDialog.Description = "Select a folder to add to the library (sub-folders will be scanned as well)";

            if (_folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                Thread addMusicThread = new Thread(AddFilesToLibrary);

                _progressWindow = new ProgressMonitorBox();
                _progressWindow.Show();

                addMusicThread.Start();
            }
        }

        private void AddFilesToLibrary()
        {
            string selectedPath = @_folderBrowserDialog.SelectedPath;

            try
            {
                bool scanSubDirectories = true;

                Action<object, AudioFileImportedEventArgs> progressHandler = null;
                progressHandler = new Action<object, AudioFileImportedEventArgs>((o, a) => Invoke(new ValueUpdateDelegate(() => DoTrackAddedToLibrary(o, a))));

                Action<object, int> completeHandler = null;
                completeHandler = new Action<object, int>((o, i) => Invoke(new ValueUpdateDelegate(() => DoImportFinished(o, i))));

                MusicLibrary.GetInstance().AddDirectory(selectedPath, scanSubDirectories, progressHandler, completeHandler);
            }
            catch (Exception ex)
            {
                HandleNonFatalException(string.Format("Error while adding path '{0}' to the library", selectedPath), ex);
            }
        }

        private void RemoveTrackFromLibrary(Track track)
        {
            try
            {
                if (track == null)
                {
                    throw new ArgumentNullException("track");
                }

                lstTrackBrowser.Items.Remove(track);

                if (_currentPlaylist.CurrentlyLoadedTrack != null)
                {
                    if (_currentPlaylist.CurrentlyLoadedTrack == track)
                    {
                        _currentPlaylist.StopPlaylistPlayback();
                    }
                }

                _currentPlaylist.Tracks.Remove(track);

                lstPlaylist.Items.Remove(track);

                MusicLibrary.GetInstance().RemoveTrack(track);
            }
            catch (Exception ex)
            {
                HandleNonFatalException(string.Format("Error while removing track '{0}' from the library", track), ex);
            }
        }

        private void DoTrackAddedToLibrary(object sender, AudioFileImportedEventArgs audioFileImported)
        {
            if (_progressWindow.ProgressBarMaximum != audioFileImported.TotalFilesThisImport)
            {
                // Set progress bar maximum if not set already.
                _progressWindow.ProgressBarMaximum = audioFileImported.TotalFilesThisImport;
            }

            // Reset progress bar if import has finished.
            if (audioFileImported.Status == AudioFileImportedStatus.Added && _progressWindow.ProgressBarCompleted)
            {
                _progressWindow.ResetProgressBar();
            }

            // Move up a step in progress bar and display notification message.
            _progressWindow.ImportProgressBarStep();
            _progressWindow.NotifcationLabelUpdate(string.Format("{0} '{1}'", audioFileImported.Status, audioFileImported.FileAdded.Split('\\').LastOrDefault()));
        }

        private void DoImportFinished(object sender, int numTracksAdded)
        {
            if (numTracksAdded > 0)
            {
                MessageBox.Show(string.Format("Successfully added {0} tracks to the library", numTracksAdded), "Dukebox - Added Tracks to Library", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No compatible audio files found in specified directory", "Dukebox - No Tracks Added to Library", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _progressWindow.Hide();
            _progressWindow.Dispose();
            _progressWindow = null;

            if (numTracksAdded > 0)
            {
                // Refresh filters if files were successfully imported.
                RefreshUI(true);
            }
        }
    }
}
