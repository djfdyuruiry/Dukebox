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
    public partial class InputBoxForm : Form
    {
        private Action _onExitOrCancel;
        private Action<string> _onOk;

        public InputBoxForm()
        {
            InitializeComponent();
        }

        public InputBoxForm(string message, string title, Action<string> onOk, Action onExitOrCancel = null) : this()
        {
            if (message == null)
            {
                throw new ArgumentException("message");
            }
            if (title == null)
            {
                throw new ArgumentException("title");
            }

            lblMessage.Text = message;
            Text = title;

            _onExitOrCancel = onExitOrCancel;
            _onOk = onOk;
        }

        private void CloseForm(object sender, object eventArgs)
        {
            if (_onExitOrCancel != null)
            {
                _onExitOrCancel();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (_onOk != null)
            {
                _onOk(txtInput.Text);
            }

            Close();
        }

        private void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            var key = (Keys)char.ToUpper(e.KeyChar);

            if (key == Keys.Enter)
            {
                btnOk.PerformClick();
            }
            else if (key == Keys.Escape)
            {
                btnCancel.PerformClick();
            }
        }
    }
}
