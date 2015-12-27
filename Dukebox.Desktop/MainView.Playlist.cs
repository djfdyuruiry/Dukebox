using System;
using System.Windows.Forms;

namespace Dukebox.Desktop
{
    public partial class MainView : Form
    {
        private void mnuShuffle_Click(object sender, EventArgs e)
        {
            Dukebox.Desktop.Properties.Settings.Default.shuffle = !Dukebox.Desktop.Properties.Settings.Default.shuffle;
            _currentPlaylist.Shuffle = Dukebox.Desktop.Properties.Settings.Default.shuffle;
        }

        private void mnuRepeat_Click(object sender, EventArgs e)
        {
            Dukebox.Desktop.Properties.Settings.Default.repeat = !Dukebox.Desktop.Properties.Settings.Default.repeat;
            _currentPlaylist.Repeat = Dukebox.Desktop.Properties.Settings.Default.repeat;
        }

        private void mnuRepeatAll_Click(object sender, EventArgs e)
        {
            Dukebox.Desktop.Properties.Settings.Default.repeatAll = !Dukebox.Desktop.Properties.Settings.Default.repeatAll;
            _currentPlaylist.RepeatAll = Dukebox.Desktop.Properties.Settings.Default.repeatAll;
        }

        private void btnClearPlaylist_Click(object sender, EventArgs e)
        {
            _currentPlaylist.StopPlaylistPlayback();
            _currentPlaylist.Tracks.Clear();

            lstPlaylist.Items.Clear();
        }
    }
}
