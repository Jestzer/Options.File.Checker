using System;
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

namespace Options.File.Checker.WPF
{
    /// <summary>
    /// Interaction logic for ChooseLicenseOfferingWindow.xaml
    /// </summary>
    public partial class ChooseLicenseOfferingWindow : Window
    {
        public ChooseLicenseOfferingWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between maximized and not.
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TrialSaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TrialCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
