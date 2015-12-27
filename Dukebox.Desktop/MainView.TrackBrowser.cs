using Dukebox.Library.Model;
using Dukebox.Library.Repositories;
using Dukebox.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Dukebox.Desktop
{
    public partial class MainView : Form
    {
        /// <summary>
        /// Update the track browser with results with each change of
        /// search term text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearchBox_TextChanged(object sender, EventArgs e)
        {
            var searchAreas = new List<SearchAreas>() { SearchAreas.All };

            lstTrackBrowser.Items.Clear();
            var results = MusicLibrary.GetInstance().SearchForTracks(txtSearchBox.Text, searchAreas);
            results.ForEach(t => lstTrackBrowser.Items.Add(t));
        }

        /// <summary>
        /// Load tree node items from library into playlist.
        /// </summary>
        private void treeFilters_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent != null || (e.Node.Text == LibraryBrowserNodes.RecentlyPlayed))
            {
                var node = treeLibraryBrowser.GetNodeAt(e.Location);
                long nodeKey;

                long.TryParse(e.Node.Name, out nodeKey);

                treeLibraryBrowser.SelectedNode = node;

                _currentPlaylist.StopPlaylistPlayback();
                _currentPlaylist.Tracks.Clear();

                if (node.Parent.Text == LibraryBrowserNodes.Artists)
                {
                    _currentPlaylist.Tracks = MusicLibrary.GetInstance().GetTracksByAttribute(SearchAreas.Artist, nodeKey).ToList();
                }
                else if (node.Parent.Text == LibraryBrowserNodes.Albums)
                {
                    _currentPlaylist.Tracks = MusicLibrary.GetInstance().GetTracksByAttribute(SearchAreas.Album, nodeKey).ToList();
                }
                else if (e.Node.Text == LibraryBrowserNodes.RecentlyPlayed)
                {
                    _currentPlaylist.Tracks = MusicLibrary.GetInstance().RecentlyPlayed;
                }
                else if (node.Parent.Text == LibraryBrowserNodes.Playlists)
                {
                    try
                    {
                        var tracks = MusicLibrary.GetInstance().GetPlaylistById(nodeKey);
                        _currentPlaylist.Tracks = tracks.GetTracksForPlaylist();
                    }
                    catch (Exception ex)
                    {
                        HandleNonFatalException(string.Format("Error loading playlist '{0}'", node.Text), ex);
                    }
                }

                RefreshUI();
                _currentPlaylist.StartPlaylistPlayback();
            }
        }

        /// <summary>
        /// Load tree node items from library into browser list.
        /// </summary>
        private void treeFilters_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var getMusicLibrary = e.Node.Text == LibraryBrowserNodes.MusicLibrary;
            var getRecentlyPlayed = e.Node.Text == LibraryBrowserNodes.RecentlyPlayed;
            long nodeKey;
            
            long.TryParse(e.Node.Name, out nodeKey);

            if (e.Node.Parent != null || getMusicLibrary || getRecentlyPlayed)
            {
                lstTrackBrowser.Items.Clear();

                TreeNode node = treeLibraryBrowser.GetNodeAt(e.Location);
                treeLibraryBrowser.SelectedNode = node;

                if (getMusicLibrary)
                {
                    txtSearchBox.Text = string.Empty;
                    txtSearchBox_TextChanged(this, null);
                }
                else if (getRecentlyPlayed)
                {
                    MusicLibrary.GetInstance().RecentlyPlayed.ForEach(t => lstTrackBrowser.Items.Add(t));
                }
                else if (node.Parent.Text == LibraryBrowserNodes.Artists || node.Parent.Text == LibraryBrowserNodes.RecentlyPlayed)
                {
                    MusicLibrary.GetInstance().GetTracksByAttribute(SearchAreas.Artist, nodeKey).ForEach(t => lstTrackBrowser.Items.Add(t));
                }
                else if (node.Parent.Text == LibraryBrowserNodes.Albums)
                {
                    MusicLibrary.GetInstance().GetTracksByAttribute(SearchAreas.Album, nodeKey).ForEach(t => lstTrackBrowser.Items.Add(t));
                }
                else if (node.Parent.Text == LibraryBrowserNodes.Playlists)
                {
                    try
                    {
                        var tracks = MusicLibrary.GetInstance().GetPlaylistById(nodeKey).GetTracksForPlaylist();
                        tracks.ForEach(t => lstTrackBrowser.Items.Add(t));
                    }
                    catch (Exception ex)
                    {
                        HandleNonFatalException(string.Format("Error loading playlist '{0}'", node.Text), ex);
                    }
                }

                RefreshUI(false, false);
            }
        }

        /// <summary>
        /// Either play a new playlist on double click of a library
        /// browser item, or skip to the corresponding track in the
        /// current playlist of the track browser items mirror the
        /// current playlist's items.
        /// </summary>
        private void lstTrackBrowser_DoubleClick(object sender, EventArgs e)
        {
            // Check that current playlist contains items...
            bool isCurrentPlaylist = _currentPlaylist.Tracks.Count > 0;
            // ...and those items are the ones in the track browser...
            _currentPlaylist.Tracks.ForEach(a => { if (!isCurrentPlaylist) return; isCurrentPlaylist = lstTrackBrowser.Items.Contains(a); });
            // ...and in the same quantity.
            isCurrentPlaylist = isCurrentPlaylist && _currentPlaylist.Tracks.Count == lstTrackBrowser.Items.Count;

            if (isCurrentPlaylist) // Current playlist mirrors the track browser contents, just skip to appropriate track index.
            {
                int trackIdx = _currentPlaylist.Tracks.IndexOf((Track)lstTrackBrowser.SelectedItem);

                _currentPlaylist.SkipToTrack(trackIdx);
            }
            else // Reload playlist with track browser items and skip to selected track.
            {
                if (_currentPlaylist.StreamingPlaylist)
                {
                    _currentPlaylist.StopPlaylistPlayback();
                }

                _currentPlaylist.Tracks.Clear();

                foreach (Object i in lstTrackBrowser.Items)
                {
                    _currentPlaylist.Tracks.Add((Track)i);
                }

                RefreshUI();
                _currentPlaylist.SkipToTrack(lstTrackBrowser.SelectedIndex);
            }
        }

        /*
         * Track browser drag and drop methods.
         */

        private void lstBox_DragDrop(object sender, DragEventArgs e)
        {
            if (_currentlyDraggingTrack == null)
            {
                return;
            }

            var listBox = (ListBox)sender;

            if (listBox == lstPlaylist)
            {
                AddTrackToPlaylist(_currentlyDraggingTrack);
            }

            _currentlyDraggingTrack = null;
        }

        private void lstTrackBrowser_DragLeave(object sender, EventArgs e)
        {
            var listBox = (ListBox)sender;

            _currentlyDraggingTrack = (Track)listBox.SelectedItem;
        }

        private void lstPlaylist_DragEnter(object sender, DragEventArgs e)
        {
            return;
        }
    }
}
