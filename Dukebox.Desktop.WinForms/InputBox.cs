using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dukebox.Desktop
{
    public static class InputBox
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static DialogResult Show(string message, string title, ref string outputText)
        {
            try
            {
                var okClicked = false;
                var outputTextString = string.Empty;

                var form = new InputBoxForm(message, title, (input) => 
                {
                    okClicked = true;
                    outputTextString = input;
                });

                form.ShowDialog();

                try
                {
                    if (!form.IsDisposed)
                    {
                        form.Dispose();
                    }
                }
                catch
                {
                }

                outputText = outputTextString;

                return okClicked ? DialogResult.OK : DialogResult.Cancel;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error while running input dialog box with title '{0}'", title), ex);
                return DialogResult.Abort;
            }
        }
    }
}
