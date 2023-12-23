﻿using Microsoft.Win32;
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

namespace Options.File.Checker.WPF
{
    public partial class MainWindow : Window
    {

        // Debug stuff.
        private bool debug;

        // INCLUDE dictionary from the options file.
        Dictionary<int, Tuple<string, string, string, string, string>> optionsIncludeIndex = new Dictionary<int, Tuple<string, string, string, string, string>>();

        // INCLUDE_BORROW 
        Dictionary<int, Tuple<string, string, string, string, string>> optionsIncludeBorrowIndex = new Dictionary<int, Tuple<string, string, string, string, string>>();

        // INCLUDEALL
        Dictionary<int, Tuple<string, string>> optionsIncludeAllIndex = new Dictionary<int, Tuple<string, string>>();

        // RESERVE
        Dictionary<int, Tuple<int, string, string, string, string, string>> optionsReserveIndex = new Dictionary<int, Tuple<int, string, string, string, string, string>>();

        // EXCLUDE
        Dictionary<int, Tuple<string, string, string, string, string>> optionsExcludeIndex = new Dictionary<int, Tuple<string, string, string, string, string>>();

        // EXCLUDE_BORROW
        Dictionary<int, Tuple<string, string, string, string, string>> optionsExcludeBorrowIndex = new Dictionary<int, Tuple<string, string, string, string, string>>();

        // EXCLUDEALL
        Dictionary<int, Tuple<string, string>> optionsExcludeAllIndex = new Dictionary<int, Tuple<string, string>>();

        // HOST_GROUP
        Dictionary<int, Tuple<string, string>> optionsHostGroupIndex = new Dictionary<int, Tuple<string, string>>();

        // GROUP
        Dictionary<int, Tuple<string, string, int>> optionsGroupIndex = new Dictionary<int, Tuple<string, string, int>>();

        // MAX
        Dictionary<string, Tuple<string, string, string>> optionsMaxIndex = new Dictionary<string, Tuple<string, string, string>>();

        // License file dictionary.
        Dictionary<int, Tuple<string, int, string, string, string>> licenseFileIndex = new Dictionary<int, Tuple<string, int, string, string, string>>();

        // End operations if needed.
        bool analysisFailed = false;
        bool analysisOfOptionsFileProductsFailed = false;

