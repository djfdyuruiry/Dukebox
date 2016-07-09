using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dukebox.Desktop.Converters
{
    class VisibilityToBorderThickness : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility)value;
            var borderThickness = int.Parse(parameter.ToString());

            return visibility == Visibility.Visible ? borderThickness : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
