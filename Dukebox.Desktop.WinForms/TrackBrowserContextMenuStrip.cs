using Dukebox.Model.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dukebox.Desktop
{
    /// <summary>
    /// Context menu for the track browser.
    /// </summary>
    public class TrackBrowserContextMenuStrip : ContextMenuStrip 
    {
        private ToolStripMenuItem _addToPlaylistMenuItem;
        private ToolStripMenuItem _removeFromLibraryMenuItem;
        private ToolStripMenuItem _propertiesMenuItem;

        private Action<Track> _addTrackHandler;
        private Action<Track> _removeTrackFromLibraryHandler;

        private Track _currentTrack;

        public TrackBrowserContextMenuStrip(Action<Track> addTrackHandler, Action<Track> removeTrackFromLibraryHandler)
        {
            if (addTrackHandler == null)
            {
                throw new ArgumentNullException("addTrackHandler");
            }

            if (removeTrackFromLibraryHandler == null)
            {
                throw new ArgumentNullException("removeTrackFromLibraryHandler");
            }

            var seperator = new ToolStripSeparator();

            _addToPlaylistMenuItem = new ToolStripMenuItem() { Text = "Add to Playlist" };
            _removeFromLibraryMenuItem = new ToolStripMenuItem() { Text = "Remove from Library" };
            _propertiesMenuItem = new ToolStripMenuItem() { Text = "Properties" };

            _addToPlaylistMenuItem.Click += ((s, e) => MenuItem_Click(s, e, AddToPlaylistMenuItem_Click));
            _removeFromLibraryMenuItem.Click += ((s, e) => MenuItem_Click(s, e, RemoveFromLibraryMenuItem_Click));
            _propertiesMenuItem.Click += ((s, e) => MenuItem_Click(s, e, PropertiesMenuItem_Click));

            Items.AddRange(new ToolStripItem[] { _addToPlaylistMenuItem, _removeFromLibraryMenuItem, seperator, _propertiesMenuItem });

            _addTrackHandler = addTrackHandler;
            _removeTrackFromLibraryHandler = removeTrackFromLibraryHandler;

            Visible = false;
            Hide();
        }

        public void MenuItem_Click(object sender, EventArgs e, Action<object, EventArgs> clickHandler)
        {
            if (_currentTrack == null)
            {
                return;
            }

            clickHandler(sender, e);

            _currentTrack = null;
        }

        private void AddToPlaylistMenuItem_Click(object sender, EventArgs e)
        {
            _addTrackHandler(_currentTrack);
        }

        private void RemoveFromLibraryMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _removeTrackFromLibraryHandler(_currentTrack);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error occurred while removing track '{0}' from the library", _currentTrack), ex);
            }
        }

        private void PropertiesMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();

            //ShowPropertiesWindows(CurrentTrack);
        }

        public void Show(Track currentTrack)
        {
            _currentTrack = currentTrack;

            base.Show(Cursor.Position);
            Visible = true;
        }

        
    }
}
