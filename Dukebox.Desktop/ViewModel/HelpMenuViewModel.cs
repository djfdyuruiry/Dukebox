using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dukebox.Desktop.Interfaces;
using GalaSoft.MvvmLight.Command;
using System.Reflection;
using System.Windows;

namespace Dukebox.Desktop.ViewModel
{
    public class HelpMenuViewModel : ViewModelBase, IHelpMenuViewModel
    {
        private readonly string _assemblyName;
        private readonly string _assemblyVersion;
        private readonly string _copyright;

        public ICommand About { get; private set; }

        public HelpMenuViewModel() : base()
        {
            About = new RelayCommand(ShowAboutScreen);

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            
            _assemblyName = assembly.GetName().FullName;
            _assemblyVersion = assembly.GetName().Version.ToString();
            _copyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute), false)).Copyright;
        }

        private void ShowAboutScreen()
        {
            MessageBox.Show(string.Format("{0} {1} \n\n {2}", _assemblyName, _assemblyVersion, _copyright), string.Format("About {0}", _assemblyName),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
