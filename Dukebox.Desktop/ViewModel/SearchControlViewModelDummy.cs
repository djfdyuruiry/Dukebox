using Dukebox.Desktop.Interfaces;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dukebox.Desktop.ViewModel
{
    public class SearchControlViewModelDummy : ISearchControlViewModel
    {
        public ICommand ClearSearch
        {
            get 
            { 
                return new RelayCommand(NoOp); 
            }
        }

        public string SearchText
        {
            get
            {
                return string.Empty;
            }
            set
            {}
        }

        public bool SearchEnabled
        {
            get { return true; }
        }

        private void NoOp()
        {
        }
    }
}
