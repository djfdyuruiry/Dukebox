using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace Dukebox.Desktop.ViewModel
{
    public class PlaybackMenuViewModel : ViewModelBase, IPlaybackMenuViewModel
    {
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly IDukeboxUserSettings _userSettings;

        private bool _shuffleOn;
        private bool _repeatOn;
        private bool _repeatAllOn;

        public ICommand Shuffle { get; private set; }
        public ICommand Repeat { get; private set; }
        public ICommand RepeatAll { get; private set; }

        public bool ShuffleOn
        {
            get
            {
                return _shuffleOn;
            }
            set
            {
                _shuffleOn = value;
                OnPropertyChanged("ShuffleOn");

                _userSettings.Shuffle = value;
                _audioPlaylist.Shuffle = value;
            }
        }
        public bool RepeatOn
        {
            get
            {
                return _repeatOn;
            }
            set
            {
                _repeatOn = value;
                OnPropertyChanged("RepeatOn");

                _userSettings.Repeat = value;
                _audioPlaylist.Repeat = value;
            }
        }
        public bool RepeatAllOn
        {
            get
            {
                return _repeatAllOn;
            }
            set
            {
                _repeatAllOn = value;
                OnPropertyChanged("RepeatAllOn");
                
                _userSettings.RepeatAll = value;
                _audioPlaylist.RepeatAll = value;
            }
        }

        public PlaybackMenuViewModel(IAudioPlaylist audioPlaylist, IDukeboxUserSettings userSettings) : base()
        {
            _audioPlaylist = audioPlaylist;
            _userSettings = userSettings;

            ShuffleOn = _userSettings.Shuffle;
            RepeatOn = _userSettings.Repeat;
            RepeatAllOn = _userSettings.RepeatAll;
            
            Shuffle = new RelayCommand(() => 
            {
                audioPlaylist.Shuffle = !ShuffleOn;
                ShuffleOn = !ShuffleOn;
            });
            Repeat = new RelayCommand(() => 
            {
                audioPlaylist.Shuffle = !RepeatOn;
                RepeatOn = !RepeatOn;
            });
            RepeatAll = new RelayCommand(() => 
            {
                audioPlaylist.Shuffle = !RepeatAllOn;
                RepeatAllOn = !RepeatAllOn;
            });
        }
    }
}
