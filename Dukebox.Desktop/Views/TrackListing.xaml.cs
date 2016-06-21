using System.Windows.Controls;
using System.Windows.Input;
using Dukebox.Desktop.Interfaces;
using Dukebox.Desktop.Services;

namespace Dukebox.Desktop.Views
{
    /// <summary>
    /// Interaction logic for LibraryListing.xaml
    /// </summary>
    public partial class TrackListing : UserControl
    {
        public TrackListing()
        {
            InitializeComponent();
        }
        private void TrackListingRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;

            if (row != null)
            {
                var item = row.Item as TrackWrapper;
                var trackListingViewModel = DataContext as ITrackListingViewModel;

                trackListingViewModel?.LoadTrack?.Execute(item.Data);
            }
        }
    }
}
