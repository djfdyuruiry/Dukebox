using System.ComponentModel;

namespace Dukebox.Desktop.Model
{
    public class ExtendedMetadataColumnSetting : INotifyPropertyChanged
    {
        private string _columnName;
        private bool _isEnabled;
    
        public event PropertyChangedEventHandler PropertyChanged;

        public string ColumnName
        {
            get
            {
                return _columnName;
            }
            set
            {
                _columnName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ColumnName"));
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
            }
        }

    }
}
