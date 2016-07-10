﻿using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Services;
using System.Linq;

namespace Dukebox.Desktop.Views
{
    /// <summary>
    /// Interaction logic for PlaylistListing.xaml
    /// </summary>
    public partial class PlaylistListing : UserControl
    {
        public PlaylistListing()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;

            if (listBox?.SelectedItem != null)
            {
                var item = listBox.SelectedItem as PlaylistWrapper;
                var playlistListingViewModel = DataContext as IPlaylistListingViewModel;

                playlistListingViewModel?.LoadPlaylist?.Execute(item);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;

            if (listBox?.SelectedItem != null)
            {
                var item = listBox.SelectedItem as PlaylistWrapper;

                if (item == null)
                {
                    return;
                }

                Messenger.Default.Send(new PreviewTracksMessage
                {
                    Name = item.Data.Name,
                    IsPlaylist = true
                });
            }
        }
    }
}
