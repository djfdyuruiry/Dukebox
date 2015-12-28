using Dukebox.Model.Services;
using GlobalHotKey;
using log4net;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Dukebox.Desktop
{
    /// <summary>
    /// The main UI for Dukebox.
    /// </summary>
    public partial class MainView : Form
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string TimeDisplayFormat = "{0} | {1}";
        private const string BlankTime = "00";

        // Playlist properties.
        private int _lastPlayedTrackIndex;
        private Playlist _currentPlaylist;
        private System.Timers.Timer _playbackMonitorTimer;
        private bool _userAlteringTrackBar;

        // File and library objects.
        private FolderBrowserDialog _folderBrowserDialog;
        private OpenFileDialog _fileBrowserDialog;
        private ProgressMonitorBox _progressWindow;

        // Playlist export and import browsers.
        private OpenFileDialog _playlistImportBrowser;
        private SaveFileDialog _playlistExportBrowser;

        // Context menus
        private TrackBrowserContextMenuStrip _trackBrowserContextMenu;

        // Drap and Drop track
        private Track _currentlyDraggingTrack;

        // Global hotkey monitor.
        private HotKeyManager hotKeyManager;
        
        public MainView()
        {
            _userAlteringTrackBar = false;

            InitializeComponent();
        }

        public void Init(Action callbackWhenDone = null)
        {
            InitaliseFileSelectorDialogs();
            InitalisePlaylistAndPlaybackMonitor();
            InitaliseContextMenus();

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                          ControlStyles.ContainerControl | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor
                          , true);

            LoadPlaybackControlMenuFromSettings();
            UpdateFilters(false, callbackWhenDone);
        }

        private void InitaliseContextMenus()
        {
            _trackBrowserContextMenu = new TrackBrowserContextMenuStrip(AddTrackToPlaylist, RemoveTrackFromLibrary);

            lstTrackBrowser.MouseDown += lstTrackBrowser_MouseDown;
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
        /// in track browser.
        /// </summary>
        private void MainView_Load(object sender, EventArgs e)
        {
            RegisterMultimediaHotKeys();
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
    }

    /// <summary>
    /// Default delegate for value updates in the Dukebox UI.
    /// </summary>
    public delegate void ValueUpdateDelegate();
}
