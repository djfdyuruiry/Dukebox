using Dukebox.Audio;
using Dukebox.Library.Model;
using Dukebox.Library.Repositories;
using Dukebox.Model.Services;
using GlobalHotKey;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Drawing;

namespace Dukebox.Desktop
{
    public partial class MainView : Form
    {
        private void HandleNonFatalException(string errorMessage, Exception ex)
        {
            _logger.Error(errorMessage, ex);
            MessageBox.Show(string.Format("{0}: {1}", errorMessage, ex.Message), "Dukebox Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void AddTrackToPlaylist(Track track)
        {
            try
            {
                if (track == null)
                {
                    throw new ArgumentNullException("track");
                }

                _currentPlaylist.Tracks.Add(track);
                RefreshUI(false, false);
            }
            catch (Exception ex)
            {
                HandleNonFatalException(string.Format("Error while adding track '{0}' to the playlist", track), ex);
            }
        }

        /// <summary>
        /// Update the forms view model to display the current
        /// status for playback. Show the current track details,
        /// album art if available and highlight the current track
        /// in the playlist control.
        /// </summary>
        private void UpdateUI(Object sender, NewTrackLoadedEventArgs e)
        {
            // Prevent over processing when no new information needs to
            // be handled.
            if (e.TrackIndex == _lastPlayedTrackIndex)
            {
                return;
            }

            // Display currently playing song.
            lblCurrentlyPlaying.Text = e.Track.ToString();

            DrawAlbumArt(e.Track);
            UpdatePausePlayGraphic();

            // Update playlist control.
            lstPlaylist.Refresh();
            _lastPlayedTrackIndex = e.TrackIndex;
            lstPlaylist.Refresh();

            var recentlyPlayedFilter = treeLibraryBrowser.Nodes[LibraryBrowserNodes.RecentlyPlayedIndex];
            recentlyPlayedFilter.Collapse();
            recentlyPlayedFilter.Nodes.Clear();

            MusicLibrary.GetInstance().RecentlyPlayed.Where(t => t.Artist != null).Select(t => recentlyPlayedFilter.Nodes.Add(t.Artist.name));
        }

        private void DrawAlbumArt(Track track)
        {
            if (track.Metadata.HasAlbumArt)
            {
                try
                {
                    picAlbumArt.Image = (Image)(new Bitmap(track.Metadata.AlbumArt, picAlbumArt.Size));
                    picAlbumArt.Visible = true;

                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Error fetching album art for display for file {0}", track.Song.filename), ex);
                }
            }

            if (picAlbumArt.Visible)
            {
                picAlbumArt.Image = null;
                picAlbumArt.Visible = false;
            }
        }

        private void UpdatePausePlayGraphic()
        {
            try
            {
                // Update pause/play background image.
                if (_currentPlaylist.PlayingAudio && btnPlay.BackgroundImage != Dukebox.Desktop.Properties.Resources.GnomeMediaPlaybackPause)
                {
                    btnPlay.BackgroundImage = Dukebox.Desktop.Properties.Resources.GnomeMediaPlaybackPause;
                }
                else if (!_currentPlaylist.PlayingAudio && btnPlay.BackgroundImage != Dukebox.Desktop.Properties.Resources.GnomeMediaPlaybackStart)
                {
                    btnPlay.BackgroundImage = Dukebox.Desktop.Properties.Resources.GnomeMediaPlaybackStart;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to switch background image for btnPlay on frmMainView", ex);
            }
        }

        /// <summary>
        /// Update the playback controls to reflect the current state 
        /// of the media player.
        /// </summary>
        private void UpdatePlaybackControls(Object sender, TrackListAccessEventArgs e)
        {
            // Enable saving current playlist when it is not empty.
            if (e.TrackListSize > 0)
            {
                saveToFileToolStripMenuItem.Enabled = true;
                saveToDbToolStripMenuItem.Enabled = true;
            }
            else if (e.TrackListSize < 1)
            {
                saveToFileToolStripMenuItem.Enabled = false;
                saveToDbToolStripMenuItem.Enabled = false;

                lstPlaylist.Items.Clear();

                lblCurrentlyPlaying.Text = string.Empty;
                lblPlaybackTime.Text = string.Format(MediaPlayer.MINUTE_FORMAT, BlankTime, BlankTime);

                trackAudioSeek.Value = 0;
            }
        }

        /// <summary>
        /// Update the playback time monitor label.
        /// </summary>
        private void UpdatePlaybackTime(Object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (_currentPlaylist.StreamingPlaylist)
                {
                    var mediaPlayer = MediaPlayer.GetInstance();
                    var minsPlayed = mediaPlayer.MinutesPlayed;
                    var audioLengthInMins = mediaPlayer.AudioLengthInMins;
                    var percentagePlayed = mediaPlayer.PercentagePlayed;

                    Invoke(new ValueUpdateDelegate(() => lblPlaybackTime.Text = string.Format(TimeDisplayFormat, minsPlayed, audioLengthInMins)));
                    Invoke(new ValueUpdateDelegate(() =>
                    {
                        if (!_userAlteringTrackBar)
                        {
                            trackAudioSeek.Value = (int)(percentagePlayed * 2);
                        }
                    }));
                }
                else
                {
                    Invoke(new ValueUpdateDelegate(() => lblPlaybackTime.Text = string.Format(MediaPlayer.MINUTE_FORMAT, BlankTime, BlankTime)));
                    Invoke(new ValueUpdateDelegate(() => trackAudioSeek.Value = 0));
                    Invoke(new ValueUpdateDelegate(() => lblCurrentlyPlaying.Text = string.Empty));
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error("Error updating playback time", ex);
            }
        }

        /// <summary>
        /// Clear tree node filters and call refresh library cache in
        /// new thread with progress monitor window.
        /// </summary>
        private void UpdateFilters(bool showProgressWindow = true, Action callbackWhenDone = null)
        {
            TreeNode artists = treeLibraryBrowser.Nodes[LibraryBrowserNodes.ArtistsIndex];
            TreeNode albums = treeLibraryBrowser.Nodes[LibraryBrowserNodes.AlbumsIndex];
            TreeNode playlists = treeLibraryBrowser.Nodes[LibraryBrowserNodes.PlaylistsIndex];

            artists.Nodes.Clear();
            albums.Nodes.Clear();
            playlists.Nodes.Clear();

            if (showProgressWindow)
            {
                _progressWindow = new ProgressMonitorBox();
                _progressWindow.TopMost = true;
                _progressWindow.Show();
            }

            (new Thread(() =>
            {
                RefreshLibraryCache(artists, albums, playlists, showProgressWindow);

                if (callbackWhenDone != null)
                {
                    callbackWhenDone();
                }
            })).Start();
        }

        private void RefreshLibraryCache(TreeNode artists, TreeNode albums, TreeNode playlists, bool showProgressWindow = true)
        {
            if (showProgressWindow)
            {
                // Show busy cursor and display notification.         
                _progressWindow.NotifcationLabelUpdate("Refreshing the library cache, this may take a few seconds");
                Cursor.Current = Cursors.WaitCursor;
            }

            var artistsInLibrary = MusicLibrary.GetInstance().OrderedArtists;
            var artistCount = artistsInLibrary.Count;

            var albumsInLibrary = MusicLibrary.GetInstance().OrderedAlbums;
            var albumCount = albumsInLibrary.Count;

            var playlistsInLibrary = MusicLibrary.GetInstance().OrderedPlaylists;
            var playlistCount = playlistsInLibrary.Count;

            if (showProgressWindow)
            {
                Cursor.Current = Cursors.Default;
            }

            if (showProgressWindow)
            {
                // Set progress window maximum value to reflect articles to be imported.
                _progressWindow.ProgressBarMaximum = artistCount + albumCount + playlistCount;
            }

            var progress = 1;

            // Artists
            foreach (var artist in artistsInLibrary)
            {
                if (showProgressWindow)
                {
                    _progressWindow.ImportProgressBarStep();
                    _progressWindow.NotifcationLabelUpdate(string.Format("Loading artist '{0}' from library [{1}/{2}]", artist.name, progress, artistCount));
                }

                Invoke(new ValueUpdateDelegate(() => artists.Nodes.Add(artist.id.ToString(), artist.name)));
                progress++;
            }

            progress = 1;

            // Albums
            foreach (var album in albumsInLibrary)
            {
                if (showProgressWindow)
                {
                    _progressWindow.ImportProgressBarStep();
                    _progressWindow.NotifcationLabelUpdate(string.Format("Loading artist '{0}' from library [{1}/{2}]", album.name, progress, albumCount));
                }

                Invoke(new ValueUpdateDelegate(() => albums.Nodes.Add(album.id.ToString(), album.name)));
                progress++;
            }

            progress = 1;

            // Playlists
            foreach (var playlist in playlistsInLibrary)
            {
                if (showProgressWindow)
                {
                    _progressWindow.ImportProgressBarStep();
                    _progressWindow.NotifcationLabelUpdate(string.Format("Loading playlist '{0}' from library [{1}/{2}]", playlist.name, progress, albumCount));
                }

                Invoke(new ValueUpdateDelegate(() => playlists.Nodes.Add(playlist.id.ToString(), playlist.name)));
                progress++;
            }

            if (showProgressWindow)
            {
                // Dispose of progress window.
                Invoke(new ValueUpdateDelegate(() =>
                {
                    _progressWindow.Hide();
                    _progressWindow.Dispose();
                    _progressWindow = null;
                }));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateFilters"></param>
        /// <param name="redrawPlaybackTime"></param>
        private void RefreshUI(bool updateFilters = false, bool redrawPlaybackTime = true)
        {
            if (redrawPlaybackTime)
            {
                lblPlaybackTime.Text = string.Format(MediaPlayer.MINUTE_FORMAT, 0, 0);
            }

            lstPlaylist.Items.Clear();
            _currentPlaylist.Tracks.ForEach(t => { lstPlaylist.Items.Add(t); });
            lstPlaylist.Refresh();

            if (updateFilters)
            {
                UpdateFilters();
            }
        }

        /// <summary>
        /// Manually handle drawing of playlist items on 
        /// screen. Highlight currently playing track with
        /// a different background colour.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstPlaylist_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Graphics g = e.Graphics;

            if (e.Index == _currentPlaylist.GetCurrentTrackIndex())
            {
                g.FillRectangle(new SolidBrush(Color.PowderBlue), e.Bounds);
            }

            if (e.Index != -1)
            {
                Track track = ((Track)lstPlaylist.Items[e.Index]);
                g.DrawString(track.ToString(), e.Font, new SolidBrush(e.ForeColor), new PointF(e.Bounds.X, e.Bounds.Y));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstTrackBrowser_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Graphics g = e.Graphics;

            if (e.Index != -1)
            {
                Track track = ((Track)lstTrackBrowser.Items[e.Index]);
                g.DrawString(track.ToString(), e.Font, new SolidBrush(e.ForeColor), new PointF(e.Bounds.X, e.Bounds.Y));
            }
        }

        private void lstTrackBrowser_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks > 1)
            {
                return;
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        InitTrackDragDrop(e);
                        break;
                    }
                case MouseButtons.Right:
                    {
                        InitTrackBrowserContextMenu(e);
                        break;
                    }
            }
        }

        private void InitTrackDragDrop(MouseEventArgs e)
        {
            var selectedListIndex = lstTrackBrowser.IndexFromPoint(e.Location);

            if (selectedListIndex == ListBox.NoMatches)
            {
                return;
            }

            var track = (Track)lstTrackBrowser.Items[selectedListIndex];
            DoDragDrop(track.ToString(), DragDropEffects.All);
        }

        private void InitTrackBrowserContextMenu(MouseEventArgs e)
        {
            var selectedListIndex = lstTrackBrowser.IndexFromPoint(e.Location);

            if (selectedListIndex == ListBox.NoMatches)
            {
                return;
            }

            var selectedTrack = (Track)lstTrackBrowser.Items[selectedListIndex];

            lstTrackBrowser.SelectedIndex = selectedListIndex;
            _trackBrowserContextMenu.Show(selectedTrack);
        }
    }
}
