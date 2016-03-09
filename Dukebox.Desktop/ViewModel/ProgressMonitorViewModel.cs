using Dukebox.Desktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dukebox.Desktop.ViewModel
{
    public class ProgressMonitorViewModel : ViewModelBase, IProgressMonitorViewModel
    {
        private string _title;
        private string _statusText;
        private string _headerText;
        private int _maximumProgressValue;
        private int _currentProgressValue;

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }
        public string HeaderText 
        {
            get
            {
                return _headerText;
            }
            set
            {
                _headerText = value;
                OnPropertyChanged("HeaderText");
            }
        }
        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                OnPropertyChanged("StatusText");
            }
        }
        public int MaximumProgressValue
        {
            get
            {
                return _maximumProgressValue;
            }
            set
            {
                _maximumProgressValue = value;
                OnPropertyChanged("MaximumProgressValue");
            }
        }
        public int CurrentProgressValue 
        {
            get
            {
                return _currentProgressValue;
            }
            set
            {
                _currentProgressValue = value;
                OnPropertyChanged("CurrentProgressValue");
            }
        }
    }
}
