using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library.Config
{
    /// <summary>
    /// Workaround for circular dependancy to lookup
    /// executing assemblies config.
    /// </summary>
    public static class DukeboxSettings
    {
        // Fetch the settings object from foreign assembly.
        private static SettingsBase _settings = Assembly.Load("Dukebox").GetTypes()
                                                    .Where((t) => t.Name == "Settings" && typeof(SettingsBase).IsAssignableFrom(t))
                                                    .Select((t) => (SettingsBase)t.GetProperty("Default").GetValue( null, null ))
                                                    .FirstOrDefault();

        public static SettingsBase Settings
        {
            get
            {
                return _settings;
            }
        }

        public static bool IsConfigSettingValidString(string settingName)
        {
            return Settings[settingName] != null && !string.IsNullOrWhiteSpace(Settings[settingName].ToString());
        }

        public static string GetSettingAsString(string settingName)
        {
            if (!IsConfigSettingValidString(settingName))
            {
                throw new ConfigurationException(string.Format("Config setting {0} does not exist or is an empty string in the current App.config file.", settingName));
            }

            return Settings[settingName].ToString();
        }
    }
}
