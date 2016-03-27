using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Helper;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Library;
using Dukebox.Library.Interfaces;
using Dukebox.Library.Model;

namespace Dukebox.Desktop.ViewModel
{
    public class ArtistListingViewModel : ViewModelBase, IArtistListingViewModel, ISearchControlViewModel
    {
        private readonly IMusicLibrary _musicLibrary;

        private List<Artist> _artists;
        private ListSearchHelper<Artist> _listSearchHelper;
        private string _searchText;
        private readonly IAudioPlaylist _audioPlaylist;

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;

                // run search on key press
                DoSearch();
                OnPropertyChanged("SearchText");
            }
        }
        public bool SearchEnabled
        {
            get
            {
                return true;
            }
        }
        public List<Artist> Artists
        {
            get
            {
                return _listSearchHelper.FilteredItems;
            }
            private set
            {
                _artists = value;
                _listSearchHelper.Items = _artists;
                SearchText = string.Empty;

                OnPropertyChanged("Artists");
            }
        }
        public ICommand ClearSearch { get; private set; }
        public ICommand LoadArtist { get; private set; }

        public ArtistListingViewModel(IMusicLibrary musicLibrary, IAudioPlaylist audioPlaylist) : base()
        {
            _musicLibrary = musicLibrary;
            _audioPlaylist = audioPlaylist;

            _listSearchHelper = new ListSearchHelper<Artist>
            {
                FilterLambda = (a, s) => a.name.ToLower().Contains(s.ToLower())
            };

            _musicLibrary.ArtistAdded += (o, e) => RefreshArtistsFromLibrary();
            
            LoadArtist = new RelayCommand<Artist>(DoLoadArtist);
            ClearSearch = new RelayCommand(() => SearchText = string.Empty);

            RefreshArtistsFromLibrary();
        }

        public void RefreshArtistsFromLibrary()
        {
            Artists = _musicLibrary.OrderedArtists;
        }

        private void DoLoadArtist(Artist artist)
        {
            var tracks = _musicLibrary.GetTracksForArtist(artist);
            _audioPlaylist.LoadPlaylistFromList(tracks);

            SendNotificationMessage(NotificationMessages.AudioPlaylistLoadedNewTracks);
        }

        private void DoSearch()
        {
            _listSearchHelper.SearchFilter = SearchText;

            // trigger filtered items call via songs property
            OnPropertyChanged("Artists");
        }
    }
}
