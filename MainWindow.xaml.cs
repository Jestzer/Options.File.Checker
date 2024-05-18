using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Text.Json;
using System.Media;

namespace Options.File.Checker.WPF
{
    public partial class MainWindow : Window
    {        
        public MainWindow()
        {
            InitializeComponent();

            // For printing the version number.
            DataContext = this;            

            LicenseFileLocationTextBox.Text = Properties.Settings.Default.LicenseFilePathSetting;
            OptionsFileLocationTextBox.Text = Properties.Settings.Default.OptionsFilePathSetting;
        }

        private void ShowErrorWindow(string errorMessage)
        {
            OutputTextBlock.Text = string.Empty;
            ErrorWindow errorWindow = new();
            errorWindow.ErrorTextBlock.Text = errorMessage;
            errorWindow.Owner = this;
            SystemSounds.Exclamation.Play();
            errorWindow.ShowDialog();
        }

        public static string PackageVersion
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                if (assembly != null)
                {
                    var version = assembly.GetName().Version;
                    if (version != null)
                    {
                        return version.ToString();
                    }
                }
                return "Error getting version number.";
            }
        }

        // The efforts one must go through to make this UI not look horrible is truly incredible.
        // Implement window dragging with the mouse.
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

        // Changing the window border size when maximized so that it doesn't go past the screen borders.
        public void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                BorderThickness = new Thickness(5);
            }
            else
            {
                BorderThickness = new Thickness(0);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private async void CheckforUpdateButton_Click(object sender, EventArgs e)
        {
            Version currentVersion = new(PackageVersion);

            // GitHub API URL for the latest release.
            string latestReleaseUrl = "https://api.github.com/repos/Jestzer/options.file.checker.c/releases/latest";

            // Use HttpClient to fetch the latest release data.
            using (HttpClient client = new())
            {
                // GitHub API requires a user-agent. I'm adding the extra headers to reduce HTTP error 403s.
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("options.file.checker.c", PackageVersion));
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    try
                    {
                        // Make the latest release a JSON string.
                        string jsonString = await client.GetStringAsync(latestReleaseUrl);

                        // Parse the JSON to get the tag_name (version number).
                        using JsonDocument doc = JsonDocument.Parse(jsonString);
                        JsonElement root = doc.RootElement;
                        string latestVersionString = root.GetProperty("tag_name").GetString()!;

                        // Remove 'v' prefix if present in the tag name.
                        latestVersionString = latestVersionString.TrimStart('v');

                        // Parse the version string.
                        Version latestVersion = new Version(latestVersionString);

                        // Compare the current version with the latest version.
                        if (currentVersion.CompareTo(latestVersion) < 0)
                        {
                            // A newer version is available!
                            ErrorWindow errorWindow = new();
                            errorWindow.Owner = this;
                            errorWindow.ErrorTextBlock.Text = "";
                            errorWindow.URLTextBlock.Visibility = Visibility.Visible;
                            errorWindow.Title = "Check for updates";
                            errorWindow.ShowDialog();
                        }
                        else
                        {
                            // The current version is up-to-date.
                            ErrorWindow errorWindow = new();
                            errorWindow.Owner = this;
                            errorWindow.Title = "Check for updates";
                            errorWindow.ErrorTextBlock.Text = "You are using the latest release available.";
                            errorWindow.ShowDialog();
                        }
                    }
                    catch (JsonException ex)
                    {
                        ErrorWindow errorWindow = new();
                        errorWindow.Owner = this;
                        errorWindow.Title = "Check for updates";
                        errorWindow.ErrorTextBlock.Text = "The Json code in this program didn't work. Here's the automatic error message it made: \"" + ex.Message + "\"";
                        errorWindow.ShowDialog();
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        ErrorWindow errorWindow = new();
                        errorWindow.Owner = this;
                        errorWindow.Title = "Check for updates";
                        errorWindow.ErrorTextBlock.Text = "HTTP error 403: GitHub is saying you're sending them too many requests, so... slow down, I guess? " +
                            "Here's the automatic error message: \"" + ex.Message + "\"";
                        errorWindow.ShowDialog();
                    }
                    catch (HttpRequestException ex)
                    {
                        ErrorWindow errorWindow = new();
                        errorWindow.Owner = this;
                        errorWindow.Title = "Check for updates";
                        errorWindow.ErrorTextBlock.Text = "HTTP error. Here's the automatic error message: \"" + ex.Message + "\"";
                        errorWindow.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.Owner = this;
                    errorWindow.Title = "Check for updates";
                    errorWindow.ErrorTextBlock.Text = "Oh dear, it looks this program had a hard time making the needed connection to GitHub. Make sure you're connected to the internet " +
                        "and your lousy firewall/VPN isn't blocking the connection. Here's the automated error message: \"" + ex.Message + "\"";
                    errorWindow.ShowDialog();
                }
            }
        }
        private void LicenseFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select a License File",
                Filter = "License Files (*.dat;*.lic)|*.dat;*.lic|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                LicenseFileLocationTextBox.Text = selectedFile;

                // The first of many checks.
                // This massive file is hopefully not a license file...
                FileInfo fileInfo = new(selectedFile);
                long fileSizeInBytes = fileInfo.Length;
                const long FiftyMegabytes = 50L * 1024L * 1024L;

                if (fileSizeInBytes > FiftyMegabytes)
                {
                    ShowErrorWindow("There is an issue with the selected license file: it is over 50 MB and therefore, likely (hopefully) not a license file.");
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                if (!System.IO.File.ReadAllText(selectedFile).Contains("INCREMENT"))
                {
                    ShowErrorWindow("There is an issue with the selected license file: it is either not a license file or it is corrupted.");
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                string fileContents = System.IO.File.ReadAllText(selectedFile);
                if (fileContents.Contains("lo=IN") || fileContents.Contains("lo=DC") || fileContents.Contains("lo=CIN"))
                {
                    ShowErrorWindow("There is an issue with the selected license file: it contains an Individual or Designated Computer license, " +
                        "which cannot use an options file.");
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                if (System.IO.File.ReadAllText(selectedFile).Contains("CONTRACT_ID="))
                {
                    ShowErrorWindow("There is an issue with the selected license file: it is not a MathWorks license file.");
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                if (!System.IO.File.ReadAllText(selectedFile).Contains("SERVER") && !System.IO.File.ReadAllText(selectedFile).Contains("DAEMON"))
                {
                    ShowErrorWindow("There is an issue with the selected license file: it is missing the SERVER and/or DAEMON line.");
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }
            }
            Properties.Settings.Default.LicenseFilePathSetting = LicenseFileLocationTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void OptionsFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select an Options File",
                Filter = "Options Files (*.opt)|*.opt|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                OptionsFileLocationTextBox.Text = selectedFile;

                // This massive file is hopefully not an options file...
                FileInfo fileInfo = new(selectedFile);
                long fileSizeInBytes = fileInfo.Length;
                const long FiftyMegabytes = 50L * 1024L * 1024L;

                if (fileSizeInBytes > FiftyMegabytes)
                {
                    ShowErrorWindow("There is an issue with the selected options file: it is over 50 MB and therefore, likely (hopefully) not an options file.");
                    OptionsFileLocationTextBox.Text = string.Empty;
                    return;
                }

                string fileContent = System.IO.File.ReadAllText(selectedFile);

                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    ShowErrorWindow("There is an issue with the selected license file: it is either empty or only contains white space.");
                    OptionsFileLocationTextBox.Text = string.Empty;
                    return;
                }
            }
            Properties.Settings.Default.OptionsFilePathSetting = OptionsFileLocationTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void LicenseFileLocationTextBox_TextChanged(object sender, EventArgs e)
        {
            bool isLicenseFileValid = System.IO.File.Exists(LicenseFileLocationTextBox.Text);
            bool isOptionsFileValid = System.IO.File.Exists(OptionsFileLocationTextBox.Text);
            AnalyzerButton.IsEnabled = isLicenseFileValid && isOptionsFileValid;
        }

        private void OptionsFileLocationTextBox_TextChanged(object sender, EventArgs e)
        {
            bool isLicenseFileValid = System.IO.File.Exists(LicenseFileLocationTextBox.Text);
            bool isOptionsFileValid = System.IO.File.Exists(OptionsFileLocationTextBox.Text);
            AnalyzerButton.IsEnabled = isLicenseFileValid && isOptionsFileValid;
        }
        private void SaveOutputButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save Output",
                Filter = "Text Files (*.txt)|*.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                // Save the text to the selected file
                System.IO.File.WriteAllText(filePath, OutputTextBlock.Text);
            }
        }

        private void AnalyzerButton_Click(object sender, RoutedEventArgs e)
        {
            string licenseFilePath = LicenseFileLocationTextBox.Text;
            string optionsFilePath = OptionsFileLocationTextBox.Text;

            // Call the AnalyzeFiles method. I'm leaving these unused variables in case I want to use them later. You're welcome future me.
            var (serverLineHasPort,
                daemonLineHasPort,
                daemonPortIsCNUFriendly,
                caseSensitivity,
                unspecifiedLicenseOrProductKey,
                licenseFileDictionary,
                includeDictionary,
                includeBorrowDictionary,
                includeAllDictionary,
                excludeDictionary,
                excludeBorrowDictionary,
                excludeAllDictionary,
                reserveDictionary,
                maxDictionary,
                groupDictionary,
                hostGroupDictionary,
                err) = LicenseAndOptionsFileDataAnalyzer.AnalyzeData(licenseFilePath, optionsFilePath);

            // Check if there was an error.
            if (!string.IsNullOrEmpty(err))
            {
                ShowErrorWindow(err);
                return;
            }

            // Process the returned data.
            StringBuilder output = new();

            if (!serverLineHasPort)
            {
                output.AppendLine("Warning: you did not specify a port number on your SERVER line.\n");
            }

            if (!daemonLineHasPort)
            {
                output.AppendLine("Warning: you did not specify a port number on your DAEMON line. This means random port will be chosen each time you restart FlexLM.\n");
            }

            if (caseSensitivity)
            {
                output.AppendLine("Warning: case sensitivity is enabled for users define in GROUPs and HOST_GROUPs.\n");
            }

            // Warn the user if they didn't specify a license number or product key in their seat-subtracting option entries.
            if (unspecifiedLicenseOrProductKey)
            {
                output.AppendLine("Please note: you did not specify a license number or product key for either one of your INCLUDE, INCLUDEALL, or RESERVE lines. This means we will subtract the seat from the first" +
                    "license the product appears on.\n");
            }

            // Print seatCount.
            if (licenseFileDictionary != null)
            {
                bool overdraftCNWarningHit = false;
                bool alreadyYelledToCNUAboutPORTFormat = false;

                foreach (var item in licenseFileDictionary)
                {
                    string seatOrSeats; // Grammar is important, I guess.

                    if (item.Value.Item2 == 1)
                    {
                        seatOrSeats = "seat";
                    }
                    else
                    {
                        seatOrSeats = "seats";
                    }

                    if (!daemonPortIsCNUFriendly && daemonLineHasPort && item.Value.Item4.Contains("CNU"))
                    {
                        if (!alreadyYelledToCNUAboutPORTFormat)
                        {
                            output.AppendLine("Please note: your license file contains a CNU license and you've specified a DAEMON port, but you did not specifically specify your DAEMON port with \"PORT=\", which is case-sensitive and recommended to do so.\n");
                            alreadyYelledToCNUAboutPORTFormat = true;
                        }
                    }

                    if (item.Value.Item4.Contains("CNU") && item.Value.Item2 == 9999999)
                    {
                        output.AppendLine($"{item.Value.Item1} has unlimited seats on license {item.Value.Item5}\n");
                    }
                    else if (item.Value.Item4.Contains("CN") && item.Value.Item2 < 0)
                    {
                        if (!overdraftCNWarningHit)
                        {
                            OutputTextBlock.Text += $"\r\nWARNING: you have specified more users on license {item.Value.Item5} for the product {item.Value.Item1} than you have seats for. " +
                                $"If every user included was using the product at once then the seat count would technically be at {item.Value.Item2}. " +
                                "This is acceptable since it is a Concurrent license, but if all seats are being used, then a user you've specified to be able to use the product will not be able to " +
                                "access this product until a seat is available.\r\n\r\nTHE WARNING ABOVE WILL ONLY PRINT ONCE FOR THIS SINGLE PRODUCT.\r\n";
                            overdraftCNWarningHit = true;
                        }
                        else
                        {
                            output.AppendLine($"You have specified more users on Concurrent license {item.Value.Item5} for the product {item.Value.Item1} than you have seats for (technically counting at {item.Value.Item2} seats.)");
                        }
                    }
                    else
                    {
                        output.AppendLine($"{item.Value.Item1} has {item.Value.Item2} unassigned {seatOrSeats} on license number {item.Value.Item5}");
                    }
                }
            }
            else
            {
                ShowErrorWindow("The license file dictionary is null. Please report this error on GitHub.");
            }

            // Show us the goods, since we didn't hit any critical errors.
            OutputTextBlock.Text = output.ToString();
        }        
    }
}
