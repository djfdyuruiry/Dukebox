using Dukebox.Audio;
using Dukebox.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Un4seen.Bass;

namespace Dukebox
{
    /// <summary>
    /// 
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the Dukebox application.
        /// Load BASS audio library and display main form.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MainView view = null;

            try
            {
                // Init BASS audio library.
                InitaliseBassLibrary();
                RegisterSupportedAudioFileFormats();

                // Display the main form.
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run((view = new MainView()));
            }
            catch (ObjectDisposedException ex)
            {
                Error(ex, ref view);
            }
            catch (Exception ex)
            {
                Error(ex, ref view);
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="view"></param>
        static void Error(Exception ex, ref MainView view)
        {
            view.Hide();
            view.Dispose();

            // Failed to load the bass library or one it's plug-ins.
            Logger.log(ex.Message, false);

            MessageBox.Show(ex.Message, "Dukebox: Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        /// <summary>
        /// Initalise the BASS audio library and
        /// log any plug-ins loaded. The number of plugins
        /// loaded is checked against the application setting 
        /// 'Dukebox.Properties.Settings.Default.bassPluginCount'.
        /// </summary>
        /// <exception cref="TypeInitializationException.TypeInitializationException">if there was an error loading 'bass.dll' or a BASS Plug-In library.</exception>
        private static void InitaliseBassLibrary()
        {
            Dictionary<int, string> bassPluginsLoaded = new Dictionary<int,string>();
            
            // Register freeware license details.
            BassNet.Registration(Dukebox.Properties.Settings.Default.bassLicenseEmail, Dukebox.Properties.Settings.Default.bassLicenseKey);

            // Load BASS library with default output device and add-on DLLs.
            bool bassInit = Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            bassPluginsLoaded = Bass.BASS_PluginLoadDirectory(Dukebox.Properties.Settings.Default.bassAddOnsPath);

            // Make sure the BASS library initalised correctly and all available plugins where loaded successfully.
            if (!bassInit || bassPluginsLoaded == null || bassPluginsLoaded.Count < Dukebox.Properties.Settings.Default.bassPluginCount)
            {
                string msg = "Error loading 'bass.dll' or a BASS Plug-In library! [BASS error code: " + Bass.BASS_ErrorGetCode().ToString() + "]";

                throw new TypeInitializationException("Un4seen.Bass.Bass", new Exception(msg));
            }

            LogBassPluginsLoaded(bassPluginsLoaded);
        }

        /// <summary>
        /// Get all the supported streaming extensions from the
        /// BASS library core and all add-ons. These are stored
        /// in the static field 'Dukebox.Audio.AudioFileFormats.SupportedFormats'.
        /// </summary>
        private static void RegisterSupportedAudioFileFormats()
        {
            // Formats supported by the BASS library core.
            AudioFileFormats.SupportedFormats = Bass.SupportedStreamExtensions.Replace("*", string.Empty).Split(';').ToList();

            // Formats supported by the BASS library Add-Ons.
            List<Type> addonClasses = Assembly.GetAssembly(typeof(Bass)).GetTypes().Where(t => t.IsClass && t.Name != "Bass" && t.Name != "BassDShow" && t.GetField("SupportedStreamExtensions") != null).ToList();
            foreach (Type addon in addonClasses)
            {
                string supportedStreamExtensions = ((string)addon.GetRuntimeField("SupportedStreamExtensions").GetValue(null));
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
            string logData = bassPluginsLoaded.Count() + " Bass add-on DLLs found (Format => {$handle, $name}): ";

            foreach (KeyValuePair<int, string> kvp in bassPluginsLoaded)
            {
                logData += "{" + kvp.Key + "," + kvp.Value + "} ";
            }

            Logger.log(logData);
        }
    }
}
