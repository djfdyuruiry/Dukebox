using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Dukebox.Desktop.ViewModel;
using Dukebox.Desktop.Views;
using Dukebox.Library.Interfaces;

namespace Dukebox.Desktop.Helper
{
    public static class AudioCdHelper
    {
        private const string audioCdBrowserPrompt = "Select Audio CD Drive";
        private const string mp3OutputBrowserPrompt = "Select Output Folder for MP3 Files";
        private const string ripCdTitle = "Ripping Audio CD";
        private const string driveRootRegexPattern = @"^[a-zA-Z]:\\$";

        private static readonly Regex driveRootRegex;
        private static readonly FolderBrowserDialog selectAudioCdDriveDialog;
        private static readonly FolderBrowserDialog selectMp3OutputDialog;

        static AudioCdHelper()
        {
            driveRootRegex = new Regex(driveRootRegexPattern);
            selectAudioCdDriveDialog = new FolderBrowserDialog();
            selectMp3OutputDialog = new FolderBrowserDialog();

            selectAudioCdDriveDialog.Description = audioCdBrowserPrompt;
            selectMp3OutputDialog.Description = mp3OutputBrowserPrompt;
        }

        public static void PlayAudioCd(string audioCdDrivePath, IMusicLibrary musicLibrary, IAudioPlaylist audioPlaylist)
        {
            if (string.IsNullOrEmpty(audioCdDrivePath))
            {
                return;
            }

            var tracks = musicLibrary.GetTracksForDirectory(audioCdDrivePath, false);
            audioPlaylist.LoadPlaylistFromList(tracks);
        }

        public static void RipCdToFolder(IAudioCdRippingService cdRippingService)
        {
            RipCdToFolder(cdRippingService, null);
        }

        public static void RipCdToFolder(IAudioCdRippingService cdRippingService, string audioCdDrivePath)
        {

            var audioCdDrive = audioCdDrivePath ?? BrowseForAudioCdDrive();

            if (string.IsNullOrEmpty(audioCdDrive))
            {
                return;
            }

            var dialogResult = selectMp3OutputDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            var progressWindow = new ProgressMonitor();
            var progressViewModel = new CdRippingProgressMonitorViewModel();

            progressWindow.DataContext = progressViewModel;
            progressViewModel.Title = ripCdTitle;

            progressViewModel.OnComplete += (o, e) => progressWindow.Dispatcher.InvokeAsync(progressWindow.Hide);

            cdRippingService.RipCdToFolder(audioCdDrive, selectMp3OutputDialog.SelectedPath, progressViewModel);

            progressWindow.Show();
        }


        public static string BrowseForAudioCdDrive()
        {
            var dialogResult = selectAudioCdDriveDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return string.Empty;
            }

            var pathIsDriveRoot = driveRootRegex.IsMatch(selectAudioCdDriveDialog.SelectedPath);

            if (!pathIsDriveRoot)
            {
                MessageBox.Show("Selected path is not a drive root.", "Error Selecting Audio CD",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }

            return selectAudioCdDriveDialog.SelectedPath;
        }
    }
}
