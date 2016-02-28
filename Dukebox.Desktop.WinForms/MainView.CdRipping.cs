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
using Dukebox.Library.CdRipping;

namespace Dukebox.Desktop
{
    public partial class MainView : Form
    {
        private void ripCdToMP3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog cdFolderDialog = new FolderBrowserDialog();
            FolderBrowserDialog outputFolderDialog = new FolderBrowserDialog();

            cdFolderDialog.Description = "Please select CD drive";
            outputFolderDialog.Description = "Please select folder to save MP3 files";

            if (cdFolderDialog.ShowDialog() == DialogResult.OK)
            {
                if (outputFolderDialog.ShowDialog() == DialogResult.OK)
                {
                    CdRipHelper cdRipHelper = new CdRipHelper();
                    var monitorBox = new ProgressMonitorBox();

                    (new Thread(() => cdRipHelper.RipCdToFolder(cdFolderDialog.SelectedPath, outputFolderDialog.SelectedPath, monitorBox))).Start();
                }
            }
        }
    }
}
