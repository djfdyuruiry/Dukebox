using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dukebox.Desktop.Views
{
    /// <summary>
    /// Interaction logic for AlbumListing.xaml
    /// </summary>
    public partial class AlbumListing : UserControl
    {
        public AlbumListing()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;

            if (listBox != null)
            {
                var item = listBox.SelectedItem;
                var albumListingViewModel = DataContext as IAlbumListingViewModel;

                albumListingViewModel?.LoadAlbum?.Execute(item);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;

            if (listBox != null)
            {
                var item = listBox.SelectedItem as Services.Album;

                Messenger.Default.Send<PreviewArtistOrAlbumMessage>(new PreviewArtistOrAlbumMessage
                {
                    Id = item.Data.Id,
                    IsAlbum = true
                });
            }
        }
    }
}
