using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dukebox
{
    /// <summary>
    /// Listbox which has graphics buffering enabled.
    /// </summary>
    public class BufferedListBox : ListBox
    {
        /// <summary>
        /// Set paint styles and double buffering.
        /// </summary>
        public BufferedListBox() : base()
        {
            SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                     System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer|
                     System.Windows.Forms.ControlStyles.ContainerControl,
                     true);

            DoubleBuffered = true;
        }
    }
}
