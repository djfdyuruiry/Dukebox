namespace Dukebox.Desktop
{
    partial class MainView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                hotKeyManager.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Artists");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Albums");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Recently Played");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
            this.btnPlay = new System.Windows.Forms.Button();
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.playFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlayFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.addFilesToLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.audioCDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlayCd = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ripTrackToMP3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlayback = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuShuffle = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRepeat = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRepeatAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlaylist = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.loadFromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuImportPlaylist = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.treeFilters = new System.Windows.Forms.TreeView();
            this.lstLibraryBrowser = new System.Windows.Forms.ListBox();
            this.lblFilters = new System.Windows.Forms.Label();
            this.lblLibraryList = new System.Windows.Forms.Label();
            this.lblPlaylist = new System.Windows.Forms.Label();
            this.lblCurrentlyPlaying = new System.Windows.Forms.Label();
            this.picAlbumArt = new System.Windows.Forms.PictureBox();
            this.tbLibraryNavigator = new System.Windows.Forms.TableLayoutPanel();
            this.tblLibraryContents = new System.Windows.Forms.TableLayoutPanel();
            this.txtSearchBox = new System.Windows.Forms.TextBox();
            this.tblLibraryBrowserAndPlaylist = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLibrary = new System.Windows.Forms.Panel();
            this.pnlPlaylist = new System.Windows.Forms.Panel();
            this.lstPlaylist = new Dukebox.Desktop.BufferedListBox();
            this.pnlPlaybackControls = new System.Windows.Forms.Panel();
            this.pnlOverview = new System.Windows.Forms.Panel();
            this.tblMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblPlaybackTime = new System.Windows.Forms.Label();
            this.mnuMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAlbumArt)).BeginInit();
            this.tbLibraryNavigator.SuspendLayout();
            this.tblLibraryContents.SuspendLayout();
            this.tblLibraryBrowserAndPlaylist.SuspendLayout();
            this.pnlLibrary.SuspendLayout();
            this.pnlPlaylist.SuspendLayout();
            this.pnlPlaybackControls.SuspendLayout();
            this.pnlOverview.SuspendLayout();
            this.tblMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPlay
            // 
            this.btnPlay.BackgroundImage = global::Dukebox.Desktop.Properties.Resources.GnomeMediaPlaybackStart;
            this.btnPlay.Location = new System.Drawing.Point(3, 3);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(35, 32);
            this.btnPlay.TabIndex = 0;
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPausePlay_Click);
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.audioCDToolStripMenuItem,
            this.mnuPlayback,
            this.mnuPlaylist,
            this.mnuHelp});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Size = new System.Drawing.Size(1008, 24);
            this.mnuMain.TabIndex = 4;
            this.mnuMain.Text = "mnuMain";
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playFileToolStripMenuItem,
            this.mnuPlayFolder,
            this.toolStripSeparator1,
            this.addFilesToLibraryToolStripMenuItem,
            this.exportLibraryToolStripMenuItem,
            this.importLibraryToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(37, 20);
            this.mnuFile.Text = "File";
            // 
            // playFileToolStripMenuItem
            // 
            this.playFileToolStripMenuItem.Name = "playFileToolStripMenuItem";
            this.playFileToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.playFileToolStripMenuItem.Text = "Play File...";
            this.playFileToolStripMenuItem.Click += new System.EventHandler(this.playFileToolStripMenuItem_Click);
            // 
            // mnuPlayFolder
            // 
            this.mnuPlayFolder.Name = "mnuPlayFolder";
            this.mnuPlayFolder.Size = new System.Drawing.Size(187, 22);
            this.mnuPlayFolder.Text = "Play Folder...";
            this.mnuPlayFolder.Click += new System.EventHandler(this.mnuPlayFolder_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(184, 6);
            // 
            // addFilesToLibraryToolStripMenuItem
            // 
            this.addFilesToLibraryToolStripMenuItem.Name = "addFilesToLibraryToolStripMenuItem";
            this.addFilesToLibraryToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.addFilesToLibraryToolStripMenuItem.Text = "Add Files To Library...";
            this.addFilesToLibraryToolStripMenuItem.Click += new System.EventHandler(this.addFilesToLibraryToolStripMenuItem_Click);
            // 
            // exportLibraryToolStripMenuItem
            // 
            this.exportLibraryToolStripMenuItem.Enabled = false;
            this.exportLibraryToolStripMenuItem.Name = "exportLibraryToolStripMenuItem";
            this.exportLibraryToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.exportLibraryToolStripMenuItem.Text = "Export Library...";
            // 
            // importLibraryToolStripMenuItem
            // 
            this.importLibraryToolStripMenuItem.Enabled = false;
            this.importLibraryToolStripMenuItem.Name = "importLibraryToolStripMenuItem";
            this.importLibraryToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.importLibraryToolStripMenuItem.Text = "Import Library...";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(184, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // audioCDToolStripMenuItem
            // 
            this.audioCDToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuPlayCd,
            this.toolStripSeparator4,
            this.ripTrackToMP3ToolStripMenuItem});
            this.audioCDToolStripMenuItem.Name = "audioCDToolStripMenuItem";
            this.audioCDToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.audioCDToolStripMenuItem.Text = "Audio CD";
            // 
            // mnuPlayCd
            // 
            this.mnuPlayCd.Name = "mnuPlayCd";
            this.mnuPlayCd.Size = new System.Drawing.Size(169, 22);
            this.mnuPlayCd.Text = "Play Audio CD...";
            this.mnuPlayCd.Click += new System.EventHandler(this.mnuPlayFolder_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(166, 6);
            // 
            // ripTrackToMP3ToolStripMenuItem
            // 
            this.ripTrackToMP3ToolStripMenuItem.Name = "ripTrackToMP3ToolStripMenuItem";
            this.ripTrackToMP3ToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.ripTrackToMP3ToolStripMenuItem.Text = "Rip CD to Folder...";
            this.ripTrackToMP3ToolStripMenuItem.Click += new System.EventHandler(this.ripCdToMP3ToolStripMenuItem_Click);
            // 
            // mnuPlayback
            // 
            this.mnuPlayback.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuShuffle,
            this.mnuRepeat,
            this.mnuRepeatAll});
            this.mnuPlayback.Name = "mnuPlayback";
            this.mnuPlayback.Size = new System.Drawing.Size(66, 20);
            this.mnuPlayback.Text = "Playback";
            // 
            // mnuShuffle
            // 
            this.mnuShuffle.CheckOnClick = true;
            this.mnuShuffle.Name = "mnuShuffle";
            this.mnuShuffle.Size = new System.Drawing.Size(127, 22);
            this.mnuShuffle.Text = "Shuffle";
            this.mnuShuffle.Click += new System.EventHandler(this.mnuShuffle_Click);
            // 
            // mnuRepeat
            // 
            this.mnuRepeat.CheckOnClick = true;
            this.mnuRepeat.Name = "mnuRepeat";
            this.mnuRepeat.Size = new System.Drawing.Size(127, 22);
            this.mnuRepeat.Text = "Repeat";
            this.mnuRepeat.Click += new System.EventHandler(this.mnuRepeat_Click);
            // 
            // mnuRepeatAll
            // 
            this.mnuRepeatAll.CheckOnClick = true;
            this.mnuRepeatAll.Name = "mnuRepeatAll";
            this.mnuRepeatAll.Size = new System.Drawing.Size(127, 22);
            this.mnuRepeatAll.Text = "Repeat All";
            this.mnuRepeatAll.Click += new System.EventHandler(this.mnuRepeatAll_Click);
            // 
            // mnuPlaylist
            // 
            this.mnuPlaylist.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem,
            this.toolStripSeparator3,
            this.loadFromFileToolStripMenuItem,
            this.saveToFileToolStripMenuItem,
            this.toolStripSeparator5,
            this.mnuImportPlaylist});
            this.mnuPlaylist.Name = "mnuPlaylist";
            this.mnuPlaylist.Size = new System.Drawing.Size(56, 20);
            this.mnuPlaylist.Text = "Playlist";
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.btnClearPlaylist_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(212, 6);
            // 
            // loadFromFileToolStripMenuItem
            // 
            this.loadFromFileToolStripMenuItem.Name = "loadFromFileToolStripMenuItem";
            this.loadFromFileToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.loadFromFileToolStripMenuItem.Text = "Load From File...";
            this.loadFromFileToolStripMenuItem.Click += new System.EventHandler(this.loadFromFileToolStripMenuItem_Click);
            // 
            // saveToFileToolStripMenuItem
            // 
            this.saveToFileToolStripMenuItem.Enabled = false;
            this.saveToFileToolStripMenuItem.Name = "saveToFileToolStripMenuItem";
            this.saveToFileToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.saveToFileToolStripMenuItem.Text = "Save To File...";
            this.saveToFileToolStripMenuItem.Click += new System.EventHandler(this.saveToFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(212, 6);
            // 
            // mnuImportPlaylist
            // 
            this.mnuImportPlaylist.Name = "mnuImportPlaylist";
            this.mnuImportPlaylist.Size = new System.Drawing.Size(215, 22);
            this.mnuImportPlaylist.Text = "Import Playlist To Library...";
            // 
            // mnuHelp
            // 
            this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.mnuHelp.Name = "mnuHelp";
            this.mnuHelp.Size = new System.Drawing.Size(44, 20);
            this.mnuHelp.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // btnStop
            // 
            this.btnStop.BackgroundImage = global::Dukebox.Desktop.Properties.Resources.GnomeMediaPlaybackStop;
            this.btnStop.Location = new System.Drawing.Point(41, 3);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(35, 32);
            this.btnStop.TabIndex = 6;
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(164, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(63, 32);
            this.btnClear.TabIndex = 7;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClearPlaylist_Click);
            // 
            // btnNext
            // 
            this.btnNext.BackgroundImage = global::Dukebox.Desktop.Properties.Resources.GnomeMediaSeekForward;
            this.btnNext.Location = new System.Drawing.Point(123, 3);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(35, 32);
            this.btnNext.TabIndex = 8;
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnPrevious
            // 
            this.btnPrevious.BackgroundImage = global::Dukebox.Desktop.Properties.Resources.GnomeMediaSeekBackward;
            this.btnPrevious.Location = new System.Drawing.Point(82, 3);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(35, 32);
            this.btnPrevious.TabIndex = 9;
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // treeFilters
            // 
            this.treeFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeFilters.Location = new System.Drawing.Point(3, 29);
            this.treeFilters.Name = "treeFilters";
            treeNode1.Name = "ArtistNode";
            treeNode1.Text = "Artists";
            treeNode2.Name = "AlbumsNode";
            treeNode2.Text = "Albums";
            treeNode3.Name = "RecentlyPlayedNode";
            treeNode3.Text = "Recently Played";
            this.treeFilters.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3});
            this.treeFilters.Size = new System.Drawing.Size(215, 621);
            this.treeFilters.TabIndex = 10;
            this.treeFilters.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeFilters_NodeMouseClick);
            this.treeFilters.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeFilters_NodeMouseDoubleClick);
            // 
            // lstLibraryBrowser
            // 
            this.lstLibraryBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLibraryBrowser.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstLibraryBrowser.FormattingEnabled = true;
            this.lstLibraryBrowser.Location = new System.Drawing.Point(0, 0);
            this.lstLibraryBrowser.Name = "lstLibraryBrowser";
            this.lstLibraryBrowser.Size = new System.Drawing.Size(763, 289);
            this.lstLibraryBrowser.TabIndex = 11;
            this.lstLibraryBrowser.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstLibraryBrowser_DrawItem);
            this.lstLibraryBrowser.DoubleClick += new System.EventHandler(this.lstLibraryBrowser_DoubleClick);
            // 
            // lblFilters
            // 
            this.lblFilters.AutoSize = true;
            this.lblFilters.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilters.Location = new System.Drawing.Point(3, 18);
            this.lblFilters.Name = "lblFilters";
            this.lblFilters.Size = new System.Drawing.Size(103, 17);
            this.lblFilters.TabIndex = 12;
            this.lblFilters.Text = "Library Contents";
            // 
            // lblLibraryList
            // 
            this.lblLibraryList.AutoSize = true;
            this.lblLibraryList.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblLibraryList.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLibraryList.Location = new System.Drawing.Point(715, 0);
            this.lblLibraryList.Name = "lblLibraryList";
            this.lblLibraryList.Size = new System.Drawing.Size(48, 17);
            this.lblLibraryList.TabIndex = 13;
            this.lblLibraryList.Text = "Library";
            // 
            // lblPlaylist
            // 
            this.lblPlaylist.AutoSize = true;
            this.lblPlaylist.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlaylist.Location = new System.Drawing.Point(235, 18);
            this.lblPlaylist.Name = "lblPlaylist";
            this.lblPlaylist.Size = new System.Drawing.Size(105, 17);
            this.lblPlaylist.TabIndex = 14;
            this.lblPlaylist.Text = "Currently Playing";
            // 
            // lblCurrentlyPlaying
            // 
            this.lblCurrentlyPlaying.AutoSize = true;
            this.lblCurrentlyPlaying.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Italic);
            this.lblCurrentlyPlaying.Location = new System.Drawing.Point(346, 18);
            this.lblCurrentlyPlaying.Name = "lblCurrentlyPlaying";
            this.lblCurrentlyPlaying.Size = new System.Drawing.Size(71, 17);
            this.lblCurrentlyPlaying.TabIndex = 15;
            this.lblCurrentlyPlaying.Text = "123456798";
            // 
            // picAlbumArt
            // 
            this.picAlbumArt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picAlbumArt.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.picAlbumArt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picAlbumArt.Location = new System.Drawing.Point(607, 3);
            this.picAlbumArt.Name = "picAlbumArt";
            this.picAlbumArt.Size = new System.Drawing.Size(153, 131);
            this.picAlbumArt.TabIndex = 16;
            this.picAlbumArt.TabStop = false;
            this.picAlbumArt.Visible = false;
            // 
            // tbLibraryNavigator
            // 
            this.tbLibraryNavigator.AutoSize = true;
            this.tbLibraryNavigator.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tbLibraryNavigator.ColumnCount = 2;
            this.tbLibraryNavigator.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.67732F));
            this.tbLibraryNavigator.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.32268F));
            this.tbLibraryNavigator.Controls.Add(this.tblLibraryContents, 0, 0);
            this.tbLibraryNavigator.Controls.Add(this.tblLibraryBrowserAndPlaylist, 1, 0);
            this.tbLibraryNavigator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLibraryNavigator.Location = new System.Drawing.Point(3, 44);
            this.tbLibraryNavigator.Name = "tbLibraryNavigator";
            this.tbLibraryNavigator.RowCount = 1;
            this.tbLibraryNavigator.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tbLibraryNavigator.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 659F));
            this.tbLibraryNavigator.Size = new System.Drawing.Size(1002, 659);
            this.tbLibraryNavigator.TabIndex = 17;
            // 
            // tblLibraryContents
            // 
            this.tblLibraryContents.ColumnCount = 1;
            this.tblLibraryContents.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblLibraryContents.Controls.Add(this.treeFilters, 0, 1);
            this.tblLibraryContents.Controls.Add(this.txtSearchBox, 0, 0);
            this.tblLibraryContents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblLibraryContents.Location = new System.Drawing.Point(3, 3);
            this.tblLibraryContents.Name = "tblLibraryContents";
            this.tblLibraryContents.RowCount = 2;
            this.tblLibraryContents.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 4F));
            this.tblLibraryContents.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 96F));
            this.tblLibraryContents.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblLibraryContents.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblLibraryContents.Size = new System.Drawing.Size(221, 653);
            this.tblLibraryContents.TabIndex = 17;
            // 
            // txtSearchBox
            // 
            this.txtSearchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearchBox.Location = new System.Drawing.Point(3, 3);
            this.txtSearchBox.Name = "txtSearchBox";
            this.txtSearchBox.Size = new System.Drawing.Size(215, 22);
            this.txtSearchBox.TabIndex = 11;
            this.txtSearchBox.TextChanged += new System.EventHandler(this.txtSearchBox_TextChanged);
            // 
            // tblLibraryBrowserAndPlaylist
            // 
            this.tblLibraryBrowserAndPlaylist.AutoSize = true;
            this.tblLibraryBrowserAndPlaylist.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tblLibraryBrowserAndPlaylist.ColumnCount = 1;
            this.tblLibraryBrowserAndPlaylist.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLibraryBrowserAndPlaylist.Controls.Add(this.pnlLibrary, 0, 1);
            this.tblLibraryBrowserAndPlaylist.Controls.Add(this.pnlPlaylist, 0, 0);
            this.tblLibraryBrowserAndPlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblLibraryBrowserAndPlaylist.Location = new System.Drawing.Point(230, 3);
            this.tblLibraryBrowserAndPlaylist.Name = "tblLibraryBrowserAndPlaylist";
            this.tblLibraryBrowserAndPlaylist.RowCount = 3;
            this.tblLibraryBrowserAndPlaylist.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 53.30033F));
            this.tblLibraryBrowserAndPlaylist.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 46.69967F));
            this.tblLibraryBrowserAndPlaylist.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblLibraryBrowserAndPlaylist.Size = new System.Drawing.Size(769, 653);
            this.tblLibraryBrowserAndPlaylist.TabIndex = 0;
            // 
            // pnlLibrary
            // 
            this.pnlLibrary.AutoSize = true;
            this.pnlLibrary.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlLibrary.Controls.Add(this.lblLibraryList);
            this.pnlLibrary.Controls.Add(this.lstLibraryBrowser);
            this.pnlLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLibrary.Location = new System.Drawing.Point(3, 340);
            this.pnlLibrary.Name = "pnlLibrary";
            this.pnlLibrary.Size = new System.Drawing.Size(763, 289);
            this.pnlLibrary.TabIndex = 19;
            // 
            // pnlPlaylist
            // 
            this.pnlPlaylist.AutoSize = true;
            this.pnlPlaylist.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlPlaylist.Controls.Add(this.picAlbumArt);
            this.pnlPlaylist.Controls.Add(this.lstPlaylist);
            this.pnlPlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPlaylist.Location = new System.Drawing.Point(3, 3);
            this.pnlPlaylist.Name = "pnlPlaylist";
            this.pnlPlaylist.Size = new System.Drawing.Size(763, 331);
            this.pnlPlaylist.TabIndex = 18;
            // 
            // lstPlaylist
            // 
            this.lstPlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstPlaylist.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstPlaylist.FormattingEnabled = true;
            this.lstPlaylist.Location = new System.Drawing.Point(0, 0);
            this.lstPlaylist.Name = "lstPlaylist";
            this.lstPlaylist.Size = new System.Drawing.Size(763, 331);
            this.lstPlaylist.TabIndex = 2;
            this.lstPlaylist.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstPlaylist_DrawItem);
            this.lstPlaylist.DoubleClick += new System.EventHandler(this.lstPlaylist_DoubleClick);
            // 
            // pnlPlaybackControls
            // 
            this.pnlPlaybackControls.Controls.Add(this.btnPlay);
            this.pnlPlaybackControls.Controls.Add(this.btnStop);
            this.pnlPlaybackControls.Controls.Add(this.btnClear);
            this.pnlPlaybackControls.Controls.Add(this.btnNext);
            this.pnlPlaybackControls.Controls.Add(this.btnPrevious);
            this.pnlPlaybackControls.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlPlaybackControls.Location = new System.Drawing.Point(766, 0);
            this.pnlPlaybackControls.Name = "pnlPlaybackControls";
            this.pnlPlaybackControls.Size = new System.Drawing.Size(236, 35);
            this.pnlPlaybackControls.TabIndex = 18;
            // 
            // pnlOverview
            // 
            this.pnlOverview.Controls.Add(this.lblCurrentlyPlaying);
            this.pnlOverview.Controls.Add(this.lblFilters);
            this.pnlOverview.Controls.Add(this.lblPlaylist);
            this.pnlOverview.Controls.Add(this.pnlPlaybackControls);
            this.pnlOverview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOverview.Location = new System.Drawing.Point(3, 3);
            this.pnlOverview.Name = "pnlOverview";
            this.pnlOverview.Size = new System.Drawing.Size(1002, 35);
            this.pnlOverview.TabIndex = 19;
            // 
            // tblMain
            // 
            this.tblMain.ColumnCount = 1;
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblMain.Controls.Add(this.pnlOverview, 0, 0);
            this.tblMain.Controls.Add(this.tbLibraryNavigator, 0, 1);
            this.tblMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblMain.Location = new System.Drawing.Point(0, 24);
            this.tblMain.Name = "tblMain";
            this.tblMain.RowCount = 2;
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.847953F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 94.15205F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblMain.Size = new System.Drawing.Size(1008, 706);
            this.tblMain.TabIndex = 20;
            // 
            // lblPlaybackTime
            // 
            this.lblPlaybackTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPlaybackTime.AutoSize = true;
            this.lblPlaybackTime.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblPlaybackTime.Location = new System.Drawing.Point(769, 7);
            this.lblPlaybackTime.Name = "lblPlaybackTime";
            this.lblPlaybackTime.Size = new System.Drawing.Size(0, 17);
            this.lblPlaybackTime.TabIndex = 19;
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.lblPlaybackTime);
            this.Controls.Add(this.tblMain);
            this.Controls.Add(this.mnuMain);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mnuMain;
            this.MinimumSize = new System.Drawing.Size(361, 761);
            this.Name = "MainView";
            this.Text = "Dukebox";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMainView_FormClosed);
            this.Load += new System.EventHandler(this.MainView_Load);
            this.Resize += new System.EventHandler(this.MainView_Resize);
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAlbumArt)).EndInit();
            this.tbLibraryNavigator.ResumeLayout(false);
            this.tbLibraryNavigator.PerformLayout();
            this.tblLibraryContents.ResumeLayout(false);
            this.tblLibraryContents.PerformLayout();
            this.tblLibraryBrowserAndPlaylist.ResumeLayout(false);
            this.tblLibraryBrowserAndPlaylist.PerformLayout();
            this.pnlLibrary.ResumeLayout(false);
            this.pnlLibrary.PerformLayout();
            this.pnlPlaylist.ResumeLayout(false);
            this.pnlPlaybackControls.ResumeLayout(false);
            this.pnlOverview.ResumeLayout(false);
            this.pnlOverview.PerformLayout();
            this.tblMain.ResumeLayout(false);
            this.tblMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.MenuStrip mnuMain;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuPlaylist;
        private System.Windows.Forms.ToolStripMenuItem mnuPlayback;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem playFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem addFilesToLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem loadFromFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.ToolStripMenuItem mnuShuffle;
        private System.Windows.Forms.ToolStripMenuItem mnuRepeat;
        private System.Windows.Forms.ToolStripMenuItem mnuRepeatAll;
        private System.Windows.Forms.TreeView treeFilters;
        private System.Windows.Forms.ListBox lstLibraryBrowser;
        private System.Windows.Forms.Label lblFilters;
        private System.Windows.Forms.Label lblLibraryList;
        private System.Windows.Forms.Label lblPlaylist;
        private System.Windows.Forms.ToolStripMenuItem mnuPlayFolder;
        private System.Windows.Forms.Label lblCurrentlyPlaying;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem mnuImportPlaylist;
        private System.Windows.Forms.PictureBox picAlbumArt;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem audioCDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuPlayCd;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem ripTrackToMP3ToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tbLibraryNavigator;
        private System.Windows.Forms.TableLayoutPanel tblLibraryBrowserAndPlaylist;
        private System.Windows.Forms.Panel pnlPlaylist;
        private System.Windows.Forms.Panel pnlLibrary;
        private System.Windows.Forms.Panel pnlPlaybackControls;
        private System.Windows.Forms.Panel pnlOverview;
        private System.Windows.Forms.TableLayoutPanel tblMain;
        private System.Windows.Forms.Label lblPlaybackTime;
        private System.Windows.Forms.TableLayoutPanel tblLibraryContents;
        private System.Windows.Forms.TextBox txtSearchBox;
        private BufferedListBox lstPlaylist;
    }
}

