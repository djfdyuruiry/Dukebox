using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library;
using Dukebox.Library.Interfaces;
using Dukebox.Model.Services;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dukebox.Desktop.ViewModel
{
    public class AudioCdViewModel : ViewModelBase, ITrackListingViewModel, ISearchControlViewModel
    {
        private readonly IAudioPlaylist _audioPlaylist;

        public ICommand ClearSearch { get; private set; }
        public string SearchText { get; set; }
        public List<Track> Tracks { get; private set; }

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

        public ICommand LoadTrack { get; private set; }

        public AudioCdViewModel(IAudioPlaylist audioPlaylist) : base()
        {
            _audioPlaylist = audioPlaylist;

            Tracks = new List<Track>();
            LoadTrack = new RelayCommand<Track>(DoLoadTrack);
        }

        private void DoLoadTrack(Track track)
        {
            _audioPlaylist.LoadPlaylistFromList(Tracks);
            _audioPlaylist.SkipToTrack(track);
        }
    }
}
