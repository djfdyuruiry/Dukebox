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
        private static SettingsBase settings = Assembly.Load("Dukebox.Desktop").GetTypes()
                                                    .Where((t) => t.Name == "Settings" && typeof(SettingsBase).IsAssignableFrom(t))
                                                    .Select((t) => (SettingsBase)t.GetProperty("Default").GetValue( null, null ))
                                                    .FirstOrDefault();

        public int AddDirectoryConcurrencyLimit
        {
            get
            {
                return int.Parse(settings["addDirectoryConcurrencyLimit"].ToString());
            }
        }

        public string AlbumArtCachePath
        {
            get
            {
                return settings["albumArtCachePath"].ToString();
            }
        }
        public string TrackDisplayFormat
        {
            get
            {
                return settings["trackDisplayFormat"].ToString();
            }
        }
    }
}
