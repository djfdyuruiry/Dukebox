using System.Windows;

namespace Dukebox.Desktop.Views
{
    /// <summary>
    /// Interaction logic for ProgressMonitor.xaml
    /// </summary>
    public partial class ProgressMonitor : Window
    {
        public ProgressMonitor()
        {
            Owner = App.Current.MainWindow;

            InitializeComponent();
        }
    }
}
