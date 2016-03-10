using Dukebox.Library.Interfaces;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Dukebox.Library.Config
{
    /// <summary>
    /// Workaround for circular dependancy to lookup
    /// executing assemblies config.
    /// </summary>
    public class DukeboxSettings : IDukeboxSettings
    {
        // Fetch the settings object from foreign assembly.
        private KeyValueConfigurationCollection _settings;

        public DukeboxSettings()
        {
            var configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = @"Dukebox.exe.Config";

            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            _settings = config.AppSettings.Settings;
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
        public string TrackDisplayFormat
        {
            get
            {
                return _settings["trackDisplayFormat"].Value;
            }
        }
    }
}
