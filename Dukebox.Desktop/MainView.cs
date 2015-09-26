using Dukebox.Audio;
using Dukebox.Library;
using Dukebox.Library.CdRipping;
using Dukebox.Model;
using GlobalHotKey;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;

namespace Dukebox.Desktop
{
    /// <summary>
    /// The main UI for Dukebox.
    /// </summary>
    public partial class MainView : Form
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
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

        public MainView()
        {
            InitaliseFileSelectorDialogs();
            InitalisePlaylistAndPlaybackMonitor();

            InitializeComponent();

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | 
                          ControlStyles.ContainerControl |  ControlStyles.OptimizedDoubleBuffer |  ControlStyles.SupportsTransparentBackColor
                          , true);

            LoadPlaybackControlMenuFromSettings();
            RegisterMultimediaHotKeys();
            UpdateFilters();
        }
        
        private void InitalisePlaylistAndPlaybackMonitor()
        {
            _currentPlaylist = new Playlist() { Repeat = false, RepeatAll = false, Shuffle = false };
            _lastPlayedTrackIndex = -1;

            _currentPlaylist.NewTrackLoadedHandlers.Add(new EventHandler((o, e) => Invoke(new ValueUpdateDelegate(() => { try { UpdateUI(o, (NewTrackLoadedEventArgs)e); } catch { } }))));
            _currentPlaylist.TrackListAccessHandlers.Add(new EventHandler((o, e) => Invoke(new ValueUpdateDelegate(() => { try { UpdatePlaybackControls(o, (TrackListAccessEventArgs)e); } catch { } }))));

            _playbackMonitorTimer = new System.Timers.Timer(250);
            _playbackMonitorTimer.Elapsed += UpdatePlaybackTime;
        }

        private void LoadPlaybackControlMenuFromSettings()
        {
            mnuShuffle.Checked = _currentPlaylist.Shuffle = Dukebox.Desktop.Properties.Settings.Default.shuffle;
            mnuRepeat.Checked = _currentPlaylist.Repeat = Dukebox.Desktop.Properties.Settings.Default.repeat;
            mnuRepeatAll.Checked = _currentPlaylist.RepeatAll = Dukebox.Desktop.Properties.Settings.Default.repeatAll;
        }

        /// <summary>
        /// Dispose of hotkey manager, current playlist, playback monitor
        /// thread and the progress window if present.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
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

            Dispose(true);
            GC.SuppressFinalize(this);
        }        

        /// <summary>
        /// Start playback monitor and show all tracks
        /// in library browser.
        /// </summary>
        private void MainView_Load(object sender, EventArgs e)
        {
            _playbackMonitorTimer.Start();
            txtSearchBox_TextChanged(this, null);
        }
        
        /// <summary>
        /// Reposition the currently playing and playlist labels.
        /// </summary>
        private void MainView_Resize(object sender, EventArgs e)
        {
            Point locationOnForm = this.PointToClient(lstPlaylist.Parent.PointToScreen(lstPlaylist.Location));

            lblPlaylist.Location = new Point(locationOnForm.X, lblPlaylist.Location.Y);
            lblCurrentlyPlaying.Location = new Point(locationOnForm.X + lblPlaylist.Width, lblCurrentlyPlaying.Location.Y);
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

        /// <summary>
        /// Stop playback monitor and current playlist
        /// playback, save user settings then exit the application.
        /// </summary>
        private void frmMainView_FormClosed(object sender, FormClosedEventArgs e)
        {
            _currentPlaylist.StopPlaylistPlayback();
            _playbackMonitorTimer.Stop();

            Dukebox.Desktop.Properties.Settings.Default.Save();

            Application.Exit();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
           this.Close();
        }
       
        #endregion

        #region File picker methods

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
                Logger.Info("Error registering global multimedia hot keys: " + ex.Message + "");
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

        #endregion
        
        #region Library management methods

        private void addFilesToLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
            bool scanSubDirectories = true;

            Action<object, AudioFileImportedEventArgs> progressHandler = null;
            progressHandler = new Action<object, AudioFileImportedEventArgs>((o, a) => Invoke(new ValueUpdateDelegate(() => DoTrackAddedToLibrary(o, a))));

            Action<object, int> completeHandler = null;
            completeHandler = new Action<object, int>((o, i) => Invoke(new ValueUpdateDelegate(() => DoImportFinished(o, i))));

            MusicLibrary.GetInstance().AddDirectoryToLibrary(selectedPath, scanSubDirectories, progressHandler, completeHandler);                
        }

        private void DoTrackAddedToLibrary(object sender, AudioFileImportedEventArgs audioFileImported)
        {
            if (_progressWindow.ProgressBarMaximum != audioFileImported.TotalFilesThisImport)
            {
                // Set progress bar maximum if not set already.
                _progressWindow.ProgressBarMaximum = audioFileImported.TotalFilesThisImport;
            }

            // Determine if file is being pre-preprocessed or added to library.
            string actionMsg = string.Empty;

            if (audioFileImported.JustProcessing)
            {
                actionMsg = "Processing ";
            }
            else
            {
                if (_progressWindow.ProgressBarCompleted)
                {
                    _progressWindow.ResetProgressBar();
                }

                actionMsg = "Adding ";
            }

            // Move up a step in progress bar and display notification message.
            _progressWindow.ImportProgressBarStep();
            _progressWindow.NotifcationLabelUpdate(actionMsg + "'" + audioFileImported.FileAdded.Split('\\').LastOrDefault() + "'");
        }

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

            if (numTracksAdded > 0)
            {
                // Refresh filters if files were successfully imported.
                RefreshUI(true);
            }
        }

