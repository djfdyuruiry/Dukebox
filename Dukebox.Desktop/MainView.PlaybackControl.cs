using GlobalHotKey;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace Dukebox.Desktop
{
    public partial class MainView : Form
    {
        private void RegisterMultimediaHotKeys()
        {
            var noKeyMod = System.Windows.Input.ModifierKeys.None;
            var hotKeys = new List<Key>() { Key.MediaPlayPause, Key.MediaNextTrack, Key.MediaPreviousTrack, Key.MediaStop };

            hotKeyManager = new HotKeyManager();

            try
            {
                hotKeys.ForEach(k => hotKeyManager.Register(new HotKey(k, noKeyMod)));

                hotKeyManager.KeyPressed += new EventHandler<KeyPressedEventArgs>((o, e) => { if (e.HotKey.Key == Key.MediaPlayPause) { btnPausePlay_Click(o, null); } });
                hotKeyManager.KeyPressed += new EventHandler<KeyPressedEventArgs>((o, e) => { if (e.HotKey.Key == Key.MediaStop) { btnStop_Click(o, null); } });
                hotKeyManager.KeyPressed += new EventHandler<KeyPressedEventArgs>((o, e) => { if (e.HotKey.Key == Key.MediaNextTrack) { btnNext_Click(o, null); } });
                hotKeyManager.KeyPressed += new EventHandler<KeyPressedEventArgs>((o, e) => { if (e.HotKey.Key == Key.MediaPreviousTrack) { btnPrevious_Click(o, null); } });
            }
            catch (Win32Exception ex)
            {
                _logger.Info("Error registering global multimedia hot keys: " + ex.Message + "");
                DialogResult msgBoxResult = MessageBox.Show("Error registering global multimedia hot keys! This usually happens when another media player is open.", "Dukebox - Error Registering Hot Keys", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);

                // Retry registering hot keys.
                if (msgBoxResult == DialogResult.Retry)
                {
                    RegisterMultimediaHotKeys();
                }
            }
        }

        private void btnPausePlay_Click(object sender, EventArgs e)
        {
            _currentPlaylist.PausePlay();
            UpdatePausePlayGraphic();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _currentPlaylist.Stop();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            _currentPlaylist.Forward();
            UpdatePausePlayGraphic();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            _currentPlaylist.Back();
            UpdatePausePlayGraphic();
        }

        private void lstPlaylist_DoubleClick(object sender, EventArgs e)
        {
            _currentPlaylist.SkipToTrack(lstPlaylist.SelectedIndex);
            UpdatePausePlayGraphic();
        }
    }
}
