using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using log4net;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using Dukebox.Model.Services;
using Dukebox.Audio;
using Dukebox.Desktop.Model;

namespace Dukebox.Desktop.ViewModel
{
    public class AudioCdViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly IAudioPlaylist _audioPlaylist;
        private readonly IAudioCdDriveMonitoringService _audioCdDriveMonitor;

        private List<Track> _tracks;

        public ICommand ClearSearch { get; private set; }
        public string SearchText { get; set; }
        public List<Track> Tracks
        {
            get
            {
                return _tracks;
            }
            private set
            {
                _tracks = value;
                OnPropertyChanged("Tracks");
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

        public ICommand LoadTrack { get; private set; }

        public AudioCdViewModel(IMusicLibrary musicLibrary, IAudioPlaylist audioPlaylist, IAudioCdDriveMonitoringService audioCdDriveMonitor) : base()
        {
            _audioPlaylist = audioPlaylist;
            _audioCdDriveMonitor = audioCdDriveMonitor;

            _audioCdDriveMonitor.AudioCdInsertedOnLoad += (o, e) => Tracks = e.CdTracks;
            _audioCdDriveMonitor.AudioCdInserted += (o, e) =>
            {
                Tracks = e.CdTracks;
                OfferToLoadCd(e.DriveDirectory);
            };
            _audioCdDriveMonitor.AudioCdEjected += (o, e) => Tracks = new List<Track>();

            Tracks = new List<Track>();
            LoadTrack = new RelayCommand<Track>(DoLoadTrack);

            _audioCdDriveMonitor.StartMonitoring();
        }
        
        private void OfferToLoadCd(string cdDirectory)
        {
            var msg = string.Format("A new Audio CD ({0}) has been detected, would you like to play it?", cdDirectory);
            var response = MessageBox.Show(msg, "Audio CD Detected", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (response == MessageBoxResult.Yes)
            {
                _audioPlaylist.LoadPlaylistFromList(Tracks);

                SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
            }
        }

        private void DoLoadTrack(Track track)
        {
            _audioPlaylist.LoadPlaylistFromList(Tracks);
            _audioPlaylist.SkipToTrack(track);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }
    }
}
