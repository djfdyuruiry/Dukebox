using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dukebox.Desktop
{
    public partial class SplashScreenView : Form
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private MainView _mainView;
        private Stopwatch _programLoadStopwatch;
        private delegate void ActionDelegate();

        public SplashScreenView()
        {            
            InitializeComponent();
        }
        
        // Make background of splash screen form transparent by painting no background.
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        private void SplashScreenView_Load(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                _programLoadStopwatch = Stopwatch.StartNew();

                // Warm up dependencies and register available audio formats.
                Program.InitaliseBassLibrary();
                Program.PreloadAssemblies();
                Program.RegisterSupportedAudioFileFormats();

                // Create main view, run CloseAndShowMainView as callback when main view is ready.
                Invoke(new ActionDelegate(() =>
                {
                    _mainView = new MainView();
                    _mainView.Init(CloseAndShowMainView);
                }));
            }).Start();
        }

        /// <summary>
        /// Hide the splash screen and show the main view.
        /// </summary>
        public void CloseAndShowMainView()
        {
            Invoke(new ActionDelegate(() =>
            {
                Hide();

                _programLoadStopwatch.Stop();
                _logger.InfoFormat("Loading Dukebox took {0}ms.", _programLoadStopwatch.ElapsedMilliseconds);

                _mainView.Show();
            }));
        }
    }
}
