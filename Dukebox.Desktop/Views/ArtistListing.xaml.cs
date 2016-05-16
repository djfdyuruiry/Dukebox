using System.Windows.Controls;
using System.Windows.Input;
using Dukebox.Desktop.Interfaces;

namespace Dukebox.Desktop.Views
{
    /// <summary>
    /// Interaction logic for ArtistListing.xaml
    /// </summary>
    public partial class ArtistListing : UserControl
    {
        public ArtistListing()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;

            if (listBox != null)
            {
                var item = listBox.SelectedItem;
                var albumListingViewModel = DataContext as IArtistListingViewModel;

                albumListingViewModel?.LoadArtist?.Execute(item);
            }
        }
    }
}
