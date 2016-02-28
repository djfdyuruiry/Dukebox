using Dukebox.Audio;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Un4seen.Bass;

namespace Dukebox.Desktop
{
    /// <summary>
    /// 
    /// </summary>
    static class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /// <summary>
        /// The main entry point for the Dukebox application.
        /// Load BASS audio library and display main form.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SplashScreenView splashView = null;

            try
            {   
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                splashView = new SplashScreenView();
                Application.Run(splashView);
            }
            catch (ObjectDisposedException ex)
            {
                HandleFatalException(ex, splashView == null ? null : splashView.MainView);
            }
            catch (Exception ex)
            {
                HandleFatalException(ex, splashView == null ? null : splashView.MainView);
            }
            
        }

        /// <summary>
        /// Report an exception, clean up the main program window and exit.
        /// </summary>
        /// <param name="ex">The exception raised.</param>
        /// <param name="view">Main program window.</param>
        private static void HandleFatalException(Exception ex, Form view)
        {
            if (view != null)
            {
                view.Hide();

                if (!view.IsDisposed)
                {
                    view.Close();
                    view.Dispose();
                }
            }

            if (MediaPlayer.GetInstance().Playing)
            {
                MediaPlayer.GetInstance().StopAudio();
            }

            var msg = "An error has forced Dukebox to stop";
            _logger.Info(msg, ex);

            MessageBox.Show(string.Format("{0}: {1}", msg, ex.Message), "Dukebox: Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            Application.Exit();
            Environment.Exit(-1);
        }

        /// <summary>
        /// Initalise the BASS audio library and
        /// log any plug-ins loaded. The number of plugins
        /// loaded is checked against the application setting 
        /// 'Dukebox.Desktop.Properties.Settings.Default.bassPluginCount'.
        /// </summary>
        /// <exception cref="TypeInitializationException.TypeInitializationException">if there was an error loading 'bass.dll' or a BASS Plug-In library.</exception>
        public static void InitaliseBassLibrary()
        {
            Dictionary<int, string> bassPluginsLoaded = new Dictionary<int,string>();
            
            // Register freeware license details.
            BassNet.Registration(Dukebox.Desktop.Properties.Settings.Default.bassLicenseEmail, Dukebox.Desktop.Properties.Settings.Default.bassLicenseKey);

            // Load BASS library with default output device and add-on DLLs.           
            bool bassInit = Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            bassPluginsLoaded = Bass.BASS_PluginLoadDirectory(Dukebox.Desktop.Properties.Settings.Default.bassAddOnsPath);

            // Make sure the BASS library initalised correctly and all available plugins where loaded successfully.
            if (!bassInit || bassPluginsLoaded == null || bassPluginsLoaded.Count < Dukebox.Desktop.Properties.Settings.Default.bassPluginCount)
            {
                string msg = string.Format("Error loading 'bass.dll' or a BASS Plug-In library! [BASS error code: {0}]", Bass.BASS_ErrorGetCode().ToString());

                throw new TypeInitializationException("Un4seen.Bass.Bass", new Exception(msg));
            }

            LogBassPluginsLoaded(bassPluginsLoaded);
        }

        /// <summary>
        /// Get all the supported streaming extensions from the
        /// BASS library core and all add-ons. These are stored
        /// in the static field 'Dukebox.Audio.AudioFileFormats.SupportedFormats'.
        /// </summary>
        public static void RegisterSupportedAudioFileFormats()
        {
            // Formats supported by the BASS library core.
            AudioFileFormats.SupportedFormats = Bass.SupportedStreamExtensions.Replace("*", string.Empty).Split(';').ToList();

            // Formats supported by the BASS library Add-Ons.
            var addonClasses = Assembly.GetAssembly(typeof(Bass)).GetTypes().Where(t => 
            {
                return t.IsClass && t.Name != "Bass" && t.Name != "BassDShow" && t.GetField("SupportedStreamExtensions") != null;
            });
           
            foreach (Type addon in addonClasses)
            {
                string supportedStreamExtensions = ((string)addon.GetField("SupportedStreamExtensions").GetValue(null));
                AudioFileFormats.SupportedFormats.AddRange(supportedStreamExtensions.Replace("*", string.Empty).Split(';').ToList());
            }
        }

        /// <summary>
        /// Log the plugins that where loaded by the Bass 
        /// 'Bass.BASS_PluginLoadDirectory' method.
        /// </summary>
        /// <param name="bassPluginsLoaded">A dictionary containing the handle for each plugin and it's name.</param>
        private static void LogBassPluginsLoaded(Dictionary<int, string> bassPluginsLoaded)
        {
            // Log bass add-ons found.
            string logData = bassPluginsLoaded.Count() + "Bass add-on DLLs found (Format => {$handle, $name}): ";

            foreach (KeyValuePair<int, string> kvp in bassPluginsLoaded)
            {
                logData += string.Format("{{{0},{1}}} ", kvp.Key, kvp.Value);
            }

            _logger.Info(logData);
        }

        /// <summary>
        /// Pre-load all required .NET assemblies.
        /// </summary>
        public static void PreloadAssemblies()
        {
            try
            {
                // Preload IKVM Java runtime & jaudiotagger.
                var ikvmAssemblies = new String[]{"IKVM.Runtime", "IKVM.OpenJDK.Core", "IKVM.OpenJDK.Text", "IKVM.OpenJDK.Util", "jaudiotagger-2.2.0"};
                ikvmAssemblies.Select(asm => Assembly.Load(asm));

                // Load other referenced assemblies next.
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

                // Ignore these DLLs.
                var forbiddenDlls = new String[]{"Bass.dll", "OptimFROG.dll"};

                var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
                var notLoadedPaths = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase));

                foreach (var path in notLoadedPaths)
                {
                    if (!forbiddenDlls.Any(fdll => path.EndsWith(fdll, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path)));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("Could not preload Dukebox assemblies.", ex);
            }
        }
    }
}
