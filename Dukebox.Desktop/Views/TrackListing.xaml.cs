using Dukebox.Desktop.Interfaces;
using Dukebox.Library.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                var item = row.Item;
                var trackListingViewModel = DataContext as ITrackListingViewModel;

                trackListingViewModel?.LoadTrack?.Execute(item);
            }
        }
    }
}
