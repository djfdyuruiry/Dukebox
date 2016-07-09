using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dukebox.Desktop.Converters
{
    [ValueConversion(typeof(bool), typeof(GridLength))]
    public class ShowTrackListingToHeightConverter : IValueConverter
    {
        private const double rowHeightWhenVisible = 0.4f;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility)value;
            return visibility == Visibility.Visible ? new GridLength(rowHeightWhenVisible, GridUnitType.Star) : new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
