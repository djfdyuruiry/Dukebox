using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Dukebox.Desktop.Helper
{
    public class ListDictionaryKeyToValueConverter : IValueConverter
    {
        private Dictionary<string, List<string>> _extendedMetadata;
        private string _metadataField;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _extendedMetadata = value as Dictionary<string, List<string>>;
            _metadataField = parameter as string;

            if (_extendedMetadata == null)
            {
                throw new InvalidOperationException("Parameter 'value' was not of type Dictionary<string, List<string>>");
            }
            else if (_metadataField == null)
            {
                throw new InvalidOperationException("Parameter 'parameter' was not of type string");
            }

            if (!_extendedMetadata.ContainsKey(_metadataField))
            {
                return "-";
            }

            var extendedMetadataValue = _extendedMetadata[_metadataField];
            var concatValue = string.Join(", ", extendedMetadataValue);

            return concatValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;

            if (stringValue == null)
            {
                throw new InvalidOperationException("Parameter 'value' was not of type string");
            }

            _extendedMetadata[_metadataField] = stringValue.Split(',').Select(s => s.Trim(' ')).ToList();

            return Binding.DoNothing;
        }
    }
}
