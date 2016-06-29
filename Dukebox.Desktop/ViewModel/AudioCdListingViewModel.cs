using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Services;

namespace Dukebox.Desktop.ViewModel
{
    public class AudioCdViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel, IAudioDriveListingViewModel
    {
        private const string loadAudioCdPromptFormat = "A new Audio CD ({0}) has been detected, would you like to play it?";
        private const string audioCdDriveIsNotReadyErrorMsg = "Audio drive is not ready, check a working Audio CD is in the drive.";
        
        private readonly IMusicLibraryUpdateService _musicLibraryUpdateService;
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly IAudioCdDriveMonitoringService _audioCdDriveMonitor;
        private readonly IAudioCdRippingService _cdRippingService;
        private readonly ITrackGeneratorService _trackGenerator;
        private readonly IMusicLibraryEventService _eventService;

        private List<ITrack> _tracks;
        private string _selectedAudioCdDrivePath;

        public ICommand ClearSearch { get; private set; }
        public string SearchText { get; set; }
        public List<TrackWrapper> Tracks
        {
            get
            {
                return _tracks.Select(t => new TrackWrapper(_musicLibraryUpdateService, _eventService, t)).ToList();
            }
        }

        private void UpdateTracks(List<ITrack> tracks)
        {
            _tracks = tracks;
            OnPropertyChanged("Tracks");
        }

        public bool EditingListingsDisabled
        {
            get 
            { 
                return false; 
            }
        }
        public bool SearchEnabled
        {
            get
            {
                return false;
            }
        }

        public Visibility ShowSearchControl
        {
            get
            {
                return Visibility.Hidden;
            }
        }

        public ICommand LoadTrack { get; private set; }

        public List<string> AudioCdDrivePaths { get; private set; }

        public string SelectedAudioCdDrivePath
        {
            get
            {
                return _selectedAudioCdDrivePath;
            }
            set
            {
                _selectedAudioCdDrivePath = value;
                OnPropertyChanged("SelectedAudioCdDrivePath");

                AttemptToLoadCdTracks(_selectedAudioCdDrivePath);
            }
        }

        public ICommand PlayCd { get; private set; }

        public ICommand RipCd { get; private set; }

        public AudioCdViewModel(IMusicLibraryUpdateService musicLibraryUpdateService, ITrackGeneratorService trackGenerator, IAudioPlaylist audioPlaylist, 
            IAudioCdDriveMonitoringService audioCdDriveMonitor, IAudioCdRippingService cdRippingService, IMusicLibraryEventService eventService) : base()
        {
            _musicLibraryUpdateService = musicLibraryUpdateService;
            _audioPlaylist = audioPlaylist;
            _audioCdDriveMonitor = audioCdDriveMonitor;
            _cdRippingService = cdRippingService;
            _trackGenerator = trackGenerator;
            _eventService = eventService;

            _audioCdDriveMonitor.AudioCdInsertedOnLoad += (o, e) =>
            {
                UpdateTracks(e.CdTracks);
                SelectedAudioCdDrivePath = e.DriveDirectory;
            };
            _audioCdDriveMonitor.AudioCdInserted += (o, e) =>
            {
                UpdateTracks(e.CdTracks);
                OfferToLoadCd(e.DriveDirectory);
            };
            _audioCdDriveMonitor.AudioCdEjected += (o, e) => UpdateTracks(new List<ITrack>());

            UpdateTracks(new List<ITrack>());
            LoadTrack = new RelayCommand<ITrack>(DoLoadTrack);

            AudioCdDrivePaths = _audioCdDriveMonitor.GetAudioCdDrivePaths();
            SelectedAudioCdDrivePath = AudioCdDrivePaths.First();

            PlayCd = new RelayCommand(DoPlayCd);
            RipCd = new RelayCommand(DoRipCd);

            _audioCdDriveMonitor.StartMonitoring();
        }
        
        private void OfferToLoadCd(string audioCdDrivePath)
        {
            var msg = string.Format(loadAudioCdPromptFormat, audioCdDrivePath);
            var response = MessageBox.Show(msg, "Audio CD Detected", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (response == MessageBoxResult.Yes)
            {
                _audioPlaylist.LoadPlaylistFromList(_tracks);

                SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
            }
        }

        private void DoLoadTrack(ITrack track)
        {
            _audioPlaylist.LoadPlaylistFromList(_tracks);
            _audioPlaylist.SkipToTrack(track);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void AttemptToLoadCdTracks(string audioCdDrivePath)
        {
            UpdateTracks(_audioCdDriveMonitor.IsDriveReady(audioCdDrivePath) ? 
                _trackGenerator.GetTracksForDirectory(audioCdDrivePath, false) : 
                new List<ITrack>());
        }

        private void DoPlayCd()
        {
            if (!_audioCdDriveMonitor.IsDriveReady(SelectedAudioCdDrivePath))
            {
                ShowCdDriveNotReadyError("Playing Audio CD");
                return;
            }

            AudioCdHelper.PlayAudioCd(SelectedAudioCdDrivePath, _trackGenerator, _audioPlaylist);
        }

        private void DoRipCd()
        {
            if (!_audioCdDriveMonitor.IsDriveReady(SelectedAudioCdDrivePath))
            {
                ShowCdDriveNotReadyError("Ripping Audio CD");
                return;
            }

            AudioCdHelper.RipCdToFolder(_cdRippingService, SelectedAudioCdDrivePath);
        }

        private void ShowCdDriveNotReadyError(string operation)
        {
            MessageBox.Show(audioCdDriveIsNotReadyErrorMsg, string.Format("Error {0}", operation),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
