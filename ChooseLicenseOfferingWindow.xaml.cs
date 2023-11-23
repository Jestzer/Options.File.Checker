using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            // Use a for loop to set the radio button appropriately.
            for (int i = 1; i <= 5; i++)
            {
                RadioButton? concurrentButton = FindName($"TrialConcurrentButton{i}") as RadioButton;
                RadioButton? nnuButton = FindName($"TrialNNUButton{i}") as RadioButton;

                if (concurrentButton != null && nnuButton != null)
                {
                    // Use reflection to get the property value because otherwise, things don't work.
                    var trialLicenseOffering = Properties.Settings.Default.GetType().GetProperty($"TrialLicenseOffering{i}")?.GetValue(Properties.Settings.Default, null);
                    if (trialLicenseOffering != null && trialLicenseOffering.ToString() == "CN")
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

            // Store the value of the radio button selected so it can be reselcted later.
            for (int i = 1; i <= 5; i++)
            {
                RadioButton? trialConcurrentButton = FindName($"TrialConcurrentButton{i}") as RadioButton;

                if (trialConcurrentButton != null)
                {
                    var property = Properties.Settings.Default.GetType().GetProperty($"TrialLicenseOffering{i}");
                    if (property != null)
                    {
                        property.SetValue(Properties.Settings.Default, trialConcurrentButton.IsChecked == true ? "CN" : "NNU", null);
                    }
                }
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
