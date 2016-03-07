using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Desktop
{
    public class DukeboxUserSettings : IDukeboxUserSettings
    {
        private Settings _settings;

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

        public string TrackDisplayFormat
        {
            get
            {
                return _settings.trackDisplayFormat;
            }
            set
            {
                _settings.trackDisplayFormat = value;
                _settings.Save();
            }
        }

        public DukeboxUserSettings()
        {
            _settings = Dukebox.Desktop.Properties.Settings.Default;
        }
    }
}
