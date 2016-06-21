using System.Collections.Generic;
using System.Linq;
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
                return _settings.shuffle;
            }
            set
            {
                _settings.shuffle = value;
                _settings.Save();
            }
        }

        public bool Repeat
        {
            get
            {
                return _settings.repeat;
            }
            set
            {
                _settings.repeat = value;
                _settings.Save();
            }
        }

        public bool RepeatAll
        {
            get
            {
                return _settings.repeatAll;
            }
            set
            {
                _settings.repeatAll = value;
                _settings.Save();
            }
        }

        public List<string> ExtendedMetadataColumnsToShow
        {
            get
            {
                return _settings.extendedMetadataColumnsToShow.Split(',').ToList();
            }
            set
            {
                _settings.extendedMetadataColumnsToShow = string.Join(",", value.ToArray());
                _settings.Save();
            }
        }

        public DukeboxUserSettings()
        {
            _settings = Settings.Default;
        }
    }
}
