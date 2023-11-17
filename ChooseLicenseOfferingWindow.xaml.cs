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
            TrialNumberTextBox1.Text = Properties.Settings.Default.TrialNumber1;
            TrialNumberTextBox2.Text = Properties.Settings.Default.TrialNumber2;
            TrialNumberTextBox3.Text = Properties.Settings.Default.TrialNumber3;
            TrialNumberTextBox4.Text = Properties.Settings.Default.TrialNumber4;
            TrialNumberTextBox5.Text = Properties.Settings.Default.TrialNumber5;

            for (int i = 1; i <= 5; i++)
            {
                string settingName = $"{settingPrefix}{i}";
                RadioButton concurrentButton = FindName($"TrialConcurrentButton{i}") as RadioButton;
                RadioButton nnuButton = FindName($"TrialNNUButton{i}") as RadioButton;

                if (concurrentButton != null && nnuButton != null)
                {
                    if (Properties.Settings.Default[settingName] == "CN")
                    {
                        concurrentButton.IsChecked = true;
                    }
                    else
                    {
                        nnuButton.IsChecked = true;
                    }
                }
            }
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
            Properties.Settings.Default.TrialNumber1 = TrialNumberTextBox1.Text;
            Properties.Settings.Default.TrialNumber2 = TrialNumberTextBox2.Text;
            Properties.Settings.Default.TrialNumber3 = TrialNumberTextBox3.Text;
            Properties.Settings.Default.TrialNumber4 = TrialNumberTextBox4.Text;
            Properties.Settings.Default.TrialNumber5 = TrialNumberTextBox5.Text;

            if (TrialConcurrentButton1.IsChecked == true)
            {
                Properties.Settings.Default.TrialLicenseOffering1 = "CN";
            }
            else
            {
                Properties.Settings.Default.TrialLicenseOffering1 = "NNU";
            }

            Properties.Settings.Default.Save();
            Close();
        }

        private void TrialCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