#endregion

        #region Playlist/UI event handlers

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

            var recentlyPlayedFilter = treeFilters.Nodes[2];
            recentlyPlayedFilter.Collapse();
            recentlyPlayedFilter.Nodes.Clear();

            MusicLibrary.GetInstance().RecentlyPlayed.Where(t => !string.IsNullOrEmpty(t.Artist.name)).Select(t => recentlyPlayedFilter.Nodes.Add(t.Artist.name));
        }

        private void DrawAlbumArt(Track track)
        {
            if (track.Metadata.HasFutherMetadataTag && track.Metadata.HasAlbumArt)
            {
                try
                {
                    picAlbumArt.Image = (Image)(new Bitmap(track.Metadata.AlbumArt, picAlbumArt.Size));
                    picAlbumArt.Visible = true;
                }
                catch (Exception ex)
                {
                    Logger.Info("Error fetching album art for display [" + track.Song.filename + "]: " + ex.Message);
                    picAlbumArt.Image = null;
                    picAlbumArt.Visible = false;
                }
            }
            else if (picAlbumArt.Visible)
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
                Logger.Info("Failed to switch background image for btnPlay on frmMainView [" + ex.Message + "]");
            }
        }

        /// <summary>
        /// Update the playback controls to reflect the current state 
        /// of the media player.
        /// </summary>
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
                Logger.Info(ex);
            }
        }

        /// <summary>
        /// Clear tree node filters and call refresh library cache in
        /// new thread with progress monitor window.
        /// </summary>
        private void UpdateFilters()
        {
            TreeNode artists = treeFilters.Nodes[0];
            TreeNode albums = treeFilters.Nodes[1];

            artists.Nodes.Clear();
            albums.Nodes.Clear();

            _progressWindow = new ProgressMonitorBox();
            _progressWindow.TopMost = true;
            _progressWindow.Show();

            (new Thread(() => RefreshLibraryCache(artists, albums))).Start();
        }

        private void RefreshLibraryCache(TreeNode artists, TreeNode albums)
        {
            // Show busy cursor and display notification.
            _progressWindow.NotifcationLabelUpdate("Refreshing the library cache, this may take a few seconds!");
            Cursor.Current = Cursors.WaitCursor;

            List<artist> artistsInLibrary = MusicLibrary.GetInstance().Artists;
            int artistCount = artistsInLibrary.Count();

            List<album> albumsInLibrary = MusicLibrary.GetInstance().Albums;
            int albumCount = albumsInLibrary.Count();

            Cursor.Current = Cursors.Default;
            
            // Set progress window maximum value to reflect articles to be imported.
            _progressWindow.ProgressBarMaximum = artistCount + albumCount;

            // Artists
            for (int i = 0; i < artistCount; i++)
            {
                _progressWindow.ImportProgressBarStep();
                _progressWindow.NotifcationLabelUpdate("Loading artist '" + artistsInLibrary[i].name + "' from library" + "' [" + i + "/" + artistCount + "]");

                Invoke(new ValueUpdateDelegate(() => artists.Nodes.Add(artistsInLibrary[i].name)));
            }

            // Albums
            for (int i = 0; i < albumCount; i++)
            {
                _progressWindow.ImportProgressBarStep();
                _progressWindow.NotifcationLabelUpdate("Loading album '" + albumsInLibrary[i].name + "' from library" + "' [" + i + "/" + albumCount + "]");

                Invoke(new ValueUpdateDelegate(() => albums.Nodes.Add(albumsInLibrary[i].name)));
            }

            // Dispose of progress window.
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
        /// Load tree node items from library into playlist.
        /// </summary>
        private void treeFilters_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent != null || (e.Node.Text == "Recently Played"))
            {
                TreeNode node = treeFilters.GetNodeAt(e.Location);
                treeFilters.SelectedNode = node;

                _currentPlaylist.StopPlaylistPlayback();
                _currentPlaylist.Tracks.Clear();
                
                if (node.Parent.Text == "Artists")
                {
                    _currentPlaylist.Tracks = MusicLibrary.GetInstance().GetTracksByAttribute(SearchAreas.Artist, e.Node.Text).ToList();
                }
                else if (node.Parent.Text == "Albums")
                {
                    _currentPlaylist.Tracks = MusicLibrary.GetInstance().GetTracksByAttribute(SearchAreas.Album, e.Node.Text).ToList();
                }
                else if (e.Node.Text == "Recently Played")
                {
                    _currentPlaylist.Tracks = MusicLibrary.GetInstance().RecentlyPlayed;
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
            var getMusicLibrary = e.Node.Text == "Music Library";
            var getRecentlyPlayed = e.Node.Text == "Recently Played";

            if (e.Node.Parent != null || getMusicLibrary || getRecentlyPlayed)
            {
                lstLibraryBrowser.Items.Clear();

                TreeNode node = treeFilters.GetNodeAt(e.Location);
                treeFilters.SelectedNode = node;

                if (getMusicLibrary)
                {
                    txtSearchBox.Text = "";
                    txtSearchBox_TextChanged(this, null);
                }
                else if (getRecentlyPlayed)
                {
                    MusicLibrary.GetInstance().RecentlyPlayed.ForEach(t => lstLibraryBrowser.Items.Add(t));
                }   
                else if (node.Parent.Text == "Artists" || node.Parent.Text == "Recently Played")
                {
                    MusicLibrary.GetInstance().GetTracksByAttribute(SearchAreas.Artist, e.Node.Text).ForEach(t => lstLibraryBrowser.Items.Add(t));
                }
                else if (node.Parent.Text == "Albums")
                {
                    MusicLibrary.GetInstance().GetTracksByAttribute(SearchAreas.Album, e.Node.Text).ForEach(t => lstLibraryBrowser.Items.Add(t));
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
                    string msg = "Error saving playlist from the file '" + _playlistExportBrowser.FileName.Split('\\').LastOrDefault() + "' [" + ex.Message + "]";
                    Logger.Info(msg);

                    MessageBox.Show(msg, "Dukebox - Error Saving Playlist File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

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
                    Logger.Info(msg);

                    MessageBox.Show(msg, "Dukebox - Error Loading Playlist File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region CD ripping menu option

        private void ripCdToMP3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog cdFolderDialog = new FolderBrowserDialog();
            FolderBrowserDialog outputFolderDialog = new FolderBrowserDialog();

            if (cdFolderDialog.ShowDialog() == DialogResult.OK)
            {
                if(outputFolderDialog.ShowDialog() == DialogResult.OK)
                {
                    CdRipHelper cdRipHelper = new CdRipHelper();
                    var monitorBox = (ICdRipViewUpdater)new ProgressMonitorBox();

                    (new Thread(() => cdRipHelper.RipCdToFolder(cdFolderDialog.SelectedPath, outputFolderDialog.SelectedPath, monitorBox))).Start();
                }
            }
        }
        
        #endregion

    }

    /// <summary>
    /// Default delegate for value updates in the Dukebox UI.
    /// </summary>
    public delegate void ValueUpdateDelegate();
}
