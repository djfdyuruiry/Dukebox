using System.Configuration;
using Dukebox.Configuration.Helper;
using Dukebox.Configuration.Interfaces;
using System.IO;
using System;

namespace Dukebox.Configuration
{
    public class DukeboxSettings : IDukeboxSettings
    {
        // Fetch the settings object from foreign assembly.
        private readonly KeyValueConfigurationCollection _settings;
        private bool _albumArtdebugSetupComplete;


        public DukeboxSettings()
        {
            var configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = @"Dukebox.Config";

            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            _settings = config.AppSettings.Settings;

            _settings["bassLicenseEmail"].Value = Base64Decoder.DoubleBase64Decode(_settings["bassLicenseEmail"].Value);
            _settings["bassLicenseKey"].Value = Base64Decoder.DoubleBase64Decode(_settings["bassLicenseKey"].Value);
        }


        public int AddDirectoryConcurrencyLimit
        {
            get
            {
                return int.Parse(_settings["addDirectoryConcurrencyLimit"].Value);
            }
        }

        public string AlbumArtCachePath
        {
            get
            {
#if DEBUG
                return ArtCacheDebugSetup();
#endif

                return _settings["albumArtCachePath"].Value;
            }
        }
                
        private string ArtCacheDebugSetup()
        {
            var absolutePath = Path.Combine(Environment.CurrentDirectory, "albumArtCache");

            if (!_albumArtdebugSetupComplete)
            {
                if (Directory.Exists(absolutePath))
                {
                    Directory.Delete(absolutePath, true);
                }

                _albumArtdebugSetupComplete = true;
            }

            return absolutePath;
        }


        public string BassAddOnsPath
        {
            get
            {
                return _settings["bassAddOnsPath"].Value;
            }
        }

        public string BassLicenseEmail
        {
            get
            {
                return _settings["bassLicenseEmail"].Value;
            }
        }

        public string BassLicenseKey
        {
            get
            {
                return _settings["bassLicenseKey"].Value;
            }
        }

        public int BassPluginCount
        {
            get
            {
                return int.Parse(_settings["bassPluginCount"].Value);
            }
        }

        public string TrackDisplayFormat
        {
            get
            {
                return _settings["trackDisplayFormat"].Value;
            }
        }
    }
}
