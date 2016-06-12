using System.Configuration;
using Dukebox.Configuration.Helper;
using Dukebox.Configuration.Interfaces;

namespace Dukebox.Configuration
{
    public class DukeboxSettings : IDukeboxSettings
    {
        // Fetch the settings object from foreign assembly.
        private readonly KeyValueConfigurationCollection _settings;
        
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
                return _settings["albumArtCachePath"].Value;
            }
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
