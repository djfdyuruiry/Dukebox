using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dukebox.Desktop.Interfaces;
using GalaSoft.MvvmLight.Command;

namespace Dukebox.Desktop.ViewModel
{
    public class HelpMenuViewModel : ViewModelBase, IHelpMenuViewModel
    {
        public ICommand About { get; private set; }

        public HelpMenuViewModel() : base()
        {
            // todo: create about screen and present on command
            About = new RelayCommand(ShowAboutScreen);
        }

        private void ShowAboutScreen()
        {
        }
    }
}
