using Dukebox.Library.CdRipping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dukebox.Desktop
{
    public partial class ProgressMonitorBox : Form, ICdRipViewUpdater
    {
        public ProgressMonitorBox()
        {
            InitializeComponent();
        }

        
    }
}
