using System.Windows;
using System.Windows.Input;

namespace Dukebox.Desktop.Views
{
    /// <summary>
    /// Interaction logic for MetadataColumnsSettings.xaml
    /// </summary>
    public partial class MetadataColumnsSettings : Window
    {
        public MetadataColumnsSettings()
        {
            Owner = App.Current.MainWindow;

            InitializeComponent();
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}
