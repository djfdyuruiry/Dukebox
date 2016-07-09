using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dukebox.Desktop.Converters
{
    class VisibilityToBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility)value;
            var borderBrushColor = (Color)parameter;

            return visibility == Visibility.Visible ? new SolidBrush(borderBrushColor) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
