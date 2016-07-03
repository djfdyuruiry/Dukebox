﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dukebox.Desktop.Views
{
    /// <summary>
    /// Interaction logic for WatchFolderSettings.xaml
    /// </summary>
    public partial class WatchFolderSettings : Window
    {
        public WatchFolderSettings()
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