        // Other goodies.
        bool cnuIsUsed = false;
        bool excludeLinesAreUsed = false;
        bool containsPLP = false;
        public MainWindow()
        {
            InitializeComponent();

            // For printing the version number.
            DataContext = this;

            // Check if debug mode is enabled.
            debug = ((App)Application.Current).Debug;

            LicenseFileLocationTextBox.Text = Properties.Settings.Default.LicenseFilePathSetting;
            OptionsFileLocationTextBox.Text = Properties.Settings.Default.OptionsFilePathSetting;
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
                            errorWindow.ErrorTextBlock.Text = "";
                            errorWindow.URLTextBlock.Visibility = Visibility.Visible;
                            errorWindow.Title = "Check for updates";
                            errorWindow.ShowDialog();
                        }
                        else
                        {
                            // The current version is up-to-date.
                            ErrorWindow errorWindow = new();
                            errorWindow.Title = "Check for updates";
                            errorWindow.ErrorTextBlock.Text = "You are using the latest release available.";
                            errorWindow.ShowDialog();
                        }
                    }
                    catch (JsonException ex)
                    {
                        ErrorWindow errorWindow = new();
                        errorWindow.Title = "Check for updates";
                        errorWindow.ErrorTextBlock.Text = "The Json code in this program didn't work. Here's the automatic error message it made: \"" + ex.Message + "\"";
                        errorWindow.ShowDialog();
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        ErrorWindow errorWindow = new();
                        errorWindow.Title = "Check for updates";
                        errorWindow.ErrorTextBlock.Text = "HTTP error 403: GitHub is saying you're sending them too many requests, so... slow down, I guess? " +
                            "Here's the automatic error message: \"" + ex.Message + "\"";
                        errorWindow.ShowDialog();
                    }
                    catch (HttpRequestException ex)
                    {
                        ErrorWindow errorWindow = new();
                        errorWindow.Title = "Check for updates";
                        errorWindow.ErrorTextBlock.Text = "HTTP error. Here's the automatic error message: \"" + ex.Message + "\"";
                        errorWindow.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.Title = "Check for updates";
                    errorWindow.ErrorTextBlock.Text = "Oh dear, it looks this program had a hard time making the needed connection to GitHub. Make sure you're connected to the internet " +
                        "and your lousy firewall/VPN isn't blocking the connection. Here's the automated error message: \"" + ex.Message + "\"";
                    errorWindow.ShowDialog();
                }
            }
        }
        private void LicenseFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Title = "Select a License File";
            openFileDialog.Filter = "License Files (*.dat;*.lic)|*.dat;*.lic|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                LicenseFileLocationTextBox.Text = selectedFile;

                // This massive file is hopefully not a license file...
                FileInfo fileInfo = new FileInfo(selectedFile);
                long fileSizeInBytes = fileInfo.Length;
                const long FiftyMegabytes = 50L * 1024L * 1024L;

                if (fileSizeInBytes > FiftyMegabytes)
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it is over 50 MB and therefore, likely (hopefully) not a license file.";
                    errorWindow.ShowDialog();
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                // The first of a handful of checks.
                bool containsINCREMENT = System.IO.File.ReadAllText(selectedFile).Contains("INCREMENT");

                if (!containsINCREMENT)
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it is either not a license file or it is corrupted.";
                    errorWindow.ShowDialog();
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                string fileContents = System.IO.File.ReadAllText(selectedFile);
                if (fileContents.Contains("lo=IN") || fileContents.Contains("lo=DC") || fileContents.Contains("lo=CIN"))
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it contains an Individual or Designated Computer license, " +
                        "which cannot use an options file.";
                    errorWindow.ShowDialog();
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                bool containsCONTRACT_ID = System.IO.File.ReadAllText(selectedFile).Contains("CONTRACT_ID=");
                if (containsCONTRACT_ID)
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it is not a MathWorks license file.";
                    errorWindow.ShowDialog();
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                bool missingServer = System.IO.File.ReadAllText(selectedFile).Contains("SERVER");
                bool missingDaemon = System.IO.File.ReadAllText(selectedFile).Contains("DAEMON");
                if (!missingDaemon && !missingServer)
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it is missing the SERVER and/or DAEMON line.";
                    errorWindow.ShowDialog();
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }
            }
            Properties.Settings.Default.LicenseFilePathSetting = LicenseFileLocationTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void OptionsFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select an Options File";
            openFileDialog.Filter = "Options Files (*.opt)|*.opt|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                OptionsFileLocationTextBox.Text = selectedFile;

                // This massive file is hopefully not an options file...
                FileInfo fileInfo = new FileInfo(selectedFile);
                long fileSizeInBytes = fileInfo.Length;
                const long FiftyMegabytes = 50L * 1024L * 1024L;

                if (fileSizeInBytes > FiftyMegabytes)
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: it is over 50 MB and therefore, likely (hopefully) not an options file.";
                    errorWindow.ShowDialog();
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                string fileContent = System.IO.File.ReadAllText(selectedFile);

                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it is either empty or only contains white space.";
                    errorWindow.ShowDialog();
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
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save Output";
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                // Save the text to the selected file
                System.IO.File.WriteAllText(filePath, OutputTextBlock.Text);
            }
        }
        private void AnalyzerButton_Click(object sender, RoutedEventArgs e)
        {
            // Prevent previous entries/modified files from skewing data.
            optionsIncludeIndex.Clear();
            optionsIncludeAllIndex.Clear();
            optionsIncludeBorrowIndex.Clear();
            optionsExcludeIndex.Clear();
            optionsExcludeAllIndex.Clear();
            optionsExcludeBorrowIndex.Clear();
            optionsReserveIndex.Clear();
            optionsHostGroupIndex.Clear();
            optionsGroupIndex.Clear();
            optionsMaxIndex.Clear();
            licenseFileIndex.Clear();
            analysisFailed = false;
            analysisOfOptionsFileProductsFailed = false;
            OutputTextBlock.Text = string.Empty;
            excludeLinesAreUsed = false;
            bool hostGroupsAreUsed = false;
            containsPLP = false;
            cnuIsUsed = false;

            try
            {
                // Did you rename one of the files?
                bool isLicenseFileValid = System.IO.File.Exists(LicenseFileLocationTextBox.Text);
                bool isOptionsFileValid = System.IO.File.Exists(OptionsFileLocationTextBox.Text);
                if (!isLicenseFileValid || !isOptionsFileValid)
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "The license and/or the options file you selected either no longer exists or doesn't have permissions to be read.";
                    errorWindow.ShowDialog();
                    OptionsFileLocationTextBox.Text = string.Empty;
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                string[] optionsFileContentsLines = System.IO.File.ReadAllLines(OptionsFileLocationTextBox.Text);
                string[] licenseFileContentsLines = System.IO.File.ReadAllLines(LicenseFileLocationTextBox.Text);

                //Filter the things that don't matter.
                string[] filteredOptionsFileLines = optionsFileContentsLines.Where(line => !line.TrimStart().StartsWith("#")
                && !string.IsNullOrWhiteSpace(line)).ToArray();
                string filteredOptionsFileContents = string.Join(Environment.NewLine, filteredOptionsFileLines);

                string[] filteredLicenseFileLines = licenseFileContentsLines.Where(line => !line.TrimStart().StartsWith("#")
                && !string.IsNullOrWhiteSpace(line)).ToArray();
                string filteredLicenseFileContents = string.Join(Environment.NewLine, filteredLicenseFileLines);

                // Error time again, in case you decided to be sneaky and close the program or manually enter the filepath.
                if (string.IsNullOrWhiteSpace(filteredOptionsFileContents))
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it is either empty or only contains white space.";
                    errorWindow.ShowDialog();
                    OptionsFileLocationTextBox.Text = string.Empty;
                    return;
                }

                // Make sure you actually picked an options file.
                if (!filteredOptionsFileContents.Contains("INCLUDE") && !filteredOptionsFileContents.Contains("EXCLUDE") && !filteredOptionsFileContents.Contains("RESERVE")
                    && !filteredOptionsFileContents.Contains("MAX") && !filteredOptionsFileContents.Contains("LINGER") && !filteredOptionsFileContents.Contains("LOG") &&
                    !filteredOptionsFileContents.Contains("TIMEOUT"))
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: it is likely not an options file or contains no usable content.";
                    errorWindow.ShowDialog();
                    OptionsFileLocationTextBox.Text = string.Empty;
                    return;
                }

                bool containsINCREMENT = System.IO.File.ReadAllText(LicenseFileLocationTextBox.Text).Contains("INCREMENT");
                if (!containsINCREMENT)
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it is either not a license file or is corrupted.";
                    errorWindow.ShowDialog();
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                if (filteredLicenseFileContents.Contains("lo=IN") || filteredLicenseFileContents.Contains("lo=DC") || filteredLicenseFileContents.Contains("lo=CIN"))
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it contains an Individual or Designated Computer license, " +
                        "which cannot use an options file.";
                    errorWindow.ShowDialog();
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                bool containsCONTRACT_ID = System.IO.File.ReadAllText(LicenseFileLocationTextBox.Text).Contains("CONTRACT_ID=");
                if (containsCONTRACT_ID)
                {
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it is not a MathWorks license file.";
                    errorWindow.ShowDialog();
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                // Congrats. You passed. For now.

                // Look for issues with your SERVER or DAEMON lines.
                AnalyzeServerAndDaemonLine();
                if (analysisFailed)
                {
                    OutputTextBlock.Text = string.Empty;
                    return;
                }

                // Currently in production, unfinished.
                CollectProductInformation();
                if (analysisFailed)
                {
                    OutputTextBlock.Text = string.Empty;
                    return;
                }

                // Check for case sensitivity.
                bool caseSensitivity = false;
                for (int optionsCaseLineIndex = 0; optionsCaseLineIndex < filteredOptionsFileLines.Length; optionsCaseLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsCaseLineIndex];
                    if (line.TrimStart().StartsWith("GROUPCASEINSENSITIVE ON"))
                    {
                        caseSensitivity = true;
                    }
                }

                OutputTextBlock.Text += $"Is case sensitivity turned off?: {caseSensitivity}\r\n";

                // GROUP dictionary.                
                for (int optionsGroupLineIndex = 0; optionsGroupLineIndex < filteredOptionsFileLines.Length; optionsGroupLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsGroupLineIndex];
                    string groupName = "brokengroupName";
                    string groupUsers = "broken group Users";
                    int groupUserCount = 0;
                    if (line.TrimStart().StartsWith("GROUP "))
                    {
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        if (lineParts.Length < 3)
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you have an incorrectly formatted GROUP line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            errorWindow.ShowDialog();
                            return;
                        }

                        groupName = lineParts[1];
                        groupUsers = string.Join(" ", lineParts.Skip(2)).TrimEnd();
                        groupUserCount = groupUsers.Split(' ').Length;

                        if (debug)
                        {
                            OutputTextBlock.Text += $"{optionsGroupLineIndex}, {groupName}: {groupUsers}. User count: {groupUserCount}\r\n";
                        }
                        optionsGroupIndex[optionsGroupLineIndex] = Tuple.Create(groupName, groupUsers, groupUserCount);
                    }
                }

                // INCLUDE dictionary setup.
                ProcessIncludeAndExcludeLines(filteredOptionsFileLines, "INCLUDE", (index, productName, licenseNumber, productKey, clientType, clientSpecified) =>
                {
                    optionsIncludeIndex[index] = Tuple.Create(productName, licenseNumber, productKey, clientType, clientSpecified);
                });
                if (analysisFailed)
                {
                    OutputTextBlock.Text = string.Empty;
                    return;
                }

                // INCLUDE_BORROW dictionary setup.
                ProcessIncludeAndExcludeLines(filteredOptionsFileLines, "INCLUDE_BORROW", (index, productName, licenseNumber, productKey, clientType, clientSpecified) =>
                {
                    optionsIncludeBorrowIndex[index] = Tuple.Create(productName, licenseNumber, productKey, clientType, clientSpecified);
                });
                if (analysisFailed)
                {
                    OutputTextBlock.Text = string.Empty;
                    return;
                }

                // EXCLUDE dictionary setup.
                ProcessIncludeAndExcludeLines(filteredOptionsFileLines, "EXCLUDE", (index, productName, licenseNumber, productKey, clientType, clientSpecified) =>
                {
                    optionsExcludeIndex[index] = Tuple.Create(productName, licenseNumber, productKey, clientType, clientSpecified);
                });

                if (analysisFailed)
                {
                    OutputTextBlock.Text = string.Empty;
                    return;
                }

                // EXCLUDE_BORROW dictionary setup.
                ProcessIncludeAndExcludeLines(filteredOptionsFileLines, "EXCLUDE_BORROW", (index, productName, licenseNumber, productKey, clientType, clientSpecified) =>
                {
                    optionsExcludeBorrowIndex[index] = Tuple.Create(productName, licenseNumber, productKey, clientType, clientSpecified);
                });

                if (analysisFailed)
                {
                    OutputTextBlock.Text = string.Empty;
                    return;
                }

                // RESERVE dictionary.                
                for (int optionsReserveLineIndex = 0; optionsReserveLineIndex < filteredOptionsFileLines.Length; optionsReserveLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsReserveLineIndex];
                    string reserveSeatsString = "broken-reserveSeats";
                    int reserveSeatCount = 0;
                    string reserveProductName = "broken-reserveProductName";
                    string reserveLicenseNumber = "broken-reserveLicenseNumber";
                    string reserveProductKey = "broken-reserveProductKey";
                    string reserveClientType = "broken-reserveClientType";
                    string reserveClientSpecified = "broken-reserveClientSpecified";

                    if (line.TrimStart().StartsWith("RESERVE "))
                    {
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        if (lineParts.Length < 5)
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you have an incorrectly formatted RESERVE line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            errorWindow.ShowDialog();
                            return;
                        }

                        // Check for stray quotation marks.
                        int quoteCount = line.Count(c => c == '"');
                        if (quoteCount % 2 != 0)
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: one of your RESERVE lines has a stray quotation mark. " +
                                $"The line in question reads as this: {line}";
                            errorWindow.ShowDialog();
                            analysisFailed = true;
                            return;
                        }

                        reserveSeatsString = lineParts[1];
                        reserveProductName = lineParts[2];

                        // Convert the seat count from a string to a integer.
                        if (int.TryParse(reserveSeatsString, out reserveSeatCount))
                        {
                            // Parsing was successful.
                        }
                        else
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you have incorrectly specified the seat count for one of your RESERVE lines. " +
                                "You either chose an invalid number or specified something other than a number.";
                            errorWindow.ShowDialog();
                            return;
                        }

                        if (reserveSeatCount <= 0)
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you specified a RESERVE line with a seat count of 0 or less... why?";
                            errorWindow.ShowDialog();
                            return;
                        }

                        if (reserveProductName.Contains('"'))
                        {
                            reserveProductName = reserveProductName.Replace("\"", "");
                            reserveLicenseNumber = lineParts[3];
                            if (!reserveProductName.Contains(':'))
                            {
                                if (reserveLicenseNumber.Contains("key="))
                                {
                                    reserveProductKey = lineParts[3];
                                    string unfixedReserveProductKey = reserveProductKey;
                                    string quotedReserveProductKey = unfixedReserveProductKey.Replace("key=", "");
                                    reserveProductKey = quotedReserveProductKey.Replace("\"", "");
                                    reserveLicenseNumber = string.Empty;
                                }
                                // asset_info=
                                else
                                {
                                    string unfixedReserveLicenseNumber = reserveLicenseNumber;
                                    string quoteReserveLicenseNumber = Regex.Replace(unfixedReserveLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                    reserveLicenseNumber = quoteReserveLicenseNumber.Replace("\"", "");
                                    reserveProductKey = string.Empty;
                                }

                                reserveClientType = lineParts[4];
                                reserveClientSpecified = string.Join(" ", lineParts.Skip(5)).TrimEnd();
                            }
                            // If you have " and :
                            else
                            {
                                string[] colonParts = reserveProductName.Split(":");
                                if (colonParts.Length != 2)
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: One of your RESERVE lines has a stray colon for {reserveProductName}.";
                                    errorWindow.ShowDialog();
                                    return;
                                }
                                reserveProductName = colonParts[0];
                                if (colonParts[1].Contains("key="))
                                {
                                    string unfixedReserveProductKey = colonParts[1];
                                    reserveProductKey = unfixedReserveProductKey.Replace("key=", "");
                                    reserveLicenseNumber = string.Empty;
                                }
                                else
                                {
                                    string unfixedReserveLicenseNumber = colonParts[1];
                                    reserveLicenseNumber = Regex.Replace(unfixedReserveLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase); reserveProductKey = string.Empty;
                                }
                                reserveClientType = lineParts[3];
                                reserveClientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                            }
                        }
                        // In case you decided to use a : instead of ""...
                        else if (reserveProductName.Contains(':'))
                        {
                            string[] colonParts = reserveProductName.Split(":");
                            if (colonParts.Length != 2)
                            {
                                OutputTextBlock.Text = string.Empty;
                                ErrorWindow errorWindow = new();
                                errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: One of your RESERVE lines has a stray colon for {reserveProductName}.";
                                errorWindow.ShowDialog();
                                return;
                            }
                            reserveProductName = colonParts[0];
                            if (colonParts[1].Contains("key="))
                            {
                                string unfixedReserveProductKey = colonParts[1];
                                reserveProductKey = unfixedReserveProductKey.Replace("key=", "");
                                reserveLicenseNumber = string.Empty;
                            }
                            else
                            {
                                string unfixedReserveLicenseNumber = colonParts[1];
                                reserveLicenseNumber = Regex.Replace(unfixedReserveLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase); reserveProductKey = string.Empty;
                            }
                            reserveClientType = lineParts[3];
                            reserveClientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                        }
                        else
                        {
                            reserveClientType = lineParts[3];
                            reserveClientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                            reserveLicenseNumber = string.Empty;
                            reserveProductKey = string.Empty;
                        }
                        if (debug)
                        {
                            OutputTextBlock.Text += $"RESERVE SC: {reserveSeatCount} for {reserveProductName}: SN:{reserveLicenseNumber}. PK:{reserveProductKey} CT:{reserveClientType} CS: {reserveClientSpecified}\r\n";
                        }
                        optionsReserveIndex[optionsReserveLineIndex] = Tuple.Create(reserveSeatCount, reserveProductName, reserveLicenseNumber, reserveProductKey, reserveClientType, reserveClientSpecified);
                    }
                }

                // MAX dictionary.
                for (int optionsMaxLineIndex = 0; optionsMaxLineIndex < filteredOptionsFileLines.Length; optionsMaxLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsMaxLineIndex];
                    string maxSeats = "broken-maxSeats";
                    string maxProductName = "broken-maxProductName";
                    string maxClientType = "broken-maxClientType";
                    string maxClientSpecified = "broken-maxClientSpecified";

                    if (line.TrimStart().StartsWith("MAX "))
                    {
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        if (lineParts.Length < 5)
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you have an incorrectly formatted MAX line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            errorWindow.ShowDialog();
                            return;
                        }

                        maxSeats = lineParts[1];
                        maxProductName = lineParts[2];
                        maxClientType = lineParts[3];
                        maxClientSpecified = string.Join(" ", lineParts.Skip(4));

                        if (debug)
                        {
                            OutputTextBlock.Text += $"Max seats: {maxSeats}. Product: {maxProductName}. CT: {maxClientType}. CS: {maxClientSpecified}\r\n";
                        }
                        optionsMaxIndex[maxProductName] = Tuple.Create(maxSeats, maxClientType, maxClientSpecified);
                    }
                }

                // HOST_GROUP dictionary.
                for (int optionsHostGroupLineIndex = 0; optionsHostGroupLineIndex < filteredOptionsFileLines.Length; optionsHostGroupLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsHostGroupLineIndex];
                    string hostGroupName = "broken-hostGroupName";
                    string hostGroupClientSpecified = "broken-hostGroupClientSpecified";

                    if (line.TrimStart().StartsWith("HOST_GROUP "))
                    {
                        hostGroupsAreUsed = true;
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        if (lineParts.Length < 3)
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you have an incorrectly formatted HOST_GROUP line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            errorWindow.ShowDialog();
                            return;
                        }

                        hostGroupName = lineParts[1];
                        hostGroupClientSpecified = string.Join(" ", lineParts.Skip(2));

                        if (debug)
                        {
                            OutputTextBlock.Text += $"HOST_GROUP Name: {hostGroupName}. Clients: {hostGroupClientSpecified}\r\n";
                        }
                        optionsHostGroupIndex[optionsHostGroupLineIndex] = Tuple.Create(hostGroupName, hostGroupClientSpecified);

                    }
                }

                // EXCLUDEALL
                for (int optionsExcludeAllLineIndex = 0; optionsExcludeAllLineIndex < filteredOptionsFileLines.Length; optionsExcludeAllLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsExcludeAllLineIndex];
                    string excludeAllClientType = "broken-excludeAllClientType";
                    string excludeAllClientSpecified = "broken-excludeAllClientSpecified";

                    if (line.TrimStart().StartsWith("EXCLUDEALL "))
                    {
                        excludeLinesAreUsed = true;
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        if (lineParts.Length < 3)
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you have an incorrectly formatted EXCLUDEALL line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            errorWindow.ShowDialog();
                            return;
                        }

                        if (lineParts.Length < 3)
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you have an incorrectly formatted EXCLUDEALL line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            errorWindow.ShowDialog();
                            return;
                        }

                        excludeAllClientType = lineParts[1];
                        excludeAllClientSpecified = string.Join(" ", lineParts.Skip(2));

                        if (excludeAllClientType != "GROUP" && excludeAllClientType != "HOST" && excludeAllClientType != "HOST_GROUP" && excludeAllClientType != "DISPLAY" &&
                            excludeAllClientType != "PROJECT" && excludeAllClientType != "INTERNET")
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have incorrectly specified the client type on an EXCLUDEALL " +
                                $"line as \"{excludeAllClientType}\". Please reformat this EXCLUDEALL line.";
                            errorWindow.ShowDialog();
                            return;
                        }

                        if (debug)
                        {
                            OutputTextBlock.Text += $"EXCLUDEALL: {excludeAllClientType}. Clients: {excludeAllClientSpecified}\r\n";
                        }
                        optionsExcludeAllIndex[optionsExcludeAllLineIndex] = Tuple.Create(excludeAllClientType, excludeAllClientSpecified);
                    }
                }

                // INCLUDEALL                
                for (int optionsIncludeAllLineIndex = 0; optionsIncludeAllLineIndex < filteredOptionsFileLines.Length; optionsIncludeAllLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsIncludeAllLineIndex];
                    string includeAllClientType = "broken-includeAllClientType";
                    string includeAllClientSpecified = "broken-includeAllClientSpecified";

                    if (line.TrimStart().StartsWith("INCLUDEALL "))
                    {
                        string[] lineParts = line.Split(' ');

                        if (lineParts.Length < 3)
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you have an incorrectly formatted INCLUDEALL line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            errorWindow.ShowDialog();
                            return;
                        }

                        includeAllClientType = lineParts[1];
                        includeAllClientSpecified = string.Join(" ", lineParts.Skip(2));

                        if (includeAllClientType != "GROUP" && includeAllClientType != "HOST" && includeAllClientType != "HOST_GROUP" && includeAllClientType != "DISPLAY" &&
                            includeAllClientType != "PROJECT" && includeAllClientType != "INTERNET")
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have incorrectly specified the client type on an INCLUDEALL " +
                                $"line as \"{includeAllClientType}\". Please reformat this INCLUDEALL line.";
                            errorWindow.ShowDialog();
                            return;
                        }

                        if (debug)
                        {
                            OutputTextBlock.Text += $"INCLUDEALL: {includeAllClientType}. Clients: {includeAllClientSpecified}\r\n";
                        }
                        optionsIncludeAllIndex[optionsIncludeAllLineIndex] = Tuple.Create(includeAllClientType, includeAllClientSpecified);
                    }
                }

                // LINGER, TIMEOUT, TIMEOUTALL, NOLOG, REPORTLOG, MAX_OVERDRAFT
                bool uncommonOptionUsed = false;
                for (int optionsIncludeAllLineIndex = 0; optionsIncludeAllLineIndex < filteredOptionsFileLines.Length; optionsIncludeAllLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsIncludeAllLineIndex];
                    if (line.TrimStart().StartsWith("LINGER") || line.TrimStart().StartsWith("TIMEOUT") || line.TrimStart().StartsWith("NOLOG") ||
                        line.TrimStart().StartsWith("REPORTLOG") || line == "MAX_OVERDRAFT" || line == "DEBUGLOG")
                    {
                        uncommonOptionUsed = true;
                        break;
                    }
                }

                // Output some warning messages, if needed.
                if (excludeLinesAreUsed)
                {
                    OutputTextBlock.Text += "\r\nYou are using at least 1 EXCLUDE, EXCLUDEALL, or EXCLUDE_BORROW line in your options file. If a user is encountering an License Manager Error -38, " +
                        "then this is because they are listed on one of these lines in the options file.\r\n";
                }

                if (hostGroupsAreUsed)
                {
                    OutputTextBlock.Text += "\r\nYou are using at least 1 HOST_GROUP line in your options file.\r\n";
                }

                // Make sure the options file you used... is an options file.
                if (!optionsIncludeIndex.Any() && !optionsIncludeAllIndex.Any() && !optionsReserveIndex.Any() && !optionsExcludeIndex.Any() && !optionsExcludeAllIndex.Any() && !optionsMaxIndex.Any() &&
                    uncommonOptionUsed == false)
                {
                    OutputTextBlock.Text = string.Empty;
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: it is either not an options file or contains no usable content.";
                    errorWindow.ShowDialog();
                    return;
                }

                // Make sure the license numbers you're specifying are part of your license file.
                CheckForValidLicenseNumbersAndProductKeys();
                if (analysisOfOptionsFileProductsFailed)
                {
                    OutputTextBlock.Text = string.Empty;
                    return;
                }

                CheckForValidProducts();
                if (analysisOfOptionsFileProductsFailed)
                {
                    OutputTextBlock.Text = string.Empty;
                    return;
                }

                ValidateGroups();
                if (analysisFailed)
                {
                    OutputTextBlock.Text = string.Empty;
                    return;
                }

                CalculateRemainingSeats();
            }
            catch (Exception ex)
            {
                OutputTextBlock.Text = string.Empty;
                ErrorWindow errorWindow = new();
                errorWindow.ErrorTextBlock.Text = "Boo hoo, you broke something. Here's the program's automated error message: \"" + ex.Message + "\"";
                errorWindow.ShowDialog();
            }
            return;
        }

        private void ProcessIncludeAndExcludeLines(string[] filteredOptionsFileLines, string optionType, Action<int, string, string, string, string, string> processLine)
        {
            for (int optionsLineIndex = 0; optionsLineIndex < filteredOptionsFileLines.Length; optionsLineIndex++)
            {
                string line = filteredOptionsFileLines[optionsLineIndex];
                string productName = "brokenProductName";
                string licenseNumber = "brokenLicenseNumber";
                string productKey = "brokenProductKey";
                string clientType = "brokenClientType";
                string clientSpecified = "brokenClientSpecified";
                if (line.TrimStart().StartsWith($"{optionType} "))
                {
                    if (line.TrimStart().StartsWith("EXCLUDE"))
                    {
                        excludeLinesAreUsed = true;
                    }
                    string[] lineParts = line.Split(' ');

                    // Stop putting in random spaces.
                    while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                    {
                        lineParts = lineParts.Skip(1).ToArray();
                    }

                    if (lineParts.Length < 4)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have an incorrectly formatted {optionType} line. It is missing necessary information. " +
                            $"The line in question is \"{line}\".";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    productName = lineParts[1];
                    if (productName.Contains('"'))
                    {
                        // Check for stray quotation marks.
                        int quoteCount = line.Count(c => c == '"');
                        if (quoteCount % 2 != 0)
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: one of your {optionType} lines has a stray quotation mark. " +
                                $"The line in question reads as this: {line}";
                            errorWindow.ShowDialog();
                            analysisFailed = true;
                            return;
                        }

                        productName = productName.Replace("\"", "");
                        licenseNumber = lineParts[2];
                        if (!productName.Contains(':'))
                        {
                            if (licenseNumber.Contains("key="))
                            {
                                productKey = lineParts[2];
                                string unfixedProductKey = productKey;
                                string quotedProductKey = unfixedProductKey.Replace("key=", "");
                                productKey = quotedProductKey.Replace("\"", "");
                                licenseNumber = string.Empty;
                            }
                            // asset_info=
                            else
                            {
                                string unfixedLicenseNumber = licenseNumber;
                                string quoteLicenseNumber = Regex.Replace(unfixedLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                licenseNumber = quoteLicenseNumber.Replace("\"", "");
                                productKey = string.Empty;
                            }

                            clientType = lineParts[3];
                            clientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                        }
                        // If you have " and :
                        else
                        {
                            string[] colonParts = productName.Split(":");
                            if (colonParts.Length != 2)
                            {
                                OutputTextBlock.Text = string.Empty;
                                ErrorWindow errorWindow = new();
                                errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: one of your {optionType} lines has a stray colon for {productName}.";
                                errorWindow.ShowDialog();
                                analysisFailed = true;
                                return;
                            }
                            productName = colonParts[0];
                            if (colonParts[1].Contains("key="))
                            {
                                string unfixedProductKey = colonParts[1];
                                productKey = unfixedProductKey.Replace("key=", "");
                                licenseNumber = string.Empty;
                            }
                            else
                            {
                                string unfixedLicenseNumber = colonParts[1];
                                licenseNumber = Regex.Replace(unfixedLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                productKey = string.Empty;
                            }
                            clientType = lineParts[2];
                            clientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                        }
                    }
                    // In case you decided to use a : instead of ""...
                    else if (productName.Contains(':'))
                    {
                        string[] colonParts = productName.Split(":");
                        if (colonParts.Length != 2)
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: one of your {optionType} lines has a stray colon for {productName}.";
                            errorWindow.ShowDialog();
                            analysisFailed = true;
                            return;
                        }
                        productName = colonParts[0];
                        if (colonParts[1].Contains("key="))
                        {
                            string unfixedProductKey = colonParts[1];
                            productKey = unfixedProductKey.Replace("key=", "");
                            licenseNumber = string.Empty;
                        }
                        else
                        {
                            string unfixedLicenseNumber = colonParts[1];
                            licenseNumber = Regex.Replace(unfixedLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                            productKey = string.Empty;
                        }
                        clientType = lineParts[2];
                        clientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                    }
                    else
                    {
                        clientType = lineParts[2];
                        clientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                        licenseNumber = string.Empty;
                        productKey = string.Empty;
                    }

                    if (debug)
                    {
                        OutputTextBlock.Text += $"{productName}: SN:{licenseNumber}. PK:{productKey} CT:{clientType} CS: {clientSpecified}\r\n";
                    }

                    // Call the processLine action with the parsed data.
                    processLine(optionsLineIndex, productName, licenseNumber, productKey, clientType, clientSpecified);
                }
            }
        }

        private void CalculateRemainingSeats()
        {
            foreach (var optionsIncludeEntry in optionsIncludeIndex)
            {
                int optionsIncludeLineIndex = optionsIncludeEntry.Key;
                Tuple<string, string, string, string, string> optionsIncludeData = optionsIncludeEntry.Value;

                // Load INCLUDE specifications.
                string includeProductName = optionsIncludeData.Item1;
                string includeLicenseNumber = optionsIncludeData.Item2;
                string includeProductKey = optionsIncludeData.Item3;
                string includeClientType = optionsIncludeData.Item4;
                string includeClientSpecified = optionsIncludeData.Item5;

                foreach (var licenseFileEntry in licenseFileIndex)
                {
                    int lineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                    string productName = licenseFileData.Item1;
                    int seatCount = licenseFileData.Item2;
                    string licenseOffering = licenseFileData.Item4;
                    string licenseNumber = licenseFileData.Item5;

                    // CNU licenses have unlimited seats.
                    if (licenseOffering.Contains("CNU") && (seatCount == 9999999))
                    {
                        continue;
                    }

                    // Check for a matching includeProductName and productName.
                    if (includeProductName == productName)
                    {
                        if (includeLicenseNumber == licenseNumber)
                        {
                            if (includeClientType == "USER")
                            {
                                // Check that a user has actually been specified.
                                if (string.IsNullOrWhiteSpace(includeClientSpecified))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: You have specified a USER to be able to use {productName} " +
                                        $"for license {licenseNumber}, but you did not define the USER.";
                                    errorWindow.ShowDialog();
                                    return;
                                }

                                // Subtract 1 from seatCount, since you only specified a single user.
                                seatCount--;

                                // Error out if the seat count is negative.
                                if (seatCount < 0)
                                {
                                    if (licenseOffering == "lo=CN")
                                    {
                                        // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                                    }
                                    else
                                    {
                                        OutputTextBlock.Text = string.Empty;
                                        ErrorWindow errorWindow = new();
                                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: You have specified too many users to be able to use {productName} " +
                                            $"for license {licenseNumber}.";
                                        errorWindow.ShowDialog();
                                        return;
                                    }
                                }

                                // Update the seatCount in the licenseFileData tuple.
                                licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                // Update the seatCount in the licenseFileIndex dictionary.
                                licenseFileIndex[lineIndex] = licenseFileData;

                                // Update the OutputTextBlock.Text with the new seatCount value.
                                if (debug)
                                {
                                    OutputTextBlock.Text += $"Remaining seat count for {productName} is now {seatCount} on license {licenseNumber}\r\n";
                                }
                            }
                            if (includeClientType == "GROUP")
                            {

                                // Check that a group has actually been specified.
                                if (string.IsNullOrWhiteSpace(includeClientSpecified))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: You have specified a GROUP to be able to use {productName} " +
                                        $"for license {licenseNumber}, but you did not specify which GROUP.";
                                    errorWindow.ShowDialog();
                                    return;
                                }

                                foreach (var optionsGroupEntry in optionsGroupIndex)
                                {
                                    // Load GROUP specifications.
                                    int optionsGroupLineIndex = optionsGroupEntry.Key;
                                    Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;

                                    string groupName = optionsGroupData.Item1;
                                    string groupUsers = optionsGroupData.Item2;
                                    int groupUserCount = optionsGroupData.Item3;

                                    if (groupName == includeClientSpecified)
                                    {
                                        // Subtract the appropriate number of seats.
                                        seatCount -= groupUserCount;

                                        // Error out if the seat count is negative.
                                        if (seatCount < 0)
                                        {
                                            if (licenseOffering == "lo=CN")
                                            {
                                                // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                                            }
                                            else
                                            {
                                                OutputTextBlock.Text = string.Empty;
                                                ErrorWindow errorWindow = new();
                                                errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: You have specified too many users to be able to use {productName} " +
                                                    $"for license {licenseNumber}.";
                                                errorWindow.ShowDialog();
                                                return;
                                            }
                                        }

                                        // Update the seatCount in the licenseFileData tuple.
                                        licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                        // Update the seatCount in the licenseFileIndex dictionary.
                                        licenseFileIndex[lineIndex] = licenseFileData;

                                        if (debug)
                                        {
                                            OutputTextBlock.Text += $"Line index: {optionsGroupLineIndex}. Remaining seat count for {productName} is now {seatCount} on license {licenseNumber}\r\n";
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (debug)
                            {
                                // We'll get to the uncategorized products later.
                                OutputTextBlock.Text += $"Matching product {productName} found, but the license numbers don't match.\r\n";
                            }
                        }
                    }
                }
            }

            // Now we have to filter out the stragglers (no license number specified.)
            foreach (var optionsIncludeEntry in optionsIncludeIndex)
            {
                string productName = "brokenProductNamePart2ofCalculateRemainingSeats";
                int optionsIncludeLineIndex = optionsIncludeEntry.Key;
                Tuple<string, string, string, string, string> optionsIncludeData = optionsIncludeEntry.Value;

                // Load INCLUDE specifications.
                string includeProductName = optionsIncludeData.Item1;
                string includeLicenseNumber = optionsIncludeData.Item2;
                string includeProductKey = optionsIncludeData.Item3;
                string includeClientType = optionsIncludeData.Item4;
                string includeClientSpecified = optionsIncludeData.Item5;

                // Skip INCLUDE entries with specified license numbers. We already accounted for them.
                if (!string.IsNullOrWhiteSpace(includeLicenseNumber))
                {
                    continue;
                }

                // Set this up now so that if there are no entries left to subtract the seat(s), we know to stop the for loops.
                bool allSeatCountsZero = true;
                int seatCount = 0;
                foreach (var licenseFileEntry in licenseFileIndex)
                {
                    int lineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                    productName = licenseFileData.Item1;
                    seatCount = licenseFileData.Item2;
                    string licenseOffering = licenseFileData.Item4;
                    string licenseNumber = licenseFileData.Item5;

                    // CNU licenses have unlimited seats.
                    if (licenseOffering.Contains("CNU") && (seatCount == 9999999))
                    {
                        continue;
                    }

                    // Check for a matching includeProductName and productName.
                    if (includeProductName == productName)
                    {
                        seatCount = licenseFileData.Item2;
                        if (seatCount == 0)
                        {
                            // See if we can find another entry with the same product that does not have a seat count of 0.
                            continue;
                        }
                        if (seatCount > 0)
                        {
                            if (includeClientType == "USER")
                            {
                                // Check that a user has actually been specified.
                                if (string.IsNullOrWhiteSpace(includeClientSpecified))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified a USER to be able to use {productName}, " +
                                    "but you did not define the USER.";
                                    errorWindow.ShowDialog();
                                    return;
                                }

                                // Subtract 1 from seatCount, since you only specified a single user.
                                seatCount--;

                                // Error out if the seat count is negative.
                                if (seatCount < 0)
                                {
                                    if (licenseOffering == "lo=CN")
                                    {
                                        // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                                    }
                                    else
                                    {
                                        OutputTextBlock.Text = string.Empty;
                                        ErrorWindow errorWindow = new();
                                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified too many users to be able to use {productName}.";
                                        errorWindow.ShowDialog();
                                        return;
                                    }
                                }

                                // Update the seatCount in the licenseFileData tuple.
                                licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                // Update the seatCount in the licenseFileIndex dictionary.
                                licenseFileIndex[lineIndex] = licenseFileData;

                                // Update the OutputTextBlock.Text with the new seatCount value.
                                if (debug)
                                {
                                    OutputTextBlock.Text += $"Remaining seat count for {productName} is now {seatCount} for no specific license.\r\n";
                                }
                            }
                            if (includeClientType == "GROUP")
                            {

                                // Check that a group has actually been specified.
                                if (string.IsNullOrWhiteSpace(includeClientSpecified))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified a GROUP to be able to use {productName}, but you did not specify which GROUP.";
                                    errorWindow.ShowDialog();
                                    return;
                                }

                                foreach (var optionsGroupEntry in optionsGroupIndex)
                                {
                                    // Load GROUP specifications.
                                    int optionsGroupLineIndex = optionsGroupEntry.Key;
                                    Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;

                                    string groupName = optionsGroupData.Item1;
                                    string groupUsers = optionsGroupData.Item2;
                                    int groupUserCount = optionsGroupData.Item3;

                                    if (groupName == includeClientSpecified)
                                    {
                                        // Subtract the appropriate number of seats.
                                        seatCount -= groupUserCount;

                                        // Error out if the seat count is negative.
                                        if (seatCount < 0)
                                        {
                                            if (licenseOffering == "lo=CN")
                                            {
                                                // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                                            }
                                            else
                                            {
                                                OutputTextBlock.Text = string.Empty;
                                                ErrorWindow errorWindow = new();
                                                errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: You have specified too many users to be able to use {productName}.";
                                                errorWindow.ShowDialog();
                                                return;
                                            }
                                        }

                                        // Update the seatCount in the licenseFileData tuple.
                                        licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                        // Update the seatCount in the licenseFileIndex dictionary.
                                        licenseFileIndex[lineIndex] = licenseFileData;

                                        if (debug)
                                        {
                                            OutputTextBlock.Text += $"Remaining seat count for {productName} is now {seatCount} for no specific license.\r\n";
                                        }
                                    }
                                }
                            }

                            // We still have seats to count from other licenses!
                            allSeatCountsZero = false;
                        }
                    }
                }
                if (allSeatCountsZero && seatCount == 0)
                {
                    OutputTextBlock.Text = string.Empty;
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: You have specified too many users to be able to use {includeProductName}.";
                    errorWindow.ShowDialog();
                    return;
                }
            }

            // INCLUDEALL seat subtraction.
            foreach (var optionsIncludeAllEntry in optionsIncludeAllIndex)
            {
                int optionsIncludeAllLineIndex = optionsIncludeAllEntry.Key;
                Tuple<string, string> optionsIncludeAllData = optionsIncludeAllEntry.Value;

                // Load INCLUDEALL specifications.
                string includeAllClientType = optionsIncludeAllData.Item1;
                string includeAllClientSpecified = optionsIncludeAllData.Item2;

                if (includeAllClientType == "USER")
                {
                    // Subtract 1 from seat count for each product in the license file.
                    foreach (var licenseFileEntry in licenseFileIndex)
                    {
                        int lineIndex = licenseFileEntry.Key;
                        Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                        string productName = licenseFileData.Item1;
                        int seatCount = licenseFileData.Item2;
                        string licenseOffering = licenseFileData.Item4;
                        string licenseNumber = licenseFileData.Item5;

                        // CNU licenses have unlimited seats.
                        if (licenseOffering.Contains("CNU") && (seatCount == 9999999))
                        {
                            continue;
                        }

                        // Subtract 1 from seatCount, since you only specified a single user.
                        seatCount--;

                        // Error out if the seat count is negative.
                        if (seatCount < 0)
                        {
                            if (licenseOffering == "lo=CN")
                            {
                                // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                            }
                            else
                            {
                                OutputTextBlock.Text = string.Empty;
                                ErrorWindow errorWindow = new();
                                errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified too many users to be able to use {productName}. " +
                                    $"Don't forget that you are using at least one INCLUDEALL line.";
                                errorWindow.ShowDialog();
                                return;
                            }
                        }

                        // Update the seatCount in the licenseFileData tuple.
                        licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                        // Update the seatCount in the licenseFileIndex dictionary.
                        licenseFileIndex[lineIndex] = licenseFileData;

                        if (debug)
                        {
                            OutputTextBlock.Text += $"Remaining seat count for {productName} is now {seatCount}. INCLUDEALL. \r\n";
                        }
                    }
                }
                else if (includeAllClientType == "GROUP")
                {
                    if (string.IsNullOrWhiteSpace(includeAllClientSpecified))
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you have specified to use a GROUP on an INCLUDEALL line, but you did not specify which GROUP.";
                        errorWindow.ShowDialog();
                        return;
                    }

                    // Subtract from seat count based on the number of users in the GROUP.
                    foreach (var licenseFileEntry in licenseFileIndex)
                    {
                        int lineIndex = licenseFileEntry.Key;
                        Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                        string productName = licenseFileData.Item1;
                        int seatCount = licenseFileData.Item2;
                        string licenseOffering = licenseFileData.Item4;
                        string licenseNumber = licenseFileData.Item5;

                        // CNU licenses have unlimited seats.
                        if (licenseOffering.Contains("CNU") && (seatCount == 9999999))
                        {
                            continue;
                        }

                        foreach (var optionsGroupEntry in optionsGroupIndex)
                        {
                            // Load GROUP specifications.
                            int optionsGroupLineIndex = optionsGroupEntry.Key;
                            Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;

                            string groupName = optionsGroupData.Item1;
                            string groupUsers = optionsGroupData.Item2;
                            int groupUserCount = optionsGroupData.Item3;

                            if (groupName == includeAllClientSpecified)
                            {
                                // Subtract the appropriate number of seats.
                                seatCount -= groupUserCount;

                                // Error out if the seat count is negative.
                                if (seatCount < 0)
                                {
                                    if (licenseOffering == "lo=CN")
                                    {
                                        // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                                    }
                                    else
                                    {
                                        OutputTextBlock.Text = string.Empty;
                                        ErrorWindow errorWindow = new();
                                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified too many users to be able to use {productName}. " +
                                            "Don't forget that you are using at least 1 INCLUDEALL line.";
                                        errorWindow.ShowDialog();
                                        return;
                                    }
                                }

                                // Update the seatCount in the licenseFileData tuple.
                                licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                // Update the seatCount in the licenseFileIndex dictionary.
                                licenseFileIndex[lineIndex] = licenseFileData;

                                if (debug)
                                {
                                    OutputTextBlock.Text += $"Line index: {optionsGroupLineIndex}. Remaining seat count for {productName} is now {seatCount} INCLUDEALL GROUP.\r\n";
                                }
                            }
                        }
                    }
                }
                else if (includeAllClientType == "HOST_GROUP" || includeAllClientType == "HOST" || includeAllClientType == "DISPLAY" || includeAllClientType == "PROJECT" || includeAllClientType == "INTERNET")
                {
                    // There is no math that can be done because you've specified a client type that can be shared between any number of users.
                }
                else
                {
                    OutputTextBlock.Text = string.Empty;
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you specified an invalid client type for an INCLUDEALL line.";
                    errorWindow.ShowDialog();
                    return;
                }
            }
            // RESERVE seat subtraction.
            foreach (var optionsReserveEntry in optionsReserveIndex)
            {
                int optionsReserveLineIndex = optionsReserveEntry.Key;
                Tuple<int, string, string, string, string, string> optionsReserveData = optionsReserveEntry.Value;

                // Load RESERVE specifications.
                int reserveSeatCount = optionsReserveData.Item1;
                string reserveProductName = optionsReserveData.Item2;
                string reserveLicenseNumber = optionsReserveData.Item3;
                string reserveProductKey = optionsReserveData.Item4;
                string reserveClientType = optionsReserveData.Item5;
                string reserveClientSpecified = optionsReserveData.Item6;

                foreach (var licenseFileEntry in licenseFileIndex)
                {
                    int lineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                    string productName = licenseFileData.Item1;
                    int seatCount = licenseFileData.Item2;
                    string licenseOffering = licenseFileData.Item4;
                    string licenseNumber = licenseFileData.Item5;

                    // CNU licenses have unlimited seats.
                    if (licenseOffering.Contains("CNU") && (seatCount == 9999999))
                    {
                        continue;
                    }

                    // Check for a matching reserveProductName and productName.
                    if (reserveProductName == productName)
                    {
                        if (reserveLicenseNumber == licenseNumber)
                        {
                            if (reserveClientType == "USER")
                            {
                                // Check that a user has actually been specified.
                                if (string.IsNullOrWhiteSpace(reserveClientSpecified))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified a USER to be able to use {productName} " +
                                        $"for license {licenseNumber}, but you did not define the USER.";
                                    errorWindow.ShowDialog();
                                    return;
                                }

                                // Subtract the specified seats reserved from seatCount, even though you only specified a single user.
                                seatCount -= reserveSeatCount;

                                // Error out if the seat count is negative.
                                if (seatCount < 0)
                                {
                                    if (licenseOffering == "lo=CN")
                                    {
                                        // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                                    }
                                    else
                                    {
                                        OutputTextBlock.Text = string.Empty;
                                        ErrorWindow errorWindow = new();
                                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: You have specified too many users to be able to use {productName} " +
                                            $"for license {licenseNumber}.";
                                        errorWindow.ShowDialog();
                                        return;
                                    }
                                }

                                // Update the seatCount in the licenseFileData tuple.
                                licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                // Update the seatCount in the licenseFileIndex dictionary.
                                licenseFileIndex[lineIndex] = licenseFileData;

                                if (debug)
                                {
                                    OutputTextBlock.Text += $"Remaining seat count for {productName} is now {seatCount} on license {licenseNumber}\r\n";
                                }
                            }
                            else if (reserveClientType == "GROUP")
                            {

                                // Check that a group has actually been specified.
                                if (string.IsNullOrWhiteSpace(reserveClientSpecified))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified a GROUP to be able to use {productName} " +
                                        $"for license {licenseNumber}, but you did not specify which GROUP.";
                                    errorWindow.ShowDialog();
                                    return;
                                }

                                foreach (var optionsGroupEntry in optionsGroupIndex)
                                {
                                    // Load GROUP specifications.
                                    int optionsGroupLineIndex = optionsGroupEntry.Key;
                                    Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;

                                    string groupName = optionsGroupData.Item1;
                                    string groupUsers = optionsGroupData.Item2;
                                    int groupUserCount = optionsGroupData.Item3;

                                    if (groupName == reserveClientSpecified)
                                    {
                                        // Subtract the reserved number of seats.
                                        seatCount -= reserveSeatCount;

                                        // Error out if the seat count is negative.
                                        if (seatCount < 0)
                                        {
                                            if (licenseOffering == "lo=CN")
                                            {
                                                // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                                            }
                                            else
                                            {
                                                OutputTextBlock.Text = string.Empty;
                                                ErrorWindow errorWindow = new();
                                                errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: You have specified too many users to be able to use {productName} " +
                                                    $"for license {licenseNumber}.";
                                                errorWindow.ShowDialog();
                                                return;
                                            }
                                        }

                                        // Update the seatCount in the licenseFileData tuple.
                                        licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                        // Update the seatCount in the licenseFileIndex dictionary.
                                        licenseFileIndex[lineIndex] = licenseFileData;

                                        if (debug)
                                        {
                                            OutputTextBlock.Text += $"RESERVE line index: {optionsGroupLineIndex}. Remaining seat count for {productName} is now {seatCount} on license {licenseNumber}\r\n";
                                        }
                                    }
                                }
                            }
                            // We gotta subtract whatever you've got for RESERVE lines.
                            else
                            {
                                seatCount -= reserveSeatCount;

                                if (seatCount < 0)
                                {
                                    if (licenseOffering == "lo=CN")
                                    {
                                        // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                                    }
                                    else
                                    {
                                        OutputTextBlock.Text = string.Empty;
                                        ErrorWindow errorWindow = new();
                                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified too many users to be able to use {productName}.";
                                        errorWindow.ShowDialog();
                                        return;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (debug)
                            {
                                // We'll get to the RESERVE lines with no license number specified later.
                                OutputTextBlock.Text += $"RESERVE matching product {productName} found, but the license numbers don't match.\r\n";
                            }
                        }
                    }
                }
            }

            // Still RESERVE. Now we have to filter out the stragglers (no license number specified.)
            foreach (var optionsReserveEntry in optionsReserveIndex)
            {
                string productName = "brokenProductNamePart2ofCalculateRemainingSeatsForRESERVE";
                int optionsReserveLineIndex = optionsReserveEntry.Key;
                Tuple<int, string, string, string, string, string> optionsReserveData = optionsReserveEntry.Value;

                // Load RESERVE specifications.
                int reserveSeatCount = optionsReserveData.Item1;
                string reserveProductName = optionsReserveData.Item2;
                string reserveLicenseNumber = optionsReserveData.Item3;
                string reserveProductKey = optionsReserveData.Item4;
                string reserveClientType = optionsReserveData.Item5;
                string reserveClientSpecified = optionsReserveData.Item6;

                // Skip RESERVE entries with specified license numbers. We already accounted for them.
                if (!string.IsNullOrWhiteSpace(reserveLicenseNumber))
                {
                    continue;
                }

                // Set this up now so that if there are no entries left to subtract the seat(s), we know to stop the for loops.
                bool allSeatCountsZero = true;
                int seatCount = 0;
                foreach (var licenseFileEntry in licenseFileIndex)
                {
                    int lineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                    productName = licenseFileData.Item1;
                    seatCount = licenseFileData.Item2;
                    string licenseOffering = licenseFileData.Item4;
                    string licenseNumber = licenseFileData.Item5;

                    // CNU licenses have unlimited seats.
                    if (licenseOffering.Contains("CNU") && (seatCount == 9999999))
                    {
                        continue;
                    }

                    // Check for a matching reserveProductName and productName.
                    if (reserveProductName == productName)
                    {
                        seatCount = licenseFileData.Item2;
                        if (seatCount == 0)
                        {
                            // See if we can find another entry with the same product that does not have a seat count of 0.
                            continue;
                        }
                        if (seatCount > 0)
                        {
                            if (reserveClientType == "USER")
                            {
                                // Check that a user has actually been specified.
                                if (string.IsNullOrWhiteSpace(reserveClientSpecified))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: " +
                                        $"you have specified a USER to be able to use {productName}, but you did not define the USER.";
                                    errorWindow.ShowDialog();
                                    return;
                                }

                                seatCount -= reserveSeatCount;

                                if (seatCount < 0)
                                {
                                    if (licenseOffering == "lo=CN")
                                    {
                                        // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                                    }
                                    else
                                    {
                                        OutputTextBlock.Text = string.Empty;
                                        ErrorWindow errorWindow = new();
                                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified too many users to be able to use {productName}.";
                                        errorWindow.ShowDialog();
                                        return;
                                    }
                                }

                                // Update the seatCount in the licenseFileData tuple.
                                licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                // Update the seatCount in the licenseFileIndex dictionary.
                                licenseFileIndex[lineIndex] = licenseFileData;

                                if (debug)
                                {
                                    OutputTextBlock.Text += $"RESERVE remaining seat count for {productName} is now {seatCount} for no specific license.\r\n";
                                }
                            }
                            // HOST_GROUPs need to also be included because RESERVE lines just care about their own reserve count.
                            else if (reserveClientType == "GROUP" || reserveClientType == "HOST_GROUP")
                            {

                                // Check that a group has actually been specified.
                                if (string.IsNullOrWhiteSpace(reserveClientSpecified))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified a {reserveClientType} to be able to use {productName}, " +
                                        $"but you did not specify which {reserveClientType}.";
                                    errorWindow.ShowDialog();
                                    return;
                                }

                                // For GROUP.
                                foreach (var optionsGroupEntry in optionsGroupIndex)
                                {
                                    Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;
                                    string groupName = optionsGroupData.Item1;

                                    if (groupName == reserveClientSpecified)
                                    {
                                        ProcessReserveGroupNoLicense(groupName, reserveSeatCount, licenseOffering, productName, ref seatCount, ref licenseFileData, licenseFileIndex, lineIndex, debug);
                                        if (analysisFailed)
                                        {
                                            return;
                                        }
                                    }
                                }

                                // For HOST_GROUP.
                                foreach (var optionsHostGroupEntry in optionsHostGroupIndex)
                                {
                                    Tuple<string, string> optionsHostGroupData = optionsHostGroupEntry.Value;
                                    string hostGroupName = optionsHostGroupData.Item1;

                                    if (hostGroupName == reserveClientSpecified)
                                    {
                                        ProcessReserveGroupNoLicense(hostGroupName, reserveSeatCount, licenseOffering, productName, ref seatCount, ref licenseFileData, licenseFileIndex, lineIndex, debug);
                                        if (analysisFailed)
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                            // Lazy seat count subtraction implementation for other lineTypes, since everything should be included for RESERVE.
                            else
                            {
                                seatCount -= reserveSeatCount;

                                if (seatCount < 0)
                                {
                                    if (licenseOffering == "lo=CN")
                                    {
                                        // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                                    }
                                    else
                                    {
                                        OutputTextBlock.Text = string.Empty;
                                        ErrorWindow errorWindow = new();
                                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified too many users to be able to use {productName}.";
                                        errorWindow.ShowDialog();
                                        return;
                                    }
                                }
                            }

                            // We still have seats to count from other licenses!
                            allSeatCountsZero = false;
                        }
                    }
                }
                if (allSeatCountsZero && seatCount == 0)
                {
                    OutputTextBlock.Text = string.Empty;
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified too many users to be able to use {reserveProductName}.";
                    errorWindow.ShowDialog();
                    return;
                }
            }

            // Putting this here rather than with the other private voids because it means I don't have to add failing variables in the code above.
            PrintUsefulInformation();
        }

        private void ProcessReserveGroupNoLicense(string specified, int reserveSeatCount, string licenseOffering,
                          string productName, ref int seatCount, ref Tuple<string, int, string, string, string> licenseFileData,
                          Dictionary<int, Tuple<string, int, string, string, string>> licenseFileIndex, int lineIndex, bool debug)
        {
            // Subtract the appropriate number of seats.
            seatCount -= reserveSeatCount;

            // Error out if the seat count is negative.
            if (seatCount < 0)
            {
                if (licenseOffering == "lo=CN")
                {
                    // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                }
                else
                {
                    analysisFailed = true;
                    OutputTextBlock.Text = string.Empty;
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have specified too many users to be able to use {productName}.";
                    errorWindow.ShowDialog();
                    return;
                }
            }

            // Update the seatCount in the licenseFileData tuple.
            licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

            // Update the seatCount in the licenseFileIndex dictionary.
            licenseFileIndex[lineIndex] = licenseFileData;

            if (debug)
            {
                OutputTextBlock.Text += $"RESERVE remaining seat count for {productName} is now {seatCount} for no specific license.\r\n";
            }
        }
        private void AnalyzeServerAndDaemonLine()
        {
            string[] licenseFileContentsLines = System.IO.File.ReadAllLines(LicenseFileLocationTextBox.Text);
            string[] filteredLicenseFileLines = licenseFileContentsLines.Where(line => !line.TrimStart().StartsWith("#")
            && !string.IsNullOrWhiteSpace(line)).ToArray();
            string filteredLicenseFileContents = string.Join(Environment.NewLine, filteredLicenseFileLines);

            // Remove the line breaks to make life easier.
            string lineBreaksToRemove = "\\\r\n";
            filteredLicenseFileContents = filteredLicenseFileContents.Replace(lineBreaksToRemove, string.Empty);

            // Put it back together!
            filteredLicenseFileLines = filteredLicenseFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            string daemonProperty1 = string.Empty;
            string daemonProperty2 = string.Empty;
            int serverLineCount = 0;
            int daemonLineCount = 0;
            bool productLinesHaveBeenReached = false;

            // Start looking for the goodies.
            for (int lineIndex = 0; lineIndex < filteredLicenseFileLines.Length; lineIndex++)
            {
                string line = filteredLicenseFileLines[lineIndex];
                if (line.TrimStart().StartsWith("SERVER"))
                {
                    // SERVER lines should come before the product(s).
                    if (productLinesHaveBeenReached)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: your SERVER line(s) are listed after a product.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }
                    serverLineCount++;

                    string[] lineParts = line.Split(' ');
                    string serverHostnameOrIPAddress = lineParts[1];
                    string serverHostID = lineParts[2];

                    // Set this now since it'll be used twice.
                    string unspecifiedServerPortMessage = "\r\nWarning: You have not specified a port number for the SERVER. " +
                            "This is acceptable if you are defining it with lmadmin or if you're okay with " +
                            "FlexLM picking the port number for you. However, if you did not specify the Host ID and instead put the port number in its place, " +
                            "the specified port number will not be used. The SERVER port number will be randomly chosen instead.\r\n";

                    if (lineParts.Length == 3)
                    {
                        OutputTextBlock.Text += unspecifiedServerPortMessage;
                    }
                    else
                    {
                        if (!int.TryParse(lineParts[3], out int serverPort))
                        {
                            OutputTextBlock.Text += unspecifiedServerPortMessage;
                        }
                        OutputTextBlock.Text += $"SERVER port: {serverPort}\r\n";
                    }

                    // There is no situation where you should have more than 3 SERVER lines.
                    if (serverLineCount > 3)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it has too many SERVER lines. Only 1 or 3 are accepted.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }
                }

                // DAEMON line checks.
                if (line.TrimStart().StartsWith("DAEMON"))
                {
                    // DAEMON line should come before the product(s).
                    if (productLinesHaveBeenReached)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: your DAEMON line is listed after a product.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    // There should only be one DAEMON line.
                    daemonLineCount++;
                    if (daemonLineCount > 1)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you have more than 1 DAEMON line.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    // port= and options= should only appear once.
                    int countPortEquals = Regex.Matches(line, "port=", RegexOptions.IgnoreCase).Count;
                    int countOptionsEquals = Regex.Matches(line, "options=", RegexOptions.IgnoreCase).Count;
                    int countCommentedBeginLines = Regex.Matches(line, "# BEGIN--------------", RegexOptions.IgnoreCase).Count;

                    if (countCommentedBeginLines > 0)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it has content that is intended to be commented out in your DAEMON line.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    if (countPortEquals > 1)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you have specified more than 1 port number for MLM.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    if (countOptionsEquals > 1)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you have specified the path to more than 1 options file.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    if (countOptionsEquals == 0)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you did not specify the path to the options file. " +
                            "If you included the path, but did not use options= to specify it, MathWorks licenses ask that you do so, even if they technically work without options=.\r\n\r\n";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    // daemonProperty1 and 2 could either be a port number or path to an options file.
                    string[] lineParts = line.Split(' ');

                    // Just having the word "DAEMON" isn't enough.
                    if (lineParts.Length == 1)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you have a DAEMON line, " +
                            "but did not specify the daemon to be used (MLM) nor the path to it.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    // Checking for the vendor daemon MLM.
                    string daemonVendor = lineParts[1];

                    if (string.IsNullOrWhiteSpace(daemonVendor))
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: there are too many spaces between \"DAEMON\" and \"MLM\".";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    // The vendor daemon needs to MLM. Not mlm or anything else.
                    if (daemonVendor != "MLM")
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you have incorrectly specified the vendor daemon MLM.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    // Just specifying "DAEMON MLM" isn't enough.
                    if (lineParts.Length == 2)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you did not specify the path to the vendor daemon MLM.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    if (lineParts.Length == 3)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you did not specify the path to the options file.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    // If you're using spaces in your file path, treat any number of the lineParts as the daemonPath.
                    string daemonPath = string.Empty;
                    bool isConcatenating = false;
                    bool waitingForNextQuotationMark = false;
                    bool quotationMarksUsedInDaemonPath = false;
                    int daemonLinePartNumber = 0;

                    // Check if the current part starts with a quotation mark.
                    if (lineParts[2].StartsWith("\""))
                    {
                        waitingForNextQuotationMark = true;
                        quotationMarksUsedInDaemonPath = true;
                        for (daemonLinePartNumber = 2; daemonLinePartNumber < lineParts.Length; daemonLinePartNumber++)
                        {
                            // Check if the current part ends with a quotation mark.
                            if (lineParts[daemonLinePartNumber].EndsWith("\""))
                            {
                                waitingForNextQuotationMark = false;
                                if (daemonLinePartNumber != 2)
                                {
                                    daemonPath += lineParts[daemonLinePartNumber].Substring(0, lineParts[daemonLinePartNumber].Length - 1); // Remove the last character (quotation mark)
                                    isConcatenating = false;
                                    break; // Exit the loop since we found the next quotation mark and hopefully the end of the filepath.
                                }
                                if (daemonLinePartNumber == 2)
                                {
                                    daemonPath += lineParts[daemonLinePartNumber].Substring(1, lineParts[daemonLinePartNumber].Length - 2); // Remove the first and last character (quotation mark)
                                    break; // If you put quotation marks around a MLM path with no spaces, then we're done here.
                                }
                            }
                            else if (isConcatenating)
                            {
                                daemonPath += lineParts[daemonLinePartNumber] + " ";
                            }
                            else
                            {
                                daemonPath += lineParts[daemonLinePartNumber].Substring(1) + " "; // Remove the first character (quotation mark)
                                isConcatenating = true;
                            }
                        }
                    }
                    else { daemonPath = lineParts[2]; }

                    // Make sure you're not forgetting a quotation mark, if you used one.
                    if (waitingForNextQuotationMark)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you are missing a quotation mark at the end of the full path to MLM.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    if (daemonPath.EndsWith("\""))
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you are missing a quotation mark at the beginning of the full path to MLM.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    // Setup accepted MLM paths and check them.
                    List<string> acceptedDaemonPaths = new List<string> { "mlm.exe", "mlm.exe\\", "MLM.exe", "MLM.exe\\", "MLM", "MLM/" };
                    bool isDaemonPathAccepted = false;

                    for (int j = 0; j < acceptedDaemonPaths.Count; j++)
                    {
                        if (daemonPath.TrimEnd().EndsWith(acceptedDaemonPaths[j]))
                        {
                            isDaemonPathAccepted = true;
                            break;
                        }
                    }

                    if (!isDaemonPathAccepted)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you incorrectly specifed the full path to MLM. If there are spaces in the path, " +
                            "please put quotes around the path.";
                        errorWindow.ShowDialog();
                        analysisFailed = true;
                        return;
                    }

                    // There is a possibility that you specified MLM's port and it doesn't matter which order it's in when also specifying an options file.
                    // With this in mind, we cannot predict in the variable names which will be which.

                    string generalDaemonOrPortErrorMessage = "You incorrectly specified either the MLM port or path to the options file. They should start with port= and options=. " +
                        "If you are using quotation marks to specify the full path to the options file, the first quotation mark should come after options= " +
                        "(ex: options=\"C:\\license.dat.\")";

                    // Make sure you're not using options= or port= twice.
                    bool optionsFileHasBeenSpecified = false;
                    bool daemonPortNumberHasBeenSpecified = false;

                    bool quotationMarksUsedinOptionsFilePath = false;
                    int quotedOptionsFileLinePartNumber = 0;

                    // The lineParts need to specified differently if you used quotation marks in your MLM (daemon) path.
                    // We need to at least find the path to the options file. If you've incorrectly specified an options file or port number, we'll say so.
                    if (quotationMarksUsedInDaemonPath)
                    {

                        // daemonProperty1.
                        if (lineParts.Length > daemonLinePartNumber)
                        {
                            isConcatenating = false;
                            waitingForNextQuotationMark = false;

                            // Oh yeah, you can also use quotation marks to specify the path to the options file!
                            if (lineParts[daemonLinePartNumber + 1].TrimStart().StartsWith("options=\"", StringComparison.OrdinalIgnoreCase))
                            {
                                quotationMarksUsedinOptionsFilePath = true;
                                optionsFileHasBeenSpecified = true;

                                if (lineParts[daemonLinePartNumber + 1].TrimStart().EndsWith("\""))
                                {
                                    quotedOptionsFileLinePartNumber = daemonLinePartNumber + 1;
                                    daemonProperty1 = lineParts[daemonLinePartNumber + 1];
                                }
                                else
                                {
                                    waitingForNextQuotationMark = true;
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append(lineParts[daemonLinePartNumber + 1]);
                                    isConcatenating = true;

                                    quotedOptionsFileLinePartNumber = daemonLinePartNumber + 2;
                                    while (quotedOptionsFileLinePartNumber < lineParts.Length)
                                    {
                                        // Check if the current part ends with a quotation mark.
                                        if (lineParts[quotedOptionsFileLinePartNumber].EndsWith("\""))
                                        {
                                            waitingForNextQuotationMark = false;
                                            sb.Append(" ");
                                            sb.Append(lineParts[quotedOptionsFileLinePartNumber].Substring(0, lineParts[quotedOptionsFileLinePartNumber].Length - 1)); // Remove the last character (quotation mark)
                                            break; // Exit the loop since we found the next quotation mark and hopefully the end of the filepath.
                                        }
                                        else if (isConcatenating)
                                        {
                                            sb.Append(" ");
                                            sb.Append(lineParts[quotedOptionsFileLinePartNumber]);
                                        }
                                        quotedOptionsFileLinePartNumber++;
                                    }
                                    daemonProperty1 = sb.ToString();
                                }
                            }
                            else if (lineParts[daemonLinePartNumber + 1].TrimStart().StartsWith("options=", StringComparison.OrdinalIgnoreCase))
                            {
                                daemonProperty1 = lineParts[daemonLinePartNumber + 1];
                                optionsFileHasBeenSpecified = true;
                            }
                            else if (lineParts[daemonLinePartNumber + 1].TrimStart().StartsWith("port=", StringComparison.OrdinalIgnoreCase))
                            {
                                daemonProperty1 = lineParts[daemonLinePartNumber + 1];
                                daemonPortNumberHasBeenSpecified = true;
                            }
                            else
                            {
                                OutputTextBlock.Text = string.Empty;
                                ErrorWindow errorWindow = new();
                                errorWindow.ErrorTextBlock.Text = generalDaemonOrPortErrorMessage;
                                errorWindow.ShowDialog();
                                analysisFailed = true;
                                return;
                            }
                        }

                        // daemonProperty2.
                        if (!quotationMarksUsedinOptionsFilePath)
                        {
                            if (lineParts.Length > daemonLinePartNumber + 2)
                            {
                                if (lineParts[daemonLinePartNumber + 2].TrimStart().StartsWith("options=", StringComparison.OrdinalIgnoreCase))
                                {
                                    isConcatenating = false;
                                    waitingForNextQuotationMark = false;

                                    if (optionsFileHasBeenSpecified)
                                    {
                                        OutputTextBlock.Text = string.Empty;
                                        ErrorWindow errorWindow = new();
                                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you have specified 2 options files.";
                                        errorWindow.ShowDialog();
                                        analysisFailed = true;
                                        return;
                                    }
                                    else
                                    {
                                        if (lineParts[daemonLinePartNumber + 2].TrimStart().StartsWith("options=\"", StringComparison.OrdinalIgnoreCase))
                                        {
                                            quotationMarksUsedinOptionsFilePath = true;
                                            optionsFileHasBeenSpecified = true;
                                            if (lineParts[daemonLinePartNumber + 2].TrimStart().EndsWith("\""))
                                            {
                                                quotedOptionsFileLinePartNumber = daemonLinePartNumber + 2;
                                                daemonProperty2 = lineParts[daemonLinePartNumber + 2];
                                            }
                                            else
                                            {
                                                waitingForNextQuotationMark = true;
                                                StringBuilder sb = new StringBuilder();
                                                sb.Append(lineParts[daemonLinePartNumber + 2]);
                                                isConcatenating = true;

                                                quotedOptionsFileLinePartNumber = daemonLinePartNumber + 3;
                                                while (quotedOptionsFileLinePartNumber < lineParts.Length)
                                                {
                                                    // Check if the current part ends with a quotation mark.
                                                    if (lineParts[quotedOptionsFileLinePartNumber].EndsWith("\""))
                                                    {
                                                        waitingForNextQuotationMark = false;
                                                        sb.Append(" ");
                                                        sb.Append(lineParts[quotedOptionsFileLinePartNumber].Substring(0, lineParts[quotedOptionsFileLinePartNumber].Length - 1)); // Remove the last character (quotation mark)
                                                        break; // Exit the loop since we found the next quotation mark and hopefully the end of the filepath.
                                                    }
                                                    else if (isConcatenating)
                                                    {
                                                        sb.Append(" ");
                                                        sb.Append(lineParts[quotedOptionsFileLinePartNumber]);
                                                    }
                                                    quotedOptionsFileLinePartNumber++;
                                                }
                                                daemonProperty2 = sb.ToString();
                                            }
                                        }
                                        else
                                        {
                                            daemonProperty2 = lineParts[daemonLinePartNumber + 2];
                                        }
                                    }
                                }
                                else if (lineParts[daemonLinePartNumber + 2].TrimStart().StartsWith("port=", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (daemonPortNumberHasBeenSpecified)
                                    {
                                        OutputTextBlock.Text = string.Empty;
                                        ErrorWindow errorWindow = new();
                                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you have specified 2 port numbers for MLM.";
                                        errorWindow.ShowDialog();
                                        analysisFailed = true;
                                        return;
                                    }
                                    else
                                    {
                                        daemonProperty2 = lineParts[daemonLinePartNumber + 2];
                                    }
                                }
                                else
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = generalDaemonOrPortErrorMessage;
                                    errorWindow.ShowDialog();
                                    analysisFailed = true;
                                    return;
                                }
                            }
                        }
                        // You have already specified an options file with a quoted path.                        
                        else
                        {
                            if (lineParts.Length > quotedOptionsFileLinePartNumber + 1)
                            {
                                if (lineParts[quotedOptionsFileLinePartNumber + 1].TrimStart().StartsWith("options=", StringComparison.OrdinalIgnoreCase))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you have specified 2 options files.";
                                    errorWindow.ShowDialog();
                                    analysisFailed = true;
                                    return;

                                }
                                if (lineParts[quotedOptionsFileLinePartNumber + 1].TrimStart().StartsWith("port=", StringComparison.OrdinalIgnoreCase))
                                {
                                    daemonProperty2 = (lineParts[quotedOptionsFileLinePartNumber + 1]);
                                }
                            }
                        }
                    }
                    // If quotes aren't used in the daemon path.
                    else
                    {
                        // daemonProperty1.
                        if (lineParts.Length > 3)
                        {
                            isConcatenating = false;
                            waitingForNextQuotationMark = false;
                            if (lineParts[3].TrimStart().StartsWith("options=\"", StringComparison.OrdinalIgnoreCase))
                            {
                                quotationMarksUsedinOptionsFilePath = true;
                                optionsFileHasBeenSpecified = true;

                                if (lineParts[3].TrimStart().EndsWith("\""))
                                {
                                    quotedOptionsFileLinePartNumber = 3;
                                    daemonProperty1 = (lineParts[3]);
                                }
                                else
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append(lineParts[3]);
                                    isConcatenating = true;
                                    waitingForNextQuotationMark = true;
                                    quotedOptionsFileLinePartNumber = 4;
                                    while (quotedOptionsFileLinePartNumber < lineParts.Length)
                                    {
                                        // Check if the current part ends with a quotation mark.
                                        if (lineParts[quotedOptionsFileLinePartNumber].EndsWith("\""))
                                        {
                                            waitingForNextQuotationMark = false;
                                            sb.Append(" ");
                                            sb.Append(lineParts[quotedOptionsFileLinePartNumber].Substring(0, lineParts[quotedOptionsFileLinePartNumber].Length - 1)); // Remove the last character (quotation mark)
                                            break; // Exit the loop since we found the next quotation mark and hopefully the end of the filepath.
                                        }
                                        else if (isConcatenating)
                                        {
                                            sb.Append(" ");
                                            sb.Append(lineParts[quotedOptionsFileLinePartNumber]);
                                        }
                                        quotedOptionsFileLinePartNumber++;
                                    }
                                    daemonProperty1 = sb.ToString();
                                }
                            }
                            else if (lineParts[3].TrimStart().StartsWith("options=", StringComparison.OrdinalIgnoreCase))
                            {
                                optionsFileHasBeenSpecified = true;
                                daemonProperty1 = lineParts[3];
                            }
                            else if (lineParts[3].TrimStart().StartsWith("port=", StringComparison.OrdinalIgnoreCase))
                            {
                                daemonPortNumberHasBeenSpecified = true;
                                daemonProperty1 = lineParts[3];
                            }
                            else
                            {
                                OutputTextBlock.Text = string.Empty;
                                ErrorWindow errorWindow = new();
                                errorWindow.ErrorTextBlock.Text = generalDaemonOrPortErrorMessage;
                                errorWindow.ShowDialog();
                                analysisFailed = true;
                                return;
                            }
                        }
                        // daemonProperty2.
                        if (!quotationMarksUsedinOptionsFilePath)
                        {
                            if (lineParts.Length > 4)
                            {
                                if (lineParts[4].TrimStart().StartsWith("options=", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (optionsFileHasBeenSpecified)
                                    {
                                        OutputTextBlock.Text = string.Empty;
                                        ErrorWindow errorWindow = new();
                                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you have specified 2 options files.";
                                        errorWindow.ShowDialog();
                                        analysisFailed = true;
                                        return;
                                    }
                                    else
                                    {
                                        daemonProperty2 = lineParts[4];
                                    }
                                }
                                else if (lineParts[4].TrimStart().StartsWith("port=", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (daemonPortNumberHasBeenSpecified)
                                    {
                                        OutputTextBlock.Text = string.Empty;
                                        ErrorWindow errorWindow = new();
                                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you have specified 2 port numbers for MLM.";
                                        errorWindow.ShowDialog();
                                        analysisFailed = true;
                                        return;
                                    }
                                    else
                                    {
                                        daemonProperty2 = lineParts[4];
                                    }
                                }
                                else
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = generalDaemonOrPortErrorMessage;
                                    errorWindow.ShowDialog();
                                    analysisFailed = true;
                                    return;
                                }
                            }
                        }
                        // You have already specified an options file with a quoted path.
                        else
                        {
                            if (lineParts.Length > quotedOptionsFileLinePartNumber + 1)
                            {
                                if (lineParts[quotedOptionsFileLinePartNumber + 1].TrimStart().StartsWith("options=", StringComparison.OrdinalIgnoreCase))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    ErrorWindow errorWindow = new();
                                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: you have specified 2 options files.";
                                    errorWindow.ShowDialog();
                                    analysisFailed = true;
                                    return;

                                }
                                if (lineParts[quotedOptionsFileLinePartNumber + 1].TrimStart().StartsWith("port=", StringComparison.OrdinalIgnoreCase))
                                {
                                    daemonProperty2 = (lineParts[quotedOptionsFileLinePartNumber + 1]);
                                }
                            }
                        }
                    }

                    // Print daemon information appropriately.
                    string daemonProperty1Edited = Regex.Replace(daemonProperty1, "port=", "", RegexOptions.IgnoreCase);
                    daemonProperty1Edited = Regex.Replace(daemonProperty1Edited, "options=", "", RegexOptions.IgnoreCase);
                    daemonProperty1Edited = Regex.Replace(daemonProperty1Edited, "\"", "", RegexOptions.IgnoreCase);
                    string daemonProperty2Edited = Regex.Replace(daemonProperty2, "port=", "", RegexOptions.IgnoreCase);
                    daemonProperty2Edited = Regex.Replace(daemonProperty2Edited, "options=", "", RegexOptions.IgnoreCase);
                    daemonProperty2Edited = Regex.Replace(daemonProperty2Edited, "\"", "", RegexOptions.IgnoreCase);

                    if (daemonProperty1.IndexOf("options", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (daemonProperty2.IndexOf("port", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            OutputTextBlock.Text += $"\r\nDAEMON path: {daemonPath}.\r\nOptions path: {daemonProperty1Edited}.\r\nDAEMON port: {daemonProperty2Edited}.\r\n";
                        }
                        else
                        {
                            OutputTextBlock.Text += $"\r\nDAEMON path: {daemonPath}.\r\nOptions path: {daemonProperty1Edited}.\r\nDAEMON port: unspecified, will be randomly chosen.\r\n";
                        }
                    }
                    else
                    {
                        OutputTextBlock.Text += $"\r\nDAEMON path: {daemonPath}.\r\nDAEMON port: {daemonProperty1Edited}.\r\nOptions path: {daemonProperty2Edited}.\r\n";
                    }
                }

                if (line.TrimStart().StartsWith("USE_SERVER", StringComparison.OrdinalIgnoreCase))
                {
                    OutputTextBlock.Text = string.Empty;
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it uses \"USE_SERVER\". " +
                        "This should be in the end user's network.lic file, not the Network License Manager's license file.";
                    errorWindow.ShowDialog();
                    analysisFailed = true;
                    return;
                }
                if (line.TrimStart().StartsWith("INCREMENT"))
                {
                    productLinesHaveBeenReached = true;
                }
            }

            // We have to do this after the loop so that it doesn't get stuck while counting to 3.
            if (serverLineCount == 2)
            {
                OutputTextBlock.Text = string.Empty;
                ErrorWindow errorWindow = new();
                errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it has an invalid number of SERVER lines. Only 1 or 3 are accepted.";
                errorWindow.ShowDialog();
                analysisFailed = true;
                return;
            }

            // These have to happen outside the loop to get a proper tally.
            if (daemonLineCount == 0)
            {
                OutputTextBlock.Text = string.Empty;
                ErrorWindow errorWindow = new();
                errorWindow.ErrorTextBlock.Text = "There is an issue with the selected options file: you have no DAEMON line in your license file. You should not " +
                    "be using VENDOR lines for MathWorks licenses, if you are.";
                errorWindow.ShowDialog();
                analysisFailed = true;
                return;
            }

            if ((!daemonProperty1.Contains("options=", StringComparison.OrdinalIgnoreCase)) && (!daemonProperty2.Contains("options=", StringComparison.OrdinalIgnoreCase)))
            {
                OutputTextBlock.Text = string.Empty;
                ErrorWindow errorWindow = new();
                errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it does not specify an options file.";
                errorWindow.ShowDialog();
                analysisFailed = true;
                return;
            }
        }
        private void CheckForValidProducts()
        {
            foreach (var optionsIncludeEntry in optionsIncludeIndex)
            {
                Tuple<string, string, string, string, string> optionsIncludeData = optionsIncludeEntry.Value;

                string includeProductName = optionsIncludeData.Item1;
                bool foundMatchingProduct = false;

                foreach (var licenseFileEntry in licenseFileIndex)
                {
                    int lineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                    string licenseProductName = licenseFileData.Item1;

                    // Check for a matching includeProductName and productName.
                    if (includeProductName == licenseProductName)
                    {
                        foundMatchingProduct = true;
                        break;
                    }
                }
                if (!foundMatchingProduct)
                {
                    OutputTextBlock.Text = string.Empty;
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you specified a product, {includeProductName}, but this product is not in your license file. " +
                        $"Product names must match the ones found in the license file after the word INCREMENT and they are case-sensitive.";
                    errorWindow.ShowDialog();
                    analysisOfOptionsFileProductsFailed = true;
                    return;
                }
            }
        }

        private void CheckForValidLicenseNumbersAndProductKeys()
        {
            // INCLUDE lines.            
            foreach (var optionsIncludeEntry in optionsIncludeIndex)
            {
                bool foundMatchingLicenseNumberOrProductKey = false;
                Tuple<string, string, string, string, string> optionsIncludeData = optionsIncludeEntry.Value;
                string includeProductName = optionsIncludeData.Item1;
                string includeLicenseNumber = optionsIncludeData.Item2;
                string includeProductKey = optionsIncludeData.Item3;

                // Skip if no license number nor product key is specified.
                if (string.IsNullOrWhiteSpace(includeLicenseNumber))
                {
                    if (string.IsNullOrWhiteSpace(includeProductKey))
                    {
                        continue;
                    }
                }

                foreach (var licenseFileEntry in licenseFileIndex)
                {
                    int lineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;
                    string productName = licenseFileData.Item1;
                    string productKey = licenseFileData.Item3;
                    string licenseNumber = licenseFileData.Item5;

                    if (includeLicenseNumber == licenseNumber)
                    {
                        foundMatchingLicenseNumberOrProductKey = true;
                        break;
                    }

                    if (includeProductKey == productKey)
                    {
                        if (includeProductName == productName)
                        {
                            foundMatchingLicenseNumberOrProductKey = true;
                            break;
                        }
                        else
                        {
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have an INCLUDE line that specifies a product key, {includeProductKey}, " +
                                $"but the product on your INCLUDE line, {includeProductName}, does not match the product it is tied to in the license file, {productName}.";
                            errorWindow.ShowDialog();
                            analysisOfOptionsFileProductsFailed = true;
                            return;
                        }
                    }
                }

                if (foundMatchingLicenseNumberOrProductKey == false)
                {
                    if (string.IsNullOrEmpty(includeProductKey) == true)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have an INCLUDE line that specifies a license number, {includeLicenseNumber}, " +
                            "that does not exist in the specified license file.";
                        errorWindow.ShowDialog();
                        analysisOfOptionsFileProductsFailed = true;
                        return;
                    }
                    else
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have an INCLUDE line that specifies a product key, {includeProductKey}, " +
                            "that does not exist in the specified license file.";
                        errorWindow.ShowDialog();
                        analysisOfOptionsFileProductsFailed = true;
                        return;
                    }
                }
            }

            // EXCLUDE lines.            
            foreach (var optionsExcludeEntry in optionsExcludeIndex)
            {
                bool foundMatchingLicenseNumberOrProductKey = false;
                Tuple<string, string, string, string, string> optionsExcludeData = optionsExcludeEntry.Value;
                string excludeLicenseNumber = optionsExcludeData.Item2;
                string excludeProductKey = optionsExcludeData.Item3;

                // Skip if no license number nor product key is specified.
                if (string.IsNullOrWhiteSpace(excludeLicenseNumber))
                {
                    if (string.IsNullOrWhiteSpace(excludeProductKey))
                    {
                        continue;
                    }
                }

                foreach (var licenseFileEntry in licenseFileIndex)
                {
                    int lineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;
                    string productName = licenseFileData.Item1;
                    string productKey = licenseFileData.Item3;
                    string licenseNumber = licenseFileData.Item5;

                    if (excludeLicenseNumber == licenseNumber)
                    {
                        foundMatchingLicenseNumberOrProductKey = true;
                        break;
                    }

                    if (excludeProductKey == productKey)
                    {
                        foundMatchingLicenseNumberOrProductKey = true;
                        break;
                    }
                }

                if (foundMatchingLicenseNumberOrProductKey == false)
                {
                    if (string.IsNullOrEmpty(excludeProductKey) == true)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have an EXCLUDE line that specifies a license number, {excludeLicenseNumber}, " +
                            "that does not exist in the specified license file.";
                        errorWindow.ShowDialog();
                        analysisOfOptionsFileProductsFailed = true;
                        return;
                    }
                    else
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have an EXCLUDE line that specifies a product key, {excludeProductKey}, " +
                            "that does not exist in the specified license file.";
                        errorWindow.ShowDialog();
                        analysisOfOptionsFileProductsFailed = true;
                        return;
                    }
                }
            }

            // RESERVE lines.            
            foreach (var optionsReserveEntry in optionsReserveIndex)
            {
                bool foundMatchingLicenseNumberOrProductKey = false;
                Tuple<int, string, string, string, string, string> optionsReserveData = optionsReserveEntry.Value;
                string reserveLicenseNumber = optionsReserveData.Item3;
                string reserveProductKey = optionsReserveData.Item4;

                // Skip if no license number nor product key is specified.
                if (string.IsNullOrWhiteSpace(reserveLicenseNumber))
                {
                    if (string.IsNullOrWhiteSpace(reserveProductKey))
                    {
                        continue;
                    }
                }

                foreach (var licenseFileEntry in licenseFileIndex)
                {
                    int lineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;
                    string productName = licenseFileData.Item1;
                    string productKey = licenseFileData.Item3;
                    string licenseNumber = licenseFileData.Item5;

                    if (reserveLicenseNumber == licenseNumber)
                    {
                        foundMatchingLicenseNumberOrProductKey = true;
                        break;
                    }

                    if (reserveProductKey == productKey)
                    {
                        foundMatchingLicenseNumberOrProductKey = true;
                        break;
                    }
                }

                if (foundMatchingLicenseNumberOrProductKey == false)
                {
                    if (string.IsNullOrEmpty(reserveProductKey) == true)
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have a RESERVE line that specifies a license number, {reserveLicenseNumber}, " +
                            "that does not exist in the specified license file.";
                        errorWindow.ShowDialog();
                        analysisOfOptionsFileProductsFailed = true;
                        return;
                    }
                    else
                    {
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you have a RESERVE line that specifies a product key, {reserveProductKey}, " +
                            "that does not exist in the specified license file.";
                        errorWindow.ShowDialog();
                        analysisOfOptionsFileProductsFailed = true;
                        return;
                    }
                }
            }
        }

        // Define a delegate that takes a tuple and returns the groupType and specified strings.
        public delegate (string groupType, string specified) TupleExtractor<in TTuple>(TTuple tuple);

        private bool PerformGroupCheck<TKey, TTuple>(Dictionary<TKey, TTuple> optionsIndex,
                                                     Dictionary<TKey, Tuple<string, string, int>> groupIndex,
                                                     Dictionary<TKey, Tuple<string, string>> hostGroupIndex,
                                                     TupleExtractor<TTuple> extractor,
                                                     string lineType) where TKey : notnull
        {
            foreach (var entry in optionsIndex)
            {
                TTuple tuple = entry.Value;

                // Use the extractor to get the groupType and specified strings.
                (string groupType, string specified) = extractor(tuple);
                if (groupType == "GROUP")
                {
                    foreach (var optionsGroupEntry in groupIndex)
                    {
                        Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;
                        string groupName = optionsGroupData.Item1;

                        if (groupName == specified)
                        {
                            return true; // Matching GROUP found.
                        }
                    }
                }
                else if (groupType == "HOST_GROUP")
                {
                    foreach (var optionsHostGroupEntry in hostGroupIndex)
                    {
                        Tuple<string, string> optionsHostGroupData = optionsHostGroupEntry.Value;
                        string hostGroupName = optionsHostGroupData.Item1;

                        if (hostGroupName == specified)
                        {
                            return true; // Matching HOST_GROUP found.
                        }
                    }
                }
                else
                {
                    return true; // No need to do this for other client types.
                }

                // No matching group found.
                string aOrAn;

                if (lineType == "RESERVE")
                {
                    aOrAn = "a";
                }
                else
                {
                    aOrAn = "an";
                }

                analysisFailed = true;
                OutputTextBlock.Text = string.Empty;
                ErrorWindow errorWindow = new();
                errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected options file: you specified a {groupType} on {aOrAn} {lineType} line named \"{specified}\", " +
                    $"but this {groupType} does not exist in your options file. Please check your {groupType}s for any typos. HOST_GROUP and GROUP are separate.";
                errorWindow.ShowDialog();
                return false;
            }
            return true;
        }

        private void ValidateGroups()
        {
            // PerformGroupCheck for INCLUDE lines.
            foreach (var optionsIncludeEntry in optionsIncludeIndex)
            {
                if (!PerformGroupCheck(optionsIncludeIndex, optionsGroupIndex, optionsHostGroupIndex,
                                       tuple => (tuple.Item4, tuple.Item5), "INCLUDE"))
                {
                    return;
                }
            }

            // EXCLUDE
            foreach (var optionsExcludeEntry in optionsExcludeIndex)
            {
                if (!PerformGroupCheck(optionsExcludeIndex, optionsGroupIndex, optionsHostGroupIndex,
                                       tuple => (tuple.Item4, tuple.Item5), "EXCLUDE"))
                {
                    return;
                }
            }

            // RESERVE
            foreach (var optionsReserveEntry in optionsReserveIndex)
            {
                if (!PerformGroupCheck(optionsReserveIndex, optionsGroupIndex, optionsHostGroupIndex,
                                       tuple => (tuple.Item5, tuple.Item6), "RESERVE"))
                {
                    return;
                }
            }
            // INCLUDEALL
            foreach (var optionsIncludeAllEntry in optionsIncludeAllIndex)
            {
                if (!PerformGroupCheck(optionsIncludeAllIndex, optionsGroupIndex, optionsHostGroupIndex,
                                       tuple => (tuple.Item1, tuple.Item2), "INCLUDEALL"))
                {
                    return;
                }
            }
            // EXCLUDEALL
            foreach (var optionsExcludeAllEntry in optionsExcludeAllIndex)
            {
                if (!PerformGroupCheck(optionsExcludeAllIndex, optionsGroupIndex, optionsHostGroupIndex,
                                       tuple => (tuple.Item1, tuple.Item2), "EXCLUDEALL"))
                {
                    return;
                }
            }
        }
        private void PrintUsefulInformation()
        {
            OutputTextBlock.Text += "\r\nPlease note: if you did not specify a license for an INCLUDE or RESERVE line, it will subtract the seat from the first " +
                "license the product appears on. Product names are listed with their FlexLM names.\r\n\r\n";

            if (cnuIsUsed)
            {
                OutputTextBlock.Text += "\r\nYour license file contains a Counted Named User license. An options file is likely unnecessary for this license offering.\r\n\r\n";
            }
            // Don't need to print out too much information.
            bool overdraftCNWarningHit = false;

            foreach (var licenseFileEntry in licenseFileIndex)
            {
                Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                string productName = licenseFileData.Item1;
                int seatCount = licenseFileData.Item2;
                string licenseOffering = licenseFileData.Item4;
                string licenseNumber = licenseFileData.Item5;

                if (licenseOffering.Contains("CNU") && seatCount == 9999999)
                {
                    OutputTextBlock.Text += $"The product {productName} has unlimited seats on license {licenseNumber}.\r\n";
                }
                else
                {
                    if (licenseOffering == "lo=CN" && seatCount < 0)
                    {
                        if (!overdraftCNWarningHit)
                        {
                            OutputTextBlock.Text += $"\r\nWARNING: You have specified more users on license {licenseNumber} for the product {productName} than you have seats for. " +
                                $"If every user included was using the product at once then the seat count would technically be at {seatCount}. " +
                                "This is acceptable since it is a Concurrent license, but if all seats are being used, then a user you've specified to be able to use the product will not be able to " +
                                "access this product until a seat is available.\r\n\r\nTHE WARNING ABOVE WILL ONLY PRINT ONCE.\r\n";
                            overdraftCNWarningHit = true;
                        }
                        else
                        {
                            OutputTextBlock.Text += $"You have specified more users on Concurrent license {licenseNumber} for the product {productName} than you have seats for (technically counting at {seatCount} seats.)\r\n";
                        }
                    }
                    else
                    {
                        OutputTextBlock.Text += $"The product {productName} has {seatCount} unassigned seats on license {licenseNumber}.\r\n";
                    }
                }
            }
        }

        private void CollectProductInformation()
        {
            string[] licenseFileContentsLines = System.IO.File.ReadAllLines(LicenseFileLocationTextBox.Text);

            //Filter the things that don't matter.
            string[] filteredLicenseFileLines = licenseFileContentsLines.Where(line => !line.TrimStart().StartsWith("#")
            && !string.IsNullOrWhiteSpace(line)).ToArray();
            string filteredLicenseFileContents = string.Join(Environment.NewLine, filteredLicenseFileLines);

            // Remove the line breaks to make life easier.
            string lineBreaksToRemove = "\\\r\n\t";
            filteredLicenseFileContents = filteredLicenseFileContents.Replace(lineBreaksToRemove, string.Empty);

            // Put it back together!
            filteredLicenseFileLines = filteredLicenseFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // Get the things we care about from the license file so that we can go through the options file appropriately.
            string productName = string.Empty;
            int seatCount = 0;
            string plpLicenseNumber = string.Empty;

            for (int lineIndex = 0; lineIndex < filteredLicenseFileLines.Length; lineIndex++)
            {
                string line = filteredLicenseFileLines[lineIndex];

                if (line.TrimStart().StartsWith("INCREMENT"))
                {
                    string[] lineParts = line.Split(' ');
                    productName = lineParts[1];
                    int productVersion = int.Parse(lineParts[3]);
                    string productExpirationDate = lineParts[4];
                    string productKey = lineParts[6];
                    string licenseOffering = "licenseOfferingIsBroken";
                    string licenseNumber = "licenseNumberisBroken";
                    _ = int.TryParse(lineParts[5], out seatCount);
                    string rawSeatCount = lineParts[5];

                    // If you're using a PLP license, then we don't care about this product.
                    if (productName == "TMW_Archive")
                    {
                        containsPLP = true;
                    }

                    // License number.
                    string pattern = @"asset_info=(\d+)";

                    if (line.Contains("asset_info="))
                    {
                        Regex regex = new Regex(pattern);
                        Match match = regex.Match(line);

                        if (match.Success)
                        {
                            licenseNumber = match.Groups[1].Value;
                        }
                    }
                    else if (line.Contains("SN="))
                    {
                        pattern = @"SN=(\d+)";
                        Regex regex = new Regex(pattern);
                        Match match = regex.Match(line);

                        if (match.Success)
                        {
                            licenseNumber = match.Groups[1].Value;
                        }
                        if (productName == "TMW_Archive")
                        {
                            plpLicenseNumber = licenseNumber;
                            continue;
                        }
                    }
                    else if (containsPLP && productName.Contains("PolySpace")) // This is the best guess we can make if you're using a PLP-era product.
                    {
                        licenseNumber = plpLicenseNumber;
                    }
                    else
                    {
                        analysisFailed = true;
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: the license number was not found for the product {productName}.";
                        errorWindow.ShowDialog();
                        return;
                    }

                    // License offering.
                    if (line.Contains("lo="))
                    {
                        if (line.Contains("lo=CN:"))
                        {
                            licenseOffering = "lo=CN";
                        }
                        else if (line.Contains("lo=CNU"))
                        {
                            licenseOffering = "CNU";
                        }
                        else if (line.Contains("lo=NNU"))
                        {
                            licenseOffering = "NNU";
                        }
                        else if (line.Contains("lo=TH"))
                        {
                            if (!line.Contains("USER_BASED="))
                            {
                                licenseOffering = "lo=CN";
                            }
                            else
                            {
                                analysisFailed = true;
                                OutputTextBlock.Text = string.Empty;
                                ErrorWindow errorWindow = new();
                                errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it is formatted incorrectly. " +
                                    $"{productName}'s license offering is being read as Total Headcount, but also Network Named User, which doesn't exist.";
                                errorWindow.ShowDialog();
                                return;
                            }
                        }
                        else
                        {
                            analysisFailed = true;
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: the product {productName} has an invalid license offering.";
                            errorWindow.ShowDialog();
                            return;
                        }
                    }
                    else if (line.Contains("lr=") || containsPLP && !line.Contains("asset_info=")) // Figure out your trial or PLP's license offering.
                    {
                        if (seatCount > 0)
                        {
                            if (line.Contains("USER_BASED"))
                            {
                                licenseOffering = "NNU";
                            }
                            else
                            {
                                if (containsPLP && !line.Contains("asset_info="))
                                {
                                    licenseOffering = "lo=DC"; // See PLP-era explaination below.
                                }
                                else
                                {
                                    licenseOffering = "lo=CN";
                                }
                            }
                        }
                        // This means you're likely using a macOS or Linux PLP-era license, which CAN use an options file... I think it has to.
                        else if (containsPLP && !line.Contains("asset_info="))
                        {
                            licenseOffering = "lo=IN";
                            seatCount = 1;
                        }
                        else
                        {
                            analysisFailed = true;
                            OutputTextBlock.Text = string.Empty;
                            ErrorWindow errorWindow = new();
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: the product {productName} comes from an Individual " +
                                $"or Designated Computer license, which cannot use an options file.";
                            errorWindow.ShowDialog();
                            return;
                        }
                    }
                    else
                    {
                        analysisFailed = true;
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        if (line.Contains("PLATFORMS=x"))
                        {
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: the product {productName} comes from an Individual " +
                                $"or Designated Computer license generated from a PLP on Windows, which cannot use an options file.";
                        }
                        else
                        {
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: the product {productName} has an valid license offering.";
                        }
                        errorWindow.ShowDialog();
                        return;
                    }

                    // Check the product's expiration date. Year 0000 means perpetual.
                    if (productExpirationDate == "01-jan-0000")
                    {
                        productExpirationDate = "01-jan-2999";
                    }

                    // Convert/parse the productExpirationDate string to a DateTime object.
                    DateTime expirationDate = DateTime.ParseExact(productExpirationDate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);

                    // Get the current system date.
                    DateTime currentDate = DateTime.Now.Date;

                    if (expirationDate < currentDate)
                    {
                        analysisFailed = true;
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: The product {productName} on license number " +
                            $"{licenseNumber} expired on {productExpirationDate}. Please update your license file appropriately before proceeding.";
                        errorWindow.ShowDialog();
                        return;
                    }

                    if (licenseOffering.Contains("NNU"))
                    {
                        if (seatCount != 1 && !containsPLP)
                        {
                            seatCount /= 2;
                        }                        
                    }

                    // Technically infinite. This should avoid at least 1 unnecessary error report.
                    if (licenseOffering.Contains("CNU") && (seatCount == 0))
                    {
                        seatCount = 9999999;

                        if (licenseOffering.Contains("CNU"))
                        {
                            cnuIsUsed = true;
                        }
                    }

                    if (licenseOffering == "lo=CN" && (seatCount == 0) && licenseNumber == "220668")
                    {
                        analysisFailed = true;
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        if ((productVersion <= 18) || (productName.Contains("Polyspace") && productVersion <= 22))
                        {
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: it contains a Designated Computer or Individual license, {licenseNumber}.";
                        }
                        else
                        {
                            errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: it contains a Designated Computer license, {licenseNumber}, " +
                            "that is incorrectly labeled as a Concurrent license.";
                        }

                        errorWindow.ShowDialog();
                        return;
                    }

                    if (!licenseOffering.Contains("CNU") && rawSeatCount == "uncounted")
                    {
                        analysisFailed = true;
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it contains an Individual or Designated Computer license, " +
                            $"which cannot use an options file. The license number is question is {licenseNumber}.";
                        errorWindow.ShowDialog();
                        return;
                    }

                    if (seatCount < 1 && line.Contains("asset_info="))
                    {
                        analysisFailed = true;
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: {productName} on license {licenseNumber} is reading with a seat count of zero or less.";
                        errorWindow.ShowDialog();
                        return;
                    }

                    // Before proceeding, make sure the values we've collected are valid.
                    if (string.IsNullOrWhiteSpace(productName))
                    {
                        analysisFailed = true;
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: a product name is being detected as blank on {licenseNumber}.";
                        errorWindow.ShowDialog();
                        return;
                    }

                    if (licenseNumber.Contains("broken") || string.IsNullOrWhiteSpace(licenseNumber) || Regex.IsMatch(licenseNumber, @"^[^Rab_\d]+$"))
                    {
                        analysisFailed = true;
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: an invalid license number, {licenseNumber}, is detected for {productName}.";
                        errorWindow.ShowDialog();
                        return;
                    }

                    if (licenseOffering.Contains("broken") || string.IsNullOrWhiteSpace(licenseOffering))
                    {
                        analysisFailed = true;
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: a license offering could not be detected for {productName} " +
                            $"on license number {licenseNumber}.";
                        errorWindow.ShowDialog();
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(productKey))
                    {
                        analysisFailed = true;
                        OutputTextBlock.Text = string.Empty;
                        ErrorWindow errorWindow = new();
                        errorWindow.ErrorTextBlock.Text = $"There is an issue with the selected license file: a product key could not be detected for {productName} " +
                            $"on license number {licenseNumber}.";
                        errorWindow.ShowDialog();
                        return;
                    }
                    if (debug)
                    {
                        OutputTextBlock.Text += $"INCREMENT {productName}: {seatCount} {productKey} {licenseOffering} {licenseNumber}\r\n";
                    }
                    licenseFileIndex[lineIndex] = Tuple.Create(productName, seatCount, productKey, licenseOffering, licenseNumber);
                }
            }
        }
    }
}
