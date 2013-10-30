using Dukebox.Audio;
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

            _currentPlaylist.NewTrackLoadedHandlers.Add(new EventHandler((o, e) => Invoke(new ValueUpdateDelegate(()=>UpdateUI(o, (NewTrackLoadedEventArgs)e)))));
            _currentPlaylist.TrackListAccessHandlers.Add(new EventHandler((o, e) => Invoke(new ValueUpdateDelegate(()=>UpdatePlaybackControls(o, (TrackListAccessEventArgs)e)))));

            _playbackMonitorTimer = new System.Timers.Timer(250);
            _playbackMonitorTimer.Elapsed += UpdatePlaybackTime;
            _playbackMonitorTimer.Start();

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
            // Display currently playing song.
            lblCurrentlyPlaying.Text = e.Track.ToString();

            // Draw the album art if available in the currently playing file.
            if (e.Track.Metadata.HasFutherMetadataTag && e.Track.Metadata.HasAlbumArt)
            {
                picAlbumArt.Image = (Image)(new Bitmap(e.Track.Metadata.AlbumArt, picAlbumArt.Size));
                picAlbumArt.Visible = true;
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
            if (_lastPlayedTrackIndex != e.TrackIndex)
            {
                lstPlaylist.Refresh();
                _lastPlayedTrackIndex = e.TrackIndex;
                lstPlaylist.Refresh();
            }
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
            }
        }

        /// <summary>
        /// Update the playback time monitor label.
        /// </summary>
        private void UpdatePlaybackTime(Object sender, System.Timers.ElapsedEventArgs e)
        {
            if (MediaPlayer.GetInstance().AudioLoaded)
            {
                Invoke(new ValueUpdateDelegate(()=>lblPlaybackTime.Text = MediaPlayer.GetInstance().MinutesPlayed + " | " + MediaPlayer.GetInstance().AudioLengthInMins));
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
            lblCurrentlyPlaying.Text = string.Empty;
        }

        #endregion

        #region Library browser methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    _currentPlaylist.Tracks = MusicLibrary.GetInstance().Tracks.Where(t => t.Artist.name == e.Node.Text).ToList();
                }
                else if (node.Parent.Text == "Albums")
                {
                    _currentPlaylist.Tracks = MusicLibrary.GetInstance().Tracks.Where(t => t.Album.name == e.Node.Text).ToList();
                }

                RefreshUI();

                _currentPlaylist.StartPlaylistPlayback();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeFilters_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                lstLibraryBrowser.Items.Clear();

                TreeNode node = treeFilters.GetNodeAt(e.Location);
                treeFilters.SelectedNode = node;

                if (node.Parent.Text == "Artists")
                {
                    MusicLibrary.GetInstance().Tracks.Where(t => t.Artist != null).Where(t => t.Artist.name == e.Node.Text).OrderBy(t => t.Song.artistId.HasValue ? t.Song.artistId.Value : 0).ToList().ForEach(t => lstLibraryBrowser.Items.Add(t));
                }
                else if (node.Parent.Text == "Albums")
                {
                    MusicLibrary.GetInstance().Tracks.Where(t => t.Album != null).Where(t => t.Album.name == e.Node.Text).OrderBy(t => t.Song.albumId.HasValue ? t.Song.albumId.Value : 0).ToList().ForEach(t => lstLibraryBrowser.Items.Add(t));
                }

                RefreshUI();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstLibraryBrowser_DoubleClick(object sender, EventArgs e)
        {
            if (_currentPlaylist.StreamingPlaylist)
            {
                _currentPlaylist.StopPlaylistPlayback();
            }

            _currentPlaylist.Tracks = new List<Track>();

            foreach (Object i in lstLibraryBrowser.Items)
            {
                _currentPlaylist.Tracks.Add((Track)i);
            }

            RefreshUI();

            _currentPlaylist.SkipToTrack(lstLibraryBrowser.SelectedIndex);
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
                    (new Thread(() => RipCdToFolder(cdFolderDialog.SelectedPath, outputFolderDialog.SelectedPath))).Start();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inPath"></param>
        /// <param name="outPath"></param>
        private void RipCdToFolder(string inPath, string outPath)
        {
            try
            {
                string[] inFiles = Directory.GetFiles(inPath);
                List<AudioFileMetaData> cdMetadata = AudioFileMetaData.GetAudioFileMetaDataForCd(inPath[0]);

                Invoke(new ValueUpdateDelegate(CreateCdRippingMonitor));
                int maxIdx = inFiles.Length;

                // Rip each 
                for (int i = 0; i < maxIdx; i++)
                {

                    Track t = MusicLibrary.GetInstance().GetTrackFromFile(inFiles[i], cdMetadata[i]);
                    string outFile = outPath + "\\" + t.ToString() + ".mp3";

                    MediaPlayer.ConvertCdaFileToMp3(inFiles[i], outFile, new BaseEncoder.ENCODEFILEPROC((a, b) => CdRippingProgressMonitor(a, b, t, maxIdx, i)), true);

                    // Wait until track has been ripped.
                    while (_progressWindow.ProgressBarValue != 100)
                    {
                        Thread.Sleep(10);
                    }

                    TAG_INFO outputTag = BassTags.BASS_TAG_GetFromFile(outFile);

                    outputTag.album = t.Album.name;
                    outputTag.artist = t.Artist.name;
                    outputTag.title = t.Song.title;

                    _progressWindow.ProgressBarValue = 0;
                }

                Invoke(new ValueUpdateDelegate(DestroyCdRippingMonitor));
            }
            catch (Exception ex)
            {
                string msg = "Error ripping music from Audio CD: " + ex.Message;

                Logger.log(msg);
                MessageBox.Show(msg, "Dukebox - Error Ripping from CD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateCdRippingMonitor()
        {
            _progressWindow = new ProgressMonitorBox();
            _progressWindow.Text = "Dukebox - MP3 Ripping Progress";
            _progressWindow.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        public void DestroyCdRippingMonitor()
        {
            _progressWindow.Hide();
            _progressWindow.Dispose();
            _progressWindow = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalBytesToEncode"></param>
        /// <param name="bytesEncodedSoFar"></param>
        /// <param name="track"></param>
        private void CdRippingProgressMonitor(long totalBytesToEncode, long bytesEncodedSoFar, Track track, int totalTracksToRip, int currentTrackIndex)
        {
            if (_progressWindow.ProgressBarMaximum != 100)
            {
                _progressWindow.ResetProgressBar();
                _progressWindow.ProgressBarMaximum = 100;
            }

            int percentComplete = (int)(bytesEncodedSoFar / (totalBytesToEncode / 100));

            _progressWindow.ProgressBarValue = percentComplete;
            _progressWindow.NotifcationLabelUpdate("[" + currentTrackIndex + 1 + "/" + totalTracksToRip + "] Converting " + track + " to MP3 - " + percentComplete + "%");
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
