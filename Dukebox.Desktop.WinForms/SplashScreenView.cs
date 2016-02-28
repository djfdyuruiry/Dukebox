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

        private Stopwatch _programLoadStopwatch;
        private delegate void ActionDelegate();

        public MainView MainView { get; private set; }

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
                ShowNotification("Loading BASS audio...");
                Program.InitaliseBassLibrary();

                ShowNotification("Loading .NET libraries...");
                Program.PreloadAssemblies();

                ShowNotification("Registering supported audio formats...");
                Program.RegisterSupportedAudioFileFormats();

                // Create main view, run CloseAndShowMainView as callback when main view is ready.
                Invoke(new ActionDelegate(() =>
                {
                    MainView = new MainView();

                    ShowNotification("Scanning media library...");
                    MainView.Init(CloseAndShowMainView);
                }));
            }).Start();
        }

        public void ShowNotification(string notification)
        {
            Invoke(new ActionDelegate(() =>
            {
                if (!string.IsNullOrEmpty(notification))
                {
                    lblNotification.Text = notification;
                }
             }));
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

                MainView.Show();
            }));
        }
    }
}
