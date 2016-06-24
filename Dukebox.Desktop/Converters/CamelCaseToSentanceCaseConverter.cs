using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Dukebox.Desktop.Converters
{
    class CamelCaseToSentanceCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;
            
            if (stringValue == null)
            {
                throw new InvalidOperationException("Parameter 'value' was not of type string");
            }
            else if (string.IsNullOrWhiteSpace(stringValue))
            {
                return stringValue;
            }

            string sentanceCaseString = Regex.Replace(stringValue, @"(?<!^)[A-Z]", " ${0}");

            return sentanceCaseString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;

            if (stringValue == null)
            {
                throw new InvalidOperationException("Parameter 'value' was not of type string");
            }

            return stringValue.Replace(" ", string.Empty);
        }
    }
}
