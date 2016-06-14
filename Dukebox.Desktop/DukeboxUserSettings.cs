using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Properties;

namespace Dukebox.Desktop
{
    public class DukeboxUserSettings : IDukeboxUserSettings
    {
        private readonly Settings _settings;

        public bool Shuffle 
        {
            get
            {
                return bool.Parse(_settings.shuffle);
            }
            set
            {
                _settings.shuffle = value.ToString();
                _settings.Save();
            }
        }

        public bool Repeat
        {
            get
            {
                return bool.Parse(_settings.repeat);
            }
            set
            {
                _settings.repeat = value.ToString();
                _settings.Save();
            }
        }

        public bool RepeatAll
        {
            get
            {
                return bool.Parse(_settings.repeatAll);
            }
            set
            {
                _settings.repeatAll = value.ToString();
                _settings.Save();
            }
        }

        public DukeboxUserSettings()
        {
            _settings = Dukebox.Desktop.Properties.Settings.Default;
        }
    }
}
