using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Desktop.Views;
using Dukebox.Desktop.Services;
using System.Threading.Tasks;

namespace Dukebox.Desktop.ViewModel
{
    public class AudioCdMenuViewModel : ViewModelBase, IAudioCdMenuViewModel
    {
        public const string AudioCdBrowserPrompt = "Select Audio CD Drive";
        public const string Mp3OutputBrowserPrompt = "Select Output Folder for MP3 Files";
        public const string RipCdTitle = "Ripping Audio CD";
        public const string DriveRootRegex = @"^[a-zA-Z]:\\$";
        
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

            var progressWindow = new ProgressMonitor();
            var progressViewModel = new CdRippingProgressMonitorViewModel();

            progressWindow.DataContext = progressViewModel;
            progressViewModel.Title = RipCdTitle;

            progressViewModel.OnComplete += (o, e) => progressWindow.Dispatcher.InvokeAsync(progressWindow.Hide);

            Task.Run(() => _cdRippingService.RipCdToFolder(audioCdDrive, _selectMp3OutputDialog.SelectedPath, progressViewModel));

            progressWindow.Show();
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
                MessageBox.Show("Selected path is not a drive root.", "Error Selecting Audio CD", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }

            return _selectAudioCdDriveDialog.SelectedPath;
        }
    }
}
