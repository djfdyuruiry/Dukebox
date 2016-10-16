using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Factories;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library.Interfaces;
using System;

namespace Dukebox.Desktop.ViewModel
{
    public class AudioCdViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel, IAudioDriveListingViewModel
    {
        private const string loadAudioCdPromptFormat = "A new Audio CD ({0}) has been detected, would you like to play it?";
        private const string audioCdDriveIsNotReadyErrorMsg = "Audio drive is not ready, check a working Audio CD is in the drive.";
        
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly IAudioCdDriveMonitoringService _audioCdDriveMonitor;
        private readonly IAudioCdRippingService _cdRippingService;
        private readonly ITrackGeneratorService _trackGenerator;
        private readonly TrackSourceFactory _trackSourceFactory;

        private string _selectedAudioCdDrivePath;
        private VirtualizingObservableCollection<ITrack> _tracksCollection;

        public ICommand ClearSearch { get; private set; }
        public string SearchText { get; set; }
        public VirtualizingObservableCollection<ITrack> Tracks
        {
            get
            {
                return _tracksCollection;
            }
            set
            {
                _tracksCollection = value;
                OnPropertyChanged(nameof(Tracks));
            }
        }

        public bool EditingListingsDisabled
        {
            get 
            { 
                return true; 
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

        public ICommand EditTrack => null;

        public AudioCdViewModel(ITrackGeneratorService trackGenerator, IAudioPlaylist audioPlaylist, IAudioCdDriveMonitoringService audioCdDriveMonitor, 
            IAudioCdRippingService cdRippingService, TrackSourceFactory trackSourceFactory) : base()
        {
            _audioPlaylist = audioPlaylist;
            _audioCdDriveMonitor = audioCdDriveMonitor;
            _cdRippingService = cdRippingService;
            _trackGenerator = trackGenerator;
            _trackSourceFactory = trackSourceFactory;

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

        private void UpdateTracks(List<ITrack> tracks)
        {
            var trackSource = _trackSourceFactory.BuildMemoryTracksSource(tracks);

            var paginationManager = new PaginationManager<ITrack>(trackSource)
            {
                Provider = trackSource
            };

            paginationManager.PageSize = 50;
            paginationManager.MaxPages = 2;

            Tracks = new VirtualizingObservableCollection<ITrack>(paginationManager);
        }

        private void OfferToLoadCd(string audioCdDrivePath)
        {
            var msg = string.Format(loadAudioCdPromptFormat, audioCdDrivePath);
            var response = MessageBox.Show(msg, "Audio CD Detected", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (response == MessageBoxResult.Yes)
            {
                _audioPlaylist.LoadPlaylistFromList(Tracks.Select(t => t.Song.FileName).ToList());

                SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
            }
        }

        private void DoLoadTrack(ITrack track)
        {
            _audioPlaylist.LoadPlaylistFromList(Tracks.Select(t => t.Song.FileName).ToList());
            _audioPlaylist.SkipToTrack(track.Song.FileName);

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

            AudioCdHelper.RipCdToFolder(_cdRippingService, SelectedAudioCdDrivePath, Tracks.ToList());
        }

        private void ShowCdDriveNotReadyError(string operation)
        {
            MessageBox.Show(audioCdDriveIsNotReadyErrorMsg, string.Format("Error {0}", operation),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
