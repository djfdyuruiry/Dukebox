using GalaSoft.MvvmLight.Command;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.ViewModel
{
    public class AudioCdMenuViewModel : ViewModelBase, IAudioCdMenuViewModel
    {
        public const string AudioCdBrowserPrompt = "Select Audio CD Drive";
        public const string Mp3OutputBrowserPrompt = "Select Output Folder for MP3 Files";
        public const string DriveRootRegex = @"[a-zA-Z]:\\";

        private readonly IMusicLibrary _musicLibrary;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly IAudioCdRippingService _cdRippingService;

        private FolderBrowserDialog _selectAudioCdDriveDialog;
        private FolderBrowserDialog _selectMp3OutputDialog;
        private Regex _driveRootRegex;

        public ICommand PlayAudioCd { get; private set; }
        public ICommand RipCdToFolder { get; private set; }

        public AudioCdMenuViewModel(IMusicLibrary musicLibrary, IAudioPlaylist audioPlaylist, IAudioCdRippingService cdRippingService) : base()
        {
            _musicLibrary = musicLibrary;
            _audioPlaylist = audioPlaylist;
            _cdRippingService = cdRippingService;

            _selectAudioCdDriveDialog = new FolderBrowserDialog();
            _selectMp3OutputDialog = new FolderBrowserDialog();
            _driveRootRegex = new Regex(DriveRootRegex);

            _selectAudioCdDriveDialog.Description = AudioCdBrowserPrompt;
            _selectMp3OutputDialog.Description = Mp3OutputBrowserPrompt;

            PlayAudioCd = new RelayCommand(DoPlayAudioCd);
            RipCdToFolder = new RelayCommand(DoRipCdToFolder);
        }

        private void DoPlayAudioCd()
        {
            var audioCdDrive = BrowseForAudioCdDrive();

            if (string.IsNullOrEmpty(audioCdDrive))
            {
                return;
            }

            var tracks = _musicLibrary.GetTracksForDirectory(audioCdDrive, false);
            _audioPlaylist.LoadPlaylistFromList(tracks);
        }

        private void DoRipCdToFolder()
        {
            var audioCdDrive = BrowseForAudioCdDrive();

            if (string.IsNullOrEmpty(audioCdDrive))
            {
                return;
            }

            var dialogResult = _selectMp3OutputDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            // todo: create ICdRipViewUpdater implementation and use
            _cdRippingService.RipCdToFolder(audioCdDrive, _selectMp3OutputDialog.SelectedPath, null); 
        }

        private string BrowseForAudioCdDrive()
        {
            var dialogResult = _selectAudioCdDriveDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return string.Empty;
            }

            var pathIsDriveRoot = _driveRootRegex.IsMatch(_selectAudioCdDriveDialog.SelectedPath);

            if (!pathIsDriveRoot)
            {
                // todo: notify user that path selected is not a drive root
                return string.Empty;
            }

            return _selectAudioCdDriveDialog.SelectedPath;
        }
    }
}
