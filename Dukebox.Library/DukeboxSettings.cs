using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Library
{
    /// <summary>
    /// Workaround for circular dependancy to lookup
    /// executing assemblies config.
    /// </summary>
    public static class DukeboxAssemblyLoader
    {
        // Fetch the settings object from foreign assembly.
        private static SettingsBase _settings = Assembly.Load("Dukebox.Desktop").GetTypes()
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
    }
}
