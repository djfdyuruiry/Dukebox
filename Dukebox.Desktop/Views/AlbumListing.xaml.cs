using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Model;
using Dukebox.Desktop.Services;

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

            if (listBox?.SelectedItem != null)
            {
                var item = listBox.SelectedItem as Album;
                var albumListingViewModel = DataContext as IAlbumListingViewModel;

                albumListingViewModel?.LoadAlbum?.Execute(item);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;

            if (listBox?.SelectedItem != null)
            {
                var item = listBox.SelectedItem as Album;

                if (item == null)
                {
                    return;
                }

                Messenger.Default.Send(new PreviewTracksMessage
                {
                    Name = item.Data.Name,
                    IsAlbum = true
                });
            }
        }
    }
}
