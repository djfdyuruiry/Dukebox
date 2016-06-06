using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.ViewModel
{
    public class AudioCdMenuViewModel : ViewModelBase, IAudioCdMenuViewModel
    {        
        private readonly IMusicLibrary _musicLibrary;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly IAudioCdRippingService _cdRippingService;

        public ICommand PlayAudioCd { get; private set; }
        public ICommand RipCdToFolder { get; private set; }

        public AudioCdMenuViewModel(IMusicLibrary musicLibrary, IAudioPlaylist audioPlaylist, IAudioCdRippingService cdRippingService) : base()
        {
            _musicLibrary = musicLibrary;
            _audioPlaylist = audioPlaylist;
            _cdRippingService = cdRippingService;

            PlayAudioCd = new RelayCommand(DoPlayAudioCd);
            RipCdToFolder = new RelayCommand(() => AudioCdHelper.RipCdToFolder(_cdRippingService));
        }

        private void DoPlayAudioCd()
        {
            var audioCdDrivePath = AudioCdHelper.BrowseForAudioCdDrive();

            if (string.IsNullOrEmpty(audioCdDrivePath))
            {
                return;
            }

            AudioCdHelper.PlayAudioCd(audioCdDrivePath, _musicLibrary, _audioPlaylist);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }
    }
}
