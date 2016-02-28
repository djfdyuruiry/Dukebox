﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;

namespace Dukebox.Desktop.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private Visibility _showLibraryListing;
        private Visibility _showAlbumListing;
        private Visibility _showArtistListing;
        private Visibility _showRecentlyPlayedListing;
        private Visibility _showAudioCdListing;

        public ICommand NavBarItemClickCommand { get; private set; }
        public Visibility ShowLibraryListing 
        { 
            get 
            { 
                return _showLibraryListing; 
            } 
            private set 
            { 
                _showLibraryListing = value; 
                OnPropertyChanged("ShowLibraryListing"); 
            } 
        }
        public Visibility ShowAlbumListing
        {
            get
            {
                return _showAlbumListing;
            }
            private set
            {
                _showAlbumListing = value;
                OnPropertyChanged("ShowAlbumListing");
            }
        }
        public Visibility ShowArtistListing
        {
            get
            {
                return _showArtistListing;
            }
            private set
            {
                _showArtistListing = value;
                OnPropertyChanged("ShowArtistListing");
            }
        }
        public Visibility ShowRecentlyPlayedListing
        {
            get
            {
                return _showRecentlyPlayedListing;
            }
            private set
            {
                _showRecentlyPlayedListing = value;
                OnPropertyChanged("ShowRecentlyPlayedListing");
            }
        }
        public Visibility ShowAudioCdListing
        {
            get
            {
                return _showAudioCdListing;
            }
            private set
            {
                _showAudioCdListing = value;
                OnPropertyChanged("ShowAudioCdListing");
            }
        }
        
        public MainWindowViewModel() : base()
        {
            NavBarItemClickCommand = new RelayCommand<string>(NavBarItemClicked);

            ShowLibraryListing = Visibility.Visible;
            ShowAlbumListing = Visibility.Hidden;
            ShowArtistListing = Visibility.Hidden;
            ShowRecentlyPlayedListing = Visibility.Hidden;
            ShowAudioCdListing = Visibility.Hidden;
        }

        private void NavBarItemClicked(string navIconName)
        {
            HideAllViews();

            if (navIconName == NavIconNames.Library)
            {
                ShowLibraryListing = Visibility.Visible;
            }
            else if (navIconName == NavIconNames.Albums)
            {
                ShowAlbumListing = Visibility.Visible;
            }
            else if (navIconName == NavIconNames.Artists)
            {
                ShowArtistListing = Visibility.Visible;
            }            
            else if (navIconName == NavIconNames.RecentlyPlayed)
            {
                ShowRecentlyPlayedListing = Visibility.Visible;
            }
            else if (navIconName == NavIconNames.AudioCd)
            {
                ShowAudioCdListing = Visibility.Visible;
            }
        }

        private void HideAllViews()
        {
            ShowLibraryListing = Visibility.Hidden;
            ShowAlbumListing = Visibility.Hidden;
            ShowArtistListing = Visibility.Hidden;
            ShowRecentlyPlayedListing = Visibility.Hidden;
            ShowAudioCdListing = Visibility.Hidden;
        }
    }
}
