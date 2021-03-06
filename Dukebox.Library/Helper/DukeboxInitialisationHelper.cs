﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using Un4seen.Bass;
using Dukebox.Audio;
using Dukebox.Configuration.Interfaces;

namespace Dukebox.Library.Helper
{
    public class DukeboxInitialisationHelper
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDukeboxSettings _settings;
        private readonly AudioFileFormats _audioFormats;

        public AudioFileFormats AudioFileFormats
        {
            get
            {
                return _audioFormats;
            }
        }

        public DukeboxInitialisationHelper(IDukeboxSettings settings, AudioFileFormats audioFileFormats)
        {
            _settings = settings;
            _audioFormats = audioFileFormats;
        }

        public void InitaliseAudioLibrary()
        {
            // Register freeware license details.
            BassNet.Registration(_settings.BassLicenseEmail, _settings.BassLicenseKey);

            // Load BASS library with default output device and add-on DLLs.           
            var bassInit = Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            var bassPluginsLoaded = Bass.BASS_PluginLoadDirectory(_settings.BassAddOnsPath);

            // Make sure the BASS library initalised correctly and plugins where loaded successfully.
            if (!bassInit || bassPluginsLoaded == null)
            {
                var msg = string.Format("Error loading 'bass.dll' or the BASS Plug-In library! [BASS error code: {0}]", Bass.BASS_ErrorGetCode().ToString());
                throw new TypeInitializationException("Un4seen.Bass.Bass", new Exception(msg));
            }

            if (bassPluginsLoaded.Count < _settings.BassPluginCount)
            {
                logger.WarnFormat("Number of plugins loaded by BASS was lower than expected: {0} expected, {1} actually loaded", 
                    bassPluginsLoaded.Count, _settings.BassPluginCount);
            }

            LogBassPluginsLoaded(bassPluginsLoaded);
        }

        /// <summary>
        /// Log the plugins that where loaded by the Bass 
        /// 'Bass.BASS_PluginLoadDirectory' method.
        /// </summary>
        /// <param name="bassPluginsLoaded">A dictionary containing the handle for each plugin and it's name.</param>
        private void LogBassPluginsLoaded(Dictionary<int, string> bassPluginsLoaded)
        {
            // Log bass add-ons found.
            var logStreamBuilder = new StringBuilder();
            logStreamBuilder.AppendFormat("{0} {1}", bassPluginsLoaded.Count, "Bass add-on DLLs found (Format => {$handle, $name}): ");

            foreach (KeyValuePair<int, string> kvp in bassPluginsLoaded)
            {
                logStreamBuilder.AppendFormat("{{{0},{1}}} ", kvp.Key, kvp.Value);
            }

            logger.Info(logStreamBuilder.ToString());
        }

        /// <summary>
        /// Get all the supported streaming extensions from the
        /// BASS library core and all add-ons. These are stored
        /// in the static field 'Dukebox.Audio.AudioFileFormats.SupportedFormats'.
        /// </summary>
        public void RegisterSupportedAudioFileFormats()
        {
            // Formats supported by the BASS library core.
            var coreSupportedFormats = Bass.SupportedStreamExtensions.Replace("*", string.Empty).Split(';');
            _audioFormats.SupportedFormats.AddRange(coreSupportedFormats);

            // Formats supported by the BASS library Add-Ons.
            var addonClasses = Assembly.GetAssembly(typeof(Bass)).GetTypes().Where(t =>
            {
                return t.IsClass && t.Name != "Bass" && t.Name != "BassDShow" && t.GetField("SupportedStreamExtensions") != null;
            });

            foreach (Type addon in addonClasses)
            {
                string supportedStreamExtensions = ((string)addon.GetField("SupportedStreamExtensions").GetValue(null));
                _audioFormats.SupportedFormats.AddRange(supportedStreamExtensions.Replace("*", string.Empty).Split(';').ToList());
            }

            _audioFormats.SignalFormatsHaveBeenLoaded();
        }
    }
}
