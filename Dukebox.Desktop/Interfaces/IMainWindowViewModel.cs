﻿using System;
using System.Windows;
using System.Windows.Input;

namespace Dukebox.Desktop.Interfaces
{
    public interface IMainWindowViewModel
    {
        ICommand ShowLoadingScreen { get; }
        ICommand NavBarItemClickCommand { get; }
        ICommand StopAudio { get; }
        Visibility ShowCurrentlyPlayingListing { get; }
        Visibility ShowAlbumListing { get; }
        Visibility ShowLibraryListing { get; }
        Visibility ShowArtistListing { get; }
        Visibility ShowRecentlyPlayedListing { get; }
        Visibility ShowPlaylistListing { get; }
        Visibility ShowAudioCdListing { get; }
        Visibility ShowTrackListingPreview { get; }
    }
}
