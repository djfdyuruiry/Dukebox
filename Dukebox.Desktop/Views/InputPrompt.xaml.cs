using System.Windows;

namespace Dukebox.Desktop.Views
{
    /// <summary>
    /// Interaction logic for InputPrompt.xaml
    /// </summary>
    public partial class InputPrompt : Window
    {
        public InputPrompt()
        {
            Owner = App.Current.MainWindow;

            InitializeComponent();
        }

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
