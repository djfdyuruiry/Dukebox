using Dukebox.Audio;
using Dukebox.Library.CdRipping;
using Dukebox.Library;
using Dukebox.Logging;
using Dukebox.Model;
using GlobalHotKey;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.Misc;

namespace Dukebox
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainView : Form
    {
        // Playlist properties.
        private int _lastPlayedTrackIndex;
        private Playlist _currentPlaylist;
        private System.Timers.Timer _playbackMonitorTimer;

        // File and library objects.
        private FolderBrowserDialog _folderBrowserDialog;
        private OpenFileDialog _fileBrowserDialog;
        private ProgressMonitorBox _progressWindow;

        // Playlist export and import browsers.
        private OpenFileDialog _playlistImportBrowser;
        private SaveFileDialog _playlistExportBrowser;

        // Global hotkey monitor.
        private HotKeyManager hotKeyManager;

        #region Form lifecycle methods

        /// <summary>
        /// 
        /// </summary>
        public MainView()
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

            _currentPlaylist = new Playlist() { Repeat= false, RepeatAll = false, Shuffle = false };
            _lastPlayedTrackIndex = -1;

            _currentPlaylist.NewTrackLoadedHandlers.Add(new EventHandler((o, e) => Invoke(new ValueUpdateDelegate(()=>{try{UpdateUI(o, (NewTrackLoadedEventArgs)e);}catch(Exception ex){}}))));
            _currentPlaylist.TrackListAccessHandlers.Add(new EventHandler((o, e) => Invoke(new ValueUpdateDelegate(()=>{try{UpdatePlaybackControls(o, (TrackListAccessEventArgs)e);}catch(Exception ex){}}))));

            _playbackMonitorTimer = new System.Timers.Timer(250);
            _playbackMonitorTimer.Elapsed += UpdatePlaybackTime;

            InitializeComponent();

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | 
                          ControlStyles.ContainerControl |  ControlStyles.OptimizedDoubleBuffer |  ControlStyles.SupportsTransparentBackColor
                          , true);

            mnuShuffle.Checked = Dukebox.Properties.Settings.Default.shuffle;
            mnuRepeat.Checked = Dukebox.Properties.Settings.Default.repeat;
            mnuRepeatAll.Checked = Dukebox.Properties.Settings.Default.repeatAll;

            hotKeyManager = new HotKeyManager();
            RegisterHotKeys();

            UpdateFilters();
        }

        /// <summary>
        /// 
        /// </summary>
        public new void Dispose()
        {
            hotKeyManager.Dispose();

            _currentPlaylist.Dispose();

            _playbackMonitorTimer.Stop();
            _playbackMonitorTimer.Dispose();

            if (_progressWindow != null)
            {
                _progressWindow.Close();
                _progressWindow.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RegisterHotKeys()
        {
            var noKeyMod = System.Windows.Input.ModifierKeys.None;
            var hotKeys = new List<Key>() { Key.MediaPlayPause, Key.MediaNextTrack, Key.MediaPreviousTrack, Key.MediaStop, Key.VolumeUp, Key.VolumeDown };

            try
            {
                hotKeys.ForEach(k => hotKeyManager.Register(new HotKey(k, noKeyMod)));

                hotKeyManager.KeyPressed += new EventHandler<KeyPressedEventArgs>((o, e) => { if (e.HotKey.Key == Key.MediaPlayPause) { btnPlay_Click(o, null); } });
                hotKeyManager.KeyPressed += new EventHandler<KeyPressedEventArgs>((o, e) => { if (e.HotKey.Key == Key.MediaStop) { btnStop_Click(o, null); } });
                hotKeyManager.KeyPressed += new EventHandler<KeyPressedEventArgs>((o, e) => { if (e.HotKey.Key == Key.MediaNextTrack) { btnNext_Click(o, null); } });
                hotKeyManager.KeyPressed += new EventHandler<KeyPressedEventArgs>((o, e) => { if (e.HotKey.Key == Key.MediaPreviousTrack) { btnPrevious_Click(o, null); } });
            }
            catch (Win32Exception ex)
            {
                Logger.log("Error registering global multimedia hot keys: " + ex.Message + "");
                DialogResult msgBoxResult = MessageBox.Show("Error registering global multimedia hot keys! This usually happens when another media player is open.", "Dukebox - Error Registering Hot Keys",  MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning );

                // Retry registering hot keys.
                if (msgBoxResult == DialogResult.Retry)
                {
                    RegisterHotKeys();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainView_Load(object sender, EventArgs e)
        {
            _playbackMonitorTimer.Start();
            txtSearchBox_TextChanged(this, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMainView_FormClosed(object sender, FormClosedEventArgs e)
        {
            _currentPlaylist.StopPlaylistPlayback();
            _playbackMonitorTimer.Stop();
            Application.Exit();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
           this.Close();
        }

        #endregion

        #region File picker methods

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
            if (_folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                _currentPlaylist.StopPlaylistPlayback();

                _currentPlaylist.Tracks = MusicLibrary.GetInstance().GetTracksForDirectory(_folderBrowserDialog.SelectedPath, false);
                               
                _currentPlaylist.StartPlaylistPlayback();

                RefreshUI();
            }
        }

        #endregion

        #region Playback control methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, EventArgs e)
        {
            _currentPlaylist.PausePlay();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            _currentPlaylist.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_currentPlaylist.CurrentTrackIndex != _currentPlaylist.Tracks.Count - 1 || Dukebox.Properties.Settings.Default.repeatAll)
            {
                _currentPlaylist.Forward();
            }
            else
            {
                _currentPlaylist.StopPlaylistPlayback();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            _currentPlaylist.Back();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstFiles_DoubleClick(object sender, EventArgs e)
        {
            _currentPlaylist.SkipToTrack(lstPlaylist.SelectedIndex);
        }

        #endregion
        
        #region Library management methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addFilesToLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {                
                Thread addMusicThread = new Thread(() => MusicLibrary.GetInstance().AddDirectoryToLibrary(@_folderBrowserDialog.SelectedPath,
                                                                                                          true,
                                                                                                          new Action<object, AudioFileImportedEventArgs>((o,a) => Invoke(new ValueUpdateDelegate(() => DoTrackAddedToLibrary(o,a) ))),
                                                                                                          new Action<object, int>((o,i) => Invoke(new ValueUpdateDelegate(() => DoImportFinished(o,i) )))));

                _progressWindow = new ProgressMonitorBox();
                _progressWindow.Show();

                addMusicThread.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoTrackAddedToLibrary(object sender, AudioFileImportedEventArgs e)
        {
            if (_progressWindow.ProgressBarMaximum != e.TotalFilesThisImport)
            {
                _progressWindow.ProgressBarMaximum = e.TotalFilesThisImport;
            }

            string prepend = string.Empty;

            if (e.JustProcessing)
            {
                prepend = "Processing ";
            }
            else
            {
                if (_progressWindow.ProgressBarCompleted)
                {
                    _progressWindow.ResetProgressBar();
                }

                prepend = "Adding ";
            }

            _progressWindow.ImportProgressBarStep();
            _progressWindow.NotifcationLabelUpdate(prepend + "'" + e.FileAdded.Split('\\').LastOrDefault());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="numTracksAdded"></param>
        private void DoImportFinished(object sender, int numTracksAdded)
        {
            if (numTracksAdded > 0)
            {
                MessageBox.Show("Successfully added " + numTracksAdded + " tracks to the library!", "Dukebox - Added Tracks to Library", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No compatible audio files found in specified directory!", "Dukebox - No Tracks Added to Library", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _progressWindow.Hide();
            _progressWindow.Dispose();
            _progressWindow = null;

            RefreshUI(true);
        }

#endregion

        #region Playlist/UI event handlers

        /// <summary>
        /// Update the forms view model to display the current
        /// status for playback. Show the current track details,
        /// album art if available and highlight the current track
        /// in the playlist control.
        /// </summary>
        /// <param name="sender">The object that invoked this event.</param>
        /// <param name="e">The details of the new track loaded.</param>
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

            // Draw the album art if available in the currently playing file.
            if (e.Track.Metadata.HasFutherMetadataTag && e.Track.Metadata.HasAlbumArt)
            {
                try
                {
                    picAlbumArt.Image = (Image)(new Bitmap(e.Track.Metadata.AlbumArt, picAlbumArt.Size));
                    picAlbumArt.Visible = true;
                }
                catch (Exception ex)
                {
                    Logger.log("Error fetching album art for display [" + e.Track.Song.filename + "]: " + ex.Message);
                    picAlbumArt.Image = null;
                    picAlbumArt.Visible = false;
                }
            }
            else if (picAlbumArt.Visible)
            {
                picAlbumArt.Image = null;
                picAlbumArt.Visible = false;
            }

            try
            {
                // Update pause/play background image.
                if (_currentPlaylist.PlayingAudio && btnPlay.BackgroundImage != Dukebox.Properties.Resources.GnomeMediaPlaybackPause)
                {
                    btnPlay.BackgroundImage = Dukebox.Properties.Resources.GnomeMediaPlaybackPause;
                }
                else if (!_currentPlaylist.PlayingAudio && btnPlay.BackgroundImage != Dukebox.Properties.Resources.GnomeMediaPlaybackStart)
                {
                    btnPlay.BackgroundImage = Dukebox.Properties.Resources.GnomeMediaPlaybackStart;
                }
            }
            catch (Exception ex)
            {
                Logger.log("Failed to switch background image for btnPlay on frmMainView [" + ex.Message + "]");
            }

            // Update playlist control.
            lstPlaylist.Refresh();
            _lastPlayedTrackIndex = e.TrackIndex;
            lstPlaylist.Refresh();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdatePlaybackControls(Object sender, TrackListAccessEventArgs e)
        {
            // Enable saving current playlist when it is not empty.
            if (e.TrackListSize > 0 && !saveToFileToolStripMenuItem.Enabled)
            {
                saveToFileToolStripMenuItem.Enabled = true;
            }
            else if (e.TrackListSize < 1 && loadFromFileToolStripMenuItem.Enabled)
            {
                saveToFileToolStripMenuItem.Enabled = false;

                lstPlaylist.Items.Clear();

                lblCurrentlyPlaying.Text = string.Empty;
                lblPlaybackTime.Text = string.Format(MediaPlayer.MINUTE_FORMAT, 0.0f, 0.0f);
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
                    Invoke(new ValueUpdateDelegate(() => lblPlaybackTime.Text = MediaPlayer.GetInstance().MinutesPlayed + " | " + MediaPlayer.GetInstance().AudioLengthInMins));
                }
                else
                {
                    Invoke(new ValueUpdateDelegate(() => lblPlaybackTime.Text = string.Format(MediaPlayer.MINUTE_FORMAT, 0.ToString("00"), 0.ToString("00"))));
                    Invoke(new ValueUpdateDelegate(() => lblCurrentlyPlaying.Text = string.Empty));
                }
            }
            catch(InvalidOperationException ex)
            {
                Logger.log(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateFilters()
        {
            TreeNode artists = treeFilters.Nodes[0];
            TreeNode albums = treeFilters.Nodes[1];

            foreach (TreeNode n in treeFilters.Nodes)
            {
                n.Nodes.Clear();
            }

            _progressWindow = new ProgressMonitorBox();
            _progressWindow.TopMost = true;
            _progressWindow.Show();

            (new Thread(() => RefreshLibraryCache(artists, albums))).Start();
        }

        /// <summary>
        /// 
        /// </summary>
        private void RefreshLibraryCache(TreeNode artists, TreeNode albums)
        {
            _progressWindow.NotifcationLabelUpdate("Refreshing the library cache, this may take a few seconds!");
            Cursor.Current = Cursors.WaitCursor;

            List<artist> artistsInLibrary = MusicLibrary.GetInstance().Artists;
            int artistCount = artistsInLibrary.Count();

            List<album> albumsInLibrary = MusicLibrary.GetInstance().Albums;
            int albumCount = albumsInLibrary.Count();

            Cursor.Current = Cursors.Default;

            _progressWindow.ProgressBarMaximum = artistCount + albumCount;

            for (int i = 0; i < artistCount; i++)
            {
                _progressWindow.ImportProgressBarStep();
                _progressWindow.NotifcationLabelUpdate("Loading artist '" + artistsInLibrary[i].name + "' from library" + "' [" + i + "/" + artistCount + "]");

                Invoke(new ValueUpdateDelegate(() => artists.Nodes.Add(artistsInLibrary[i].name)));
            }

            for (int i = 0; i < albumCount; i++)
            {
                _progressWindow.ImportProgressBarStep();
                _progressWindow.NotifcationLabelUpdate("Loading album '" + albumsInLibrary[i].name + "' from library" + "' [" + i + "/" + albumCount + "]");

                Invoke(new ValueUpdateDelegate(() => albums.Nodes.Add(albumsInLibrary[i].name)));
            }

            Invoke(new ValueUpdateDelegate(() =>
            {
                _progressWindow.Hide();
                _progressWindow.Dispose();
                _progressWindow = null;
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateFilters"></param>
        private void RefreshUI(bool updateFilters = false)
        {
            lblPlaybackTime.Text = string.Format(MediaPlayer.MINUTE_FORMAT, 0, 0);

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
            
            if (e.Index == _currentPlaylist.CurrentTrackIndex)
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
        private void lstLibraryBrowser_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Graphics g = e.Graphics;
            

            if (e.Index != -1)
            {
                Track track = ((Track)lstLibraryBrowser.Items[e.Index]);
                g.DrawString(track.ToString(), e.Font, new SolidBrush(e.ForeColor), new PointF(e.Bounds.X, e.Bounds.Y));
            }
        }

        #endregion

        #region Playlist flow control methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuShuffle_Click(object sender, EventArgs e)
        {
            Dukebox.Properties.Settings.Default.shuffle = !Dukebox.Properties.Settings.Default.shuffle;
            _currentPlaylist.Shuffle = Dukebox.Properties.Settings.Default.shuffle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuRepeat_Click(object sender, EventArgs e)
        {
            Dukebox.Properties.Settings.Default.repeat = !Dukebox.Properties.Settings.Default.repeat;
            _currentPlaylist.Repeat = Dukebox.Properties.Settings.Default.repeat;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuRepeatAll_Click(object sender, EventArgs e)
        {
            Dukebox.Properties.Settings.Default.repeatAll = !Dukebox.Properties.Settings.Default.repeatAll;
            _currentPlaylist.RepeatAll = Dukebox.Properties.Settings.Default.repeatAll;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            _currentPlaylist.StopPlaylistPlayback();
            _currentPlaylist.Tracks.Clear();

            lstPlaylist.Items.Clear();
        }

        #endregion

        #region Library browser methods
        
        /// <summary>
        /// Update the library browser with results with each change of
        /// search term text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearchBox_TextChanged(object sender, EventArgs e)
        {
            lstLibraryBrowser.Items.Clear();
            MusicLibrary.GetInstance().SearchForTracks(txtSearchBox.Text, SearchAreas.All).ForEach(t => lstLibraryBrowser.Items.Add(t));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">The event arguments.</param>
        private void treeFilters_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                TreeNode node = treeFilters.GetNodeAt(e.Location);
                treeFilters.SelectedNode = node;

                _currentPlaylist.StopPlaylistPlayback();
                _currentPlaylist.Tracks.Clear();

                if (node.Parent.Text == "Artists")
                {
                    _currentPlaylist.Tracks = MusicLibrary.GetInstance().SearchForTracks(e.Node.Text, SearchAreas.Artist).ToList();
                }
                else if (node.Parent.Text == "Albums")
                {
                    _currentPlaylist.Tracks = MusicLibrary.GetInstance().SearchForTracks(e.Node.Text, SearchAreas.Album).ToList();
                }

                RefreshUI();

                _currentPlaylist.StartPlaylistPlayback();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">The event arguments.</param>
        private void treeFilters_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                lstLibraryBrowser.Items.Clear();

                TreeNode node = treeFilters.GetNodeAt(e.Location);
                treeFilters.SelectedNode = node;

                if (node.Parent.Text == "Artists")
                {
                    MusicLibrary.GetInstance().SearchForTracks(e.Node.Text, SearchAreas.Artist).ForEach(t => lstLibraryBrowser.Items.Add(t));
                }
                else if (node.Parent.Text == "Albums")
                {
                    MusicLibrary.GetInstance().SearchForTracks(e.Node.Text, SearchAreas.Album).ForEach(t => lstLibraryBrowser.Items.Add(t));
                }

                RefreshUI();
            }
        }

        /// <summary>
        /// Either play a new playlist on double click of a library
        /// browser item, or skip to the corresponding track in the
        /// current playlist of the library browser items mirror the
        /// current playlist's items.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">The event arguments.</param>
        private void lstLibraryBrowser_DoubleClick(object sender, EventArgs e)
        {
            // Check that current playlist contains items...
            bool isCurrentPlaylist = _currentPlaylist.Tracks.Count > 0;
            // ...and those items are the ones in the library browser...
            _currentPlaylist.Tracks.ForEach(a => { if (!isCurrentPlaylist) return; isCurrentPlaylist = lstLibraryBrowser.Items.Contains(a); });            
            // ...and in the same quantity.
            isCurrentPlaylist = isCurrentPlaylist && _currentPlaylist.Tracks.Count == lstLibraryBrowser.Items.Count;

            if (isCurrentPlaylist) // Current playlist mirrors the library browser contents, just skip to appropriate track index.
            {
                int trackIdx = _currentPlaylist.Tracks.IndexOf((Track)lstLibraryBrowser.SelectedItem);

                _currentPlaylist.SkipToTrack(trackIdx);
            }
            else // Reload playlist with library browser items and skip to selected track.
            {
                if (_currentPlaylist.StreamingPlaylist)
                {
                    _currentPlaylist.StopPlaylistPlayback();
                }

                _currentPlaylist.Tracks.Clear();

                foreach (Object i in lstLibraryBrowser.Items)
                {
                    _currentPlaylist.Tracks.Add((Track)i);
                }

                RefreshUI();

                _currentPlaylist.SkipToTrack(lstLibraryBrowser.SelectedIndex);
            }
        }

        #endregion

        #region Playlist export and import methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_playlistExportBrowser.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _currentPlaylist.SavePlaylistToFile(_playlistExportBrowser.FileName);
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_playlistImportBrowser.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if(_currentPlaylist.LoadPlaylistFromFile(_playlistImportBrowser.FileName) > 0)
                    {
                        lstPlaylist.Items.Clear();
                        _currentPlaylist.Tracks.ForEach(t => lstPlaylist.Items.Add(t));
                        _currentPlaylist.StartPlaylistPlayback();
                    }
                }
                catch (Exception ex)
                {
                    lstPlaylist.Items.Clear();
                    _currentPlaylist.Tracks.Clear();

                    string msg = "Error loading playlist from the file '" + _playlistImportBrowser.FileName.Split('\\').LastOrDefault() + "' [" + ex.Message +"]";
                    Logger.log(msg);

                    MessageBox.Show(msg, "Dukebox - Error Loading Playlist File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region CD ripping methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ripCdToMP3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog cdFolderDialog = new FolderBrowserDialog();
            FolderBrowserDialog outputFolderDialog = new FolderBrowserDialog();

            if (cdFolderDialog.ShowDialog() == DialogResult.OK)
            {
                if(outputFolderDialog.ShowDialog() == DialogResult.OK)
                {
                    (new Thread(() => (new CdRipHelper()).RipCdToFolder(cdFolderDialog.SelectedPath, outputFolderDialog.SelectedPath))).Start();
                }
            }
        }
        
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainView_Resize(object sender, EventArgs e)
        {
            Point locationOnForm = lstPlaylist.FindForm().PointToClient(lstPlaylist.Parent.PointToScreen(lstPlaylist.Location));
            lblPlaylist.Location = new Point(locationOnForm.X, lblPlaylist.Location.Y);
            lblCurrentlyPlaying.Location = new Point(locationOnForm.X + lstPlaylist.Width, lblCurrentlyPlaying.Location.Y);
        }

        /// <summary>
        /// Show the about box window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new AboutBox()).Show();
        }

    }

    /// <summary>
    /// Default delegate for value updates in the Dukebox UI.
    /// </summary>
    public delegate void ValueUpdateDelegate();
}
