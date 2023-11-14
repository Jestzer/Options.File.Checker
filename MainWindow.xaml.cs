using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Options.File.Checker.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // INCLUDE dictionary from the options file.
        Dictionary<int, Tuple<string, string?, string?, string, string>> optionsIncludeIndex = new Dictionary<int, Tuple<string, string?, string?, string, string>>();

        // GROUP dictionary from the options file.
        Dictionary<int, Tuple<string, string, int>> optionsGroupIndex = new Dictionary<int, Tuple<string, string, int>>();

        // License file dictionary.
        Dictionary<int, Tuple<string, int, string, string, string>> licenseFileIndex = new Dictionary<int, Tuple<string, int, string, string, string>>();

        // End operations if needed.
        bool analysisOfServerAndDaemonLinesFailed = false;
        bool analysisOfOptionsFileProductsFailed = false;

        public MainWindow()
        {
            InitializeComponent();
            LicenseFileLocationTextBox.Text = Properties.Settings.Default.LicenseFilePathSetting;
            OptionsFileLocationTextBox.Text = Properties.Settings.Default.OptionsFilePathSetting;
        }

        // The efforts one must go through to make this UI not look horrible is truly incredible.
        // Implement window dragging with the mouse.
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between maximized and not.
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void LicenseFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a License File";
            openFileDialog.Filter = "License Files (*.dat;*.lic)|*.dat;*.lic|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                LicenseFileLocationTextBox.Text = selectedFile;

                // I probably should put in more checks than this, but I imagine this'll take care of 99.9% of invalid files.
                bool containsINCREMENT = System.IO.File.ReadAllText(selectedFile).Contains("INCREMENT");

                if (!containsINCREMENT)
                {
                    MessageBox.Show("The selected file is either not a license file or is corrupted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                string fileContents = System.IO.File.ReadAllText(selectedFile);
                if (fileContents.Contains("lo=IN") || fileContents.Contains("lo=DC") || fileContents.Contains("lo=CIN"))
                {
                    MessageBox.Show("The file you've selected is likely an Individual or Designated Computer license (or has been tampered), which cannot use " +
                        "an options file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                bool containsCONTRACT_ID = System.IO.File.ReadAllText(selectedFile).Contains("CONTRACT_ID=");
                if (containsCONTRACT_ID)
                {
                    MessageBox.Show("The selected license file is not a MathWorks license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                bool missingServer = System.IO.File.ReadAllText(selectedFile).Contains("SERVER");
                bool missingDaemon = System.IO.File.ReadAllText(selectedFile).Contains("DAEMON");
                if (!missingDaemon && !missingServer)
                {
                    MessageBox.Show("The selected license file is missing the SERVER and/or DAEMON line.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                string fileContent = System.IO.File.ReadAllText(selectedFile);
                bool isEmptyOrWhiteSpace = string.IsNullOrWhiteSpace(fileContent);

                if (isEmptyOrWhiteSpace)
                {
                    MessageBox.Show("The selected file is either empty or only contains white space.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    OptionsFileLocationTextBox.Text = string.Empty;
                    return;
                }
            }
            Properties.Settings.Default.OptionsFilePathSetting = OptionsFileLocationTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void LicenseFileLocationTextBox_TextChanged(object sender, EventArgs e)
        {
            string licensePath = LicenseFileLocationTextBox.Text;
            bool isLicenseFileValid = System.IO.File.Exists(licensePath);
            bool isOptionsFileValid = System.IO.File.Exists(OptionsFileLocationTextBox.Text);
            AnalyzerButton.IsEnabled = isLicenseFileValid && isOptionsFileValid;
        }

        private void OptionsFileLocationTextBox_TextChanged(object sender, EventArgs e)
        {
            string optionsPath = OptionsFileLocationTextBox.Text;
            bool isLicenseFileValid = System.IO.File.Exists(LicenseFileLocationTextBox.Text);
            bool isOptionsFileValid = System.IO.File.Exists(optionsPath);
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
            optionsGroupIndex.Clear();
            licenseFileIndex.Clear();
            analysisOfServerAndDaemonLinesFailed = false;
            analysisOfOptionsFileProductsFailed = false;
            OutputTextBlock.Text = null;
            bool excludeLinesAreUsed = false;
            bool excludeAllLinesAreUsed = false;
            bool hostGroupsAreUsed = false;

            try
            {
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
                bool optionsFilsIsEmptyOrWhiteSpace = string.IsNullOrWhiteSpace(filteredOptionsFileContents);
                if (optionsFilsIsEmptyOrWhiteSpace)
                {
                    MessageBox.Show("The selected options file is either empty or only contains white space.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    OptionsFileLocationTextBox.Text = string.Empty;
                    return;
                }

                bool containsINCREMENT = System.IO.File.ReadAllText(LicenseFileLocationTextBox.Text).Contains("INCREMENT");
                if (!containsINCREMENT)
                {
                    MessageBox.Show("The selected file is either not a license file or is corrupted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                if (filteredLicenseFileContents.Contains("lo=IN") || filteredLicenseFileContents.Contains("lo=DC") || filteredLicenseFileContents.Contains("lo=CIN"))
                {
                    MessageBox.Show("The file you've selected is likely an Individual or Designated Computer license (or has been tampered), which cannot use " +
                        "an options file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                bool containsCONTRACT_ID = System.IO.File.ReadAllText(LicenseFileLocationTextBox.Text).Contains("CONTRACT_ID=");
                if (containsCONTRACT_ID)
                {
                    MessageBox.Show("The selected license file is not a MathWorks license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                // Congrats. You passed.

                // Look for issues with your SERVER or DAEMON lines
                AnalyzeServerAndDaemonLine();
                if (analysisOfServerAndDaemonLinesFailed)
                {
                    OutputTextBlock.Text = null;
                    return;
                }

                // Get the things we care about from the license file so that we can go through the options file appropriately.

                string? productName = null;
                int seatCount = 0;

                for (int lineIndex = 0; lineIndex < filteredLicenseFileLines.Length; lineIndex++)
                {
                    string line = filteredLicenseFileLines[lineIndex];

                    if (line.TrimStart().StartsWith("INCREMENT"))
                    {
                        string[] lineParts = line.Split(' ');
                        productName = lineParts[1];
                        string productKey = lineParts[6];
                        string? licenseOffering = null;
                        string licenseNumber = "this_is_a_bug";
                        int.TryParse(lineParts[5], out seatCount);

                        // You bastards put the product key (that nobody ever uses but I still have to check just in case) on a different line /sometimes/.
                        if (productKey == "\\")
                        {
                            // Check if there is a next line.
                            if (lineIndex + 1 < filteredLicenseFileLines.Length)
                            {
                                string nextLine = filteredLicenseFileLines[lineIndex + 1];
                                // Parse the next line as needed
                                string[] nextLineParts = nextLine.Split(' ');
                                string fixedProductKey = nextLineParts[0];
                                fixedProductKey = fixedProductKey.Replace("\t", "");
                                productKey = fixedProductKey;
                                lineIndex++; // Move to the next line since it has been processed. I don't know why this needs to only be in this If statement.
                            }
                            else
                            {
                                MessageBox.Show("Invalid license file format. Product key is missing for at least 1 product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                OptionsFileLocationTextBox.Text = string.Empty;
                                return;
                            }
                        }

                        if (lineIndex + 1 < filteredLicenseFileLines.Length)
                        {
                            // Okay now do the thing to get the license offering.
                            string vendorLine = filteredLicenseFileLines[lineIndex + 1];
                            string[] vendorLineParts = vendorLine.Split(':');
                            licenseOffering = vendorLineParts[4];

                            if (licenseOffering.Contains("lr="))
                            {
                                MessageBox.Show($"Trial licenses are currently unsupported. Product in question: {productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                OptionsFileLocationTextBox.Text = string.Empty;
                                return;
                            }

                            if (licenseOffering.Contains("ei="))
                            {
                                licenseOffering = vendorLineParts[3];
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid license file format. License Offering is missing for at least 1 product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            OptionsFileLocationTextBox.Text = string.Empty;
                            return;
                        }

                        if (licenseOffering.Contains("NNU"))
                        {
                            seatCount /= 2;
                        }

                        lineIndex++;

                        // Get the license number.
                        if (lineIndex + 1 < filteredLicenseFileLines.Length)
                        {
                            string licenseNumberLine = filteredLicenseFileLines[lineIndex + 1];
                            string[] licenseNumberLineParts = licenseNumberLine.Split(" ");
                            string unprocessedlicenseNumber = licenseNumberLineParts[1];
                            licenseNumber = unprocessedlicenseNumber.Replace("asset_info=", "");
                            if (licenseNumber.Contains("USER_BASED="))
                            {
                                unprocessedlicenseNumber = licenseNumberLineParts[3];
                                licenseNumber = unprocessedlicenseNumber.Replace("asset_info=", "");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid license file format. License Number is missing from at least 1 product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            OptionsFileLocationTextBox.Text = string.Empty;
                            return;
                        }

                        //I guess we couldn't just put the license number in one place.
                        if (licenseNumber.Contains("ISSUED="))
                        {
                            if (lineIndex + 1 < filteredLicenseFileLines.Length)
                            {
                                lineIndex++;
                                string nextLine = filteredLicenseFileLines[lineIndex + 1];
                                int startIndex = nextLine.IndexOf("SN=");
                                string unprocessedLicenseNumber = nextLine.Substring(startIndex + 3, 8); // I imagine someday that 8 will need to be increased.
                                licenseNumber = Regex.Replace(unprocessedLicenseNumber, "[^0-9]", "");
                            }
                            else
                            {
                                MessageBox.Show("Invalid license file format. License Number is missing from at least 1 product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                OptionsFileLocationTextBox.Text = string.Empty;
                                return;
                            }
                        }

                        //OutputTextBlock.Text += $"{productName}: {seatCount} {productKey} {licenseOffering} {licenseNumber}\r\n";
                        licenseFileIndex[lineIndex] = Tuple.Create(productName, seatCount, productKey, licenseOffering, licenseNumber);
                    }
                }

                // Go through the Options File
                // Create a dictionary of each group, the number of users specified in them, and their names
                // With things like HOST_GROUP, IP addresses, and wildcards, give warning messages. Things like exceeding seat counts
                // will receive errors
                // Collect the INCLUDE and RESERVE lines
                // Collect the EXCLUDE and EXCLUDEALL lines. Report them as warnings
                // If the seat count is negative, report it as an error (unless the product's license offering is CNU).

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
                        groupName = lineParts[1];
                        groupUsers = string.Join(" ", lineParts.Skip(2)).TrimEnd();
                        groupUserCount = groupUsers.Split(' ').Length;

                        OutputTextBlock.Text += $"{optionsGroupLineIndex}, {groupName}: {groupUsers}. User count: {groupUserCount}\r\n";
                        optionsGroupIndex[optionsGroupLineIndex] = Tuple.Create(groupName, groupUsers, groupUserCount);
                    }
                }

                // INCLUDE dictionary.
                for (int optionsIncludeLineIndex = 0; optionsIncludeLineIndex < filteredOptionsFileLines.Length; optionsIncludeLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsIncludeLineIndex];
                    string includeProductName = "brokenIncludeProductName";
                    string? includeLicenseNumber = "brokenIncludeLicenseNumber";
                    string? includeProductKey = "brokenIncludeProductKey";
                    string includeClientType = "brokenIncludeClientType";
                    string includeClientSpecified = "brokenIncludeClientSpecified";
                    if (line.TrimStart().StartsWith("INCLUDE "))
                    {
                        string[] lineParts = line.Split(' ');
                        includeProductName = lineParts[1];
                        if (includeProductName.Contains('"'))
                        {
                            includeProductName = includeProductName.Replace("\"", "");
                            includeLicenseNumber = lineParts[2];
                            if (includeLicenseNumber.Contains("key="))
                            {
                                includeProductKey = lineParts[2];
                                string unfixedIncludeProductKey = includeProductKey;
                                string quotedIncludeProductKey = unfixedIncludeProductKey.Replace("key=", "");
                                includeProductKey = quotedIncludeProductKey.Replace("\"", "");
                                includeLicenseNumber = null;
                            }
                            else
                            {
                                string unfixedIncludeLicenseNumber = includeLicenseNumber;
                                string quoteIncludeLicenseNumber = unfixedIncludeLicenseNumber.Replace("asset_info=", "");
                                includeLicenseNumber = quoteIncludeLicenseNumber.Replace("\"", "");
                                includeProductKey = null;
                            }

                            includeClientType = lineParts[3];
                            includeClientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                        }
                        // In case you decided to use a : instead of ""...
                        else if (includeProductName.Contains(':'))
                        {
                            string[] colonParts = includeProductName.Split(":");
                            if (colonParts.Length != 2)
                            {
                                MessageBox.Show($"One of your INCLUDE lines has a stray colon: {includeProductName}...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                OptionsFileLocationTextBox.Text = string.Empty;
                                return;
                            }
                            includeProductName = colonParts[0];
                            if (colonParts[1].Contains("key="))
                            {
                                string unfixedIncludeProductKey = colonParts[1];
                                includeProductKey = unfixedIncludeProductKey.Replace("key=", "");

                            }
                            else
                            {
                                string unfixedIncludeLicenseNumber = colonParts[1];
                                includeLicenseNumber = unfixedIncludeLicenseNumber.Replace("asset_info=", "");
                            }
                            includeClientType = lineParts[2];
                            includeClientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                        }
                        else
                        {
                            includeClientType = lineParts[2];
                            includeClientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                            includeLicenseNumber = null;
                            includeProductKey = null;
                        }

                        OutputTextBlock.Text += $"{includeProductName}: SN:{includeLicenseNumber}. PK:{includeProductKey} CT:{includeClientType} CS: {includeClientSpecified}\r\n";
                        optionsIncludeIndex[optionsIncludeLineIndex] = Tuple.Create(includeProductName, includeLicenseNumber, includeProductKey, includeClientType, includeClientSpecified);
                    }
                }
                OutputTextBlock.Text += $"Is case sensitivity turned off?: {caseSensitivity.ToString()}\r\n";

                // RESERVE dictionary.
                Dictionary<string, Tuple<string, string, string>> optionsReserveIndex = new Dictionary<string, Tuple<string, string, string>>();
                for (int optionsReserveLineIndex = 0; optionsReserveLineIndex < filteredOptionsFileLines.Length; optionsReserveLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsReserveLineIndex];
                    string reserveSeats = "broken-reserveSeats";
                    string reserveProductName = "broken-reserveProductName";
                    string reserveClientType = "broken-reserveClientType";
                    string reserveClientSpecified = "broken-reserveClientSpecified";

                    if (line.TrimStart().StartsWith("RESERVE "))
                    {
                        string[] lineParts = line.Split(' ');
                        reserveSeats = lineParts[1];
                        reserveProductName = lineParts[2];
                        reserveClientType = lineParts[3];
                        reserveClientSpecified = string.Join(" ", lineParts.Skip(4));

                        OutputTextBlock.Text += $"Seats reserved: {reserveSeats}. Product: {reserveProductName}. CT: {reserveClientType}. CS: {reserveClientSpecified}\r\n";
                        optionsReserveIndex[reserveProductName] = Tuple.Create(reserveSeats, reserveClientType, reserveClientSpecified);
                    }
                }

                // MAX dictionary.
                Dictionary<string, Tuple<string, string, string>> optionsMaxIndex = new Dictionary<string, Tuple<string, string, string>>();
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
                        maxSeats = lineParts[1];
                        maxProductName = lineParts[2];
                        maxClientType = lineParts[3];
                        maxClientSpecified = string.Join(" ", lineParts.Skip(4));

                        OutputTextBlock.Text += $"Max seats: {maxSeats}. Product: {maxProductName}. CT: {maxClientType}. CS: {maxClientSpecified}\r\n";
                        optionsMaxIndex[maxProductName] = Tuple.Create(maxSeats, maxClientType, maxClientSpecified);
                    }
                }

                // EXCLUDE dictionary. Make changes to this based on the INCLUDE changes I've made since.
                Dictionary<string, Tuple<string?, string?, string, string>> optionsExcludeIndex = new Dictionary<string, Tuple<string?, string?, string, string>>();
                for (int optionsExcludeLineIndex = 0; optionsExcludeLineIndex < filteredOptionsFileLines.Length; optionsExcludeLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsExcludeLineIndex];
                    string excludeProductName = "brokenProductName";
                    string? excludeLicenseNumber = "brokenLicenseNumber";
                    string? excludeProductKey = "brokenProductKey";
                    string excludeClientType = "brokenClientType";
                    string excludeClientSpecified = "brokenClientSpecified";
                    if (line.TrimStart().StartsWith("EXCLUDE "))
                    {
                        excludeLinesAreUsed = true;
                        string[] lineParts = line.Split(' ');
                        excludeProductName = lineParts[1];
                        if (excludeProductName.Contains('"'))
                        {
                            excludeProductName = excludeProductName.Replace("\"", "");
                            excludeLicenseNumber = lineParts[2];
                            if (excludeLicenseNumber.Contains("key="))
                            {
                                excludeProductKey = lineParts[2];
                                string unfixedExcludeProductKey = excludeProductKey;
                                string quotedExcludeProductKey = unfixedExcludeProductKey.Replace("key=", "");
                                excludeProductKey = quotedExcludeProductKey.Replace("\"", "");
                                excludeLicenseNumber = null;
                            }
                            else
                            {
                                string unfixedExcludeLicenseNumber = excludeLicenseNumber;
                                string quoteExcludeLicenseNumber = unfixedExcludeLicenseNumber.Replace("asset_info=", "");
                                excludeLicenseNumber = quoteExcludeLicenseNumber.Replace("\"", "");
                                excludeProductKey = null;
                            }

                            excludeClientType = lineParts[3];
                            excludeClientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                        }
                        else
                        {
                            excludeClientType = lineParts[2];
                            excludeClientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                            excludeLicenseNumber = null;
                            excludeProductKey = null;
                        }

                        OutputTextBlock.Text += $"{excludeProductName}: SN:{excludeLicenseNumber}. PK:{excludeProductKey} CT:{excludeClientType} CS: {excludeClientSpecified}\r\n";
                        optionsExcludeIndex[excludeProductName] = Tuple.Create(excludeLicenseNumber, excludeProductKey, excludeClientType, excludeClientSpecified);
                    }
                }

                // HOST_GROUP dictionary.
                Dictionary<string, Tuple<string>> optionsHostGroupIndex = new Dictionary<string, Tuple<string>>();
                for (int optionsHostGroupLineIndex = 0; optionsHostGroupLineIndex < filteredOptionsFileLines.Length; optionsHostGroupLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsHostGroupLineIndex];
                    string hostGroupName = "broken-hostGroupName";
                    string hostGroupClientSpecified = "broken-hostGroupClientSpecified";

                    if (line.TrimStart().StartsWith("HOST_GROUP "))
                    {
                        hostGroupsAreUsed = true;
                        string[] lineParts = line.Split(' ');
                        hostGroupName = lineParts[1];
                        hostGroupClientSpecified = string.Join(" ", lineParts.Skip(2));

                        OutputTextBlock.Text += $"HOST_GROUP Name: {hostGroupName}. Clients: {hostGroupClientSpecified}\r\n";
                        optionsHostGroupIndex[hostGroupName] = Tuple.Create(hostGroupClientSpecified);

                    }
                }

                // EXCLUDEALL
                for (int optionsExcludeAllLineIndex = 0; optionsExcludeAllLineIndex < filteredOptionsFileLines.Length; optionsExcludeAllLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsExcludeAllLineIndex];

                    if (line.TrimStart().StartsWith("HOST_GROUP "))
                    {
                        excludeAllLinesAreUsed = true;
                        return;
                    }
                }
                // INCLUDEALL
                //

                // Output some warning messages, if needed.
                if (excludeLinesAreUsed)
                {
                    OutputTextBlock.Text += "\r\nYou are using at least 1 EXCLUDE line in your options file. If a user is encountering an License Manager Error -38, " +
                        "then this is because they are listed on an EXCLUDE or EXCLUDEALL line in the options file.\r\n";
                }

                if (excludeAllLinesAreUsed)
                {
                    OutputTextBlock.Text += "\r\nYou are using at least 1 EXCLUDEALL line in your options file. If a user is encountering an License Manager Error -38, " +
                        "then this is because they are listed on an EXCLUDE or EXCLUDEALL line in the options file.\r\n";
                }

                if (hostGroupsAreUsed)
                {
                    OutputTextBlock.Text += "\r\nYou are using at least 1 HOST_GROUP line in your options file. ";
                }

                CheckForValidProducts();
                if (analysisOfOptionsFileProductsFailed)
                {
                    OutputTextBlock.Text = null;
                    return;
                }
                CalculateRemainingSeats();
            }
            catch (Exception ex)
            { MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            return;
        }

        private void CalculateRemainingSeats()
        {
            foreach (var optionsIncludeEntry in optionsIncludeIndex)
            {
                int optionsIncludeLineIndex = optionsIncludeEntry.Key;
                Tuple<string, string?, string?, string, string> optionsIncludeData = optionsIncludeEntry.Value;

                // Load INCLUDE specifications.
                string includeProductName = optionsIncludeData.Item1;
                string? includeLicenseNumber = optionsIncludeData.Item2;
                string? includeProductKey = optionsIncludeData.Item3;
                string includeClientType = optionsIncludeData.Item4;
                string includeClientSpecified = optionsIncludeData.Item5;

                foreach (var licenseFileEntry in licenseFileIndex)
                {
                    int lineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                    string productName = licenseFileData.Item1;
                    string licenseNumber = licenseFileData.Item5;

                    // Check for a matching includeProductName and productName.
                    if (includeProductName == productName)
                    {
                        if (includeLicenseNumber == licenseNumber)
                        {
                            // Putting this here should make the seats counts divided by product and license number.
                            int seatCount = licenseFileData.Item2;

                            if (includeClientType == "USER")
                            {
                                // Check that a user has actually been specified.
                                bool isEmptyOrWhiteSpace = string.IsNullOrWhiteSpace(includeClientSpecified);

                                if (isEmptyOrWhiteSpace)
                                {
                                    MessageBox.Show($"You have specified a USER to be able to use {productName} for license {licenseNumber}, but you did not define the USER.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    OutputTextBlock.Text = null;
                                    return;
                                }

                                // Subtract 1 from seatCount, since you only specified a single user.
                                seatCount--;

                                // Error out if the seat count is negative.
                                if (seatCount < 0)
                                {
                                    MessageBox.Show($"You have specified too many users to be able to use {productName} for license {licenseNumber}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    OutputTextBlock.Text = null;
                                    return;
                                }

                                // Update the seatCount in the licenseFileData tuple.
                                licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                // Update the seatCount in the licenseFileIndex dictionary.
                                licenseFileIndex[lineIndex] = licenseFileData;

                                // Update the OutputTextBlock.Text with the new seatCount value.
                                OutputTextBlock.Text += $"Remaining seat count for {productName} is now {seatCount} on license {licenseNumber}\r\n";
                            }
                            if (includeClientType == "GROUP")
                            {

                                // Check that a group has actually been specified.
                                bool isEmptyOrWhiteSpace = string.IsNullOrWhiteSpace(includeClientSpecified);

                                if (isEmptyOrWhiteSpace)
                                {
                                    MessageBox.Show($"You have specified a GROUP to be able to use {productName} for license {licenseNumber}, but you did not specify which GROUP.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    OutputTextBlock.Text = null;
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
                                            MessageBox.Show($"You have specified too many users to be able to use {productName} for license {licenseNumber}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                            OutputTextBlock.Text = null;
                                            return;
                                        }

                                        // Update the seatCount in the licenseFileData tuple.
                                        licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                        // Update the seatCount in the licenseFileIndex dictionary.
                                        licenseFileIndex[lineIndex] = licenseFileData;

                                        // Update the OutputTextBlock.Text with the new seatCount value.
                                        OutputTextBlock.Text += $"Line index: {optionsGroupLineIndex}. Remaining seat count for {productName} is now {seatCount} on license {licenseNumber}\r\n";
                                    }
                                }
                            }
                        }
                        else
                        {
                            // We'll get to the uncategorized products later.
                            OutputTextBlock.Text += $"Matching product {productName} found, but the license numbers don't match.\r\n";
                        }
                    }
                }
            }

            // Now we have to filter out the stragglers (no license number specified.)
            foreach (var optionsIncludeEntry in optionsIncludeIndex)
            {
                string productName = "brokenProductNamePart2ofCalculateRemainingSeats";
                int optionsIncludeLineIndex = optionsIncludeEntry.Key;
                Tuple<string, string?, string?, string, string> optionsIncludeData = optionsIncludeEntry.Value;

                // Load INCLUDE specifications.
                string includeProductName = optionsIncludeData.Item1;
                string? includeLicenseNumber = optionsIncludeData.Item2;
                string? includeProductKey = optionsIncludeData.Item3;
                string includeClientType = optionsIncludeData.Item4;
                string includeClientSpecified = optionsIncludeData.Item5;

                // Skip INCLUDE entries with specified license numbers. We already accounted for them.
                bool includeHasNoLicenseNumber = string.IsNullOrWhiteSpace(includeLicenseNumber);

                if (!includeHasNoLicenseNumber)
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
                                bool isEmptyOrWhiteSpace = string.IsNullOrWhiteSpace(includeClientSpecified);

                                if (isEmptyOrWhiteSpace)
                                {
                                    MessageBox.Show($"You have specified a USER to be able to use {productName}, but you did not define the USER.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    OutputTextBlock.Text = null;
                                    return;
                                }

                                // Subtract 1 from seatCount, since you only specified a single user.
                                seatCount--;

                                // Error out if the seat count is negative.
                                if (seatCount < 0)
                                {
                                    MessageBox.Show($"You have specified too many users to be able to use {productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    OutputTextBlock.Text = null;
                                    return;
                                }

                                // Update the seatCount in the licenseFileData tuple.
                                licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                // Update the seatCount in the licenseFileIndex dictionary.
                                licenseFileIndex[lineIndex] = licenseFileData;

                                // Update the OutputTextBlock.Text with the new seatCount value.
                                OutputTextBlock.Text += $"Remaining seat count for {productName} is now {seatCount} for no specific license.\r\n";
                            }
                            if (includeClientType == "GROUP")
                            {

                                // Check that a group has actually been specified.
                                bool isEmptyOrWhiteSpace = string.IsNullOrWhiteSpace(includeClientSpecified);

                                if (isEmptyOrWhiteSpace)
                                {
                                    MessageBox.Show($"You have specified a GROUP to be able to use {productName}, but you did not specify which GROUP.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    OutputTextBlock.Text = null;
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
                                            MessageBox.Show($"You have specified too many users to be able to use {productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                            OutputTextBlock.Text = null;
                                            return;
                                        }

                                        // Update the seatCount in the licenseFileData tuple.
                                        licenseFileData = Tuple.Create(productName, seatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);

                                        // Update the seatCount in the licenseFileIndex dictionary.
                                        licenseFileIndex[lineIndex] = licenseFileData;

                                        // Update the OutputTextBlock.Text with the new seatCount value.
                                        OutputTextBlock.Text += $"Remaining seat count for {productName} is now {seatCount} for no specific license.\r\n";
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
                    MessageBox.Show($"You have specified too many users to be able to use {includeProductName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    OutputTextBlock.Text = null;
                    return;
                }
            }
        }

        private void AnalyzeServerAndDaemonLine()
        {
            string[] licenseFileContentsLines = System.IO.File.ReadAllLines(LicenseFileLocationTextBox.Text);
            string[] filteredLicenseFileLines = licenseFileContentsLines.Where(line => !line.TrimStart().StartsWith("#")
            && !string.IsNullOrWhiteSpace(line)).ToArray();
            string filteredLicenseFileContents = string.Join(Environment.NewLine, filteredLicenseFileLines);

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
                        MessageBox.Show("Your SERVER line(s) need to come before your products are listed in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }
                    serverLineCount++;

                    string[] lineParts = line.Split(' ');
                    string serverHostnameOrIPAddress = lineParts[1];
                    string serverHostID = lineParts[2];

                    // Set this now since it'll be used twice.
                    string unspecifiedServerPortMessage = "Warning: You have not specified a port number for the SERVER. " +
                            "This is acceptable if you are defining it with lmadmin or if you're okay with " +
                            "FlexLM picking the port number for you. However, if you did not specify the Host ID and instead put the port number in its place, " +
                            "the specified port number will not be used.\r\n";

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
                        MessageBox.Show("Your license file has too many SERVER lines. Only 1 or 3 are accepted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }
                }

                // DAEMON line checks.
                if (line.TrimStart().StartsWith("DAEMON"))
                {
                    // DAEMON line should come before the product(s).
                    if (productLinesHaveBeenReached)
                    {
                        MessageBox.Show("Your DAEMON line(s) need to come before your products are listed in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    // There should only be one DAEMON line.
                    daemonLineCount++;
                    if (daemonLineCount > 1)
                    {
                        MessageBox.Show("You have too many DAEMON lines in your license file. You only need 1, even if you have 3 SERVER lines.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    // daemonProperty1 and 2 could either be a port number or path to an options file.
                    string[] lineParts = line.Split(' ');

                    // Just having the word "DAEMON" isn't enough.
                    if (lineParts.Length == 1)
                    {
                        MessageBox.Show("You have a DAEMON line, but did not specify the daemon to be used (MLM) nor the path to it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    // Checking for the vendor daemon MLM.
                    string daemonVendor = lineParts[1];
                    bool daemonVendorIsEmptyOrWhiteSpace = string.IsNullOrWhiteSpace(daemonVendor);

                    if (daemonVendorIsEmptyOrWhiteSpace)
                    {
                        MessageBox.Show("There are too many spaces between \"DAEMON\" and \"MLM\".", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    // The vendor daemon needs to MLM. Not mlm or anything else.
                    if (daemonVendor != "MLM")
                    {
                        MessageBox.Show("You have incorrectly specified the vendor daemon MLM.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    // Just specifying "DAEMON MLM" isn't enough.
                    if (lineParts.Length == 2)
                    {
                        MessageBox.Show("You did not specify the path to the vendor daemon MLM.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    if (lineParts.Length == 3)
                    {
                        MessageBox.Show("You did not specify the path to options file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
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
                        MessageBox.Show("You are missing a quotation mark at the end of the full path to MLM.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    if (daemonPath.EndsWith("\""))
                    {
                        MessageBox.Show("You are missing a quotation mark at the beginning of the full path to MLM.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
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
                        MessageBox.Show("You incorrectly specifed the full path to MLM. If there are spaces in the path, " +
                            "please put quotes around the path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
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
                            if (lineParts[daemonLinePartNumber + 1].TrimStart().StartsWith("options=\""))
                            {
                                quotationMarksUsedinOptionsFilePath = true;
                                optionsFileHasBeenSpecified = true;
                                waitingForNextQuotationMark = true;
                                StringBuilder sb = new StringBuilder();
                                sb.Append(lineParts[daemonLinePartNumber + 1]);

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
                            else if (lineParts[daemonLinePartNumber + 1].TrimStart().StartsWith("options="))
                            {
                                daemonProperty1 = lineParts[daemonLinePartNumber + 1];
                                optionsFileHasBeenSpecified = true;
                            }
                            else if (lineParts[daemonLinePartNumber + 1].TrimStart().StartsWith("port="))
                            {
                                daemonProperty1 = lineParts[daemonLinePartNumber + 1];
                                daemonPortNumberHasBeenSpecified = true;
                            }
                            else
                            {
                                MessageBox.Show(generalDaemonOrPortErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                analysisOfServerAndDaemonLinesFailed = true;
                                return;
                            }
                        }

                        // daemonProperty2.
                        if (!quotationMarksUsedinOptionsFilePath)
                        {
                            if (lineParts.Length > daemonLinePartNumber + 2)
                            {
                                if (lineParts[daemonLinePartNumber + 2].TrimStart().StartsWith("options="))
                                {
                                    isConcatenating = false;
                                    waitingForNextQuotationMark = false;

                                    if (optionsFileHasBeenSpecified)
                                    {
                                        MessageBox.Show("You have specified 2 options files in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        analysisOfServerAndDaemonLinesFailed = true;
                                        return;
                                    }
                                    else
                                    {
                                        if (lineParts[daemonLinePartNumber + 2].TrimStart().StartsWith("options=\""))
                                        {
                                            quotationMarksUsedinOptionsFilePath = true;
                                            optionsFileHasBeenSpecified = true;
                                            waitingForNextQuotationMark = true;
                                            StringBuilder sb = new StringBuilder();
                                            sb.Append(lineParts[daemonLinePartNumber + 2]);

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
                                        else
                                        {
                                            daemonProperty2 = lineParts[daemonLinePartNumber + 2];
                                        }
                                    }
                                }
                                else if (lineParts[daemonLinePartNumber + 2].TrimStart().StartsWith("port="))
                                {
                                    if (daemonPortNumberHasBeenSpecified)
                                    {
                                        MessageBox.Show("You have specified 2 port numbers for MLM in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        analysisOfServerAndDaemonLinesFailed = true;
                                        return;
                                    }
                                    else
                                    {
                                        daemonProperty2 = lineParts[daemonLinePartNumber + 2];
                                    }
                                }
                                else
                                {
                                    MessageBox.Show(generalDaemonOrPortErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    analysisOfServerAndDaemonLinesFailed = true;
                                    return;
                                }
                            }
                        }
                        // You have already specified an options file with a quoted path.
                        // This is the first part of code that is broken. Fix it. Use the previous code that gets the last linePart...
                        else
                        {
                            if (lineParts[lineParts.Length - 1].TrimStart().StartsWith("options="))
                            {
                                MessageBox.Show("You have specified 2 options files in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                analysisOfServerAndDaemonLinesFailed = true;
                                return;

                            }
                            if (lineParts[lineParts.Length - 1].TrimStart().StartsWith("port="))
                            {
                                daemonProperty2 = (lineParts[lineParts.Length - 1]);
                            }
                        }
                    }
                    // If quotes aren't used in the daemon path.
                    else
                    {
                        // daemonProperty1.
                        if (lineParts.Length > 3)
                        {
                            if (lineParts[3].TrimStart().StartsWith("options=\""))
                            {
                                quotationMarksUsedinOptionsFilePath = true;
                                optionsFileHasBeenSpecified = true;
                                StringBuilder sb = new StringBuilder();
                                sb.Append(lineParts[3]);

                                quotedOptionsFileLinePartNumber = 4;
                                while (quotedOptionsFileLinePartNumber < lineParts.Length && !lineParts[quotedOptionsFileLinePartNumber].EndsWith("\""))
                                {
                                    sb.Append(" ");
                                    sb.Append(lineParts[quotedOptionsFileLinePartNumber]);
                                    quotedOptionsFileLinePartNumber++;
                                }

                                if (quotedOptionsFileLinePartNumber < lineParts.Length)
                                {
                                    sb.Append(" ");
                                    sb.Append(lineParts[quotedOptionsFileLinePartNumber]);
                                }

                                daemonProperty1 = sb.ToString();
                            }
                            else if (lineParts[3].TrimStart().StartsWith("options="))
                            {
                                optionsFileHasBeenSpecified = true;
                                daemonProperty1 = lineParts[3];
                            }
                            else if (lineParts[3].TrimStart().StartsWith("port="))
                            {
                                daemonPortNumberHasBeenSpecified = true;
                                daemonProperty1 = lineParts[3];
                            }
                            else
                            {
                                MessageBox.Show(generalDaemonOrPortErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                analysisOfServerAndDaemonLinesFailed = true;
                                return;
                            }
                        }
                        // daemonProperty2.
                        if (lineParts.Length > 4)
                        {
                            if (lineParts[4].TrimStart().StartsWith("options="))
                            {
                                if (optionsFileHasBeenSpecified)
                                {
                                    MessageBox.Show("You have specified 2 options files in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    analysisOfServerAndDaemonLinesFailed = true;
                                    return;
                                }
                                else
                                {
                                    daemonProperty2 = lineParts[4];
                                }
                            }
                            else if (lineParts[4].TrimStart().StartsWith("port="))
                            {
                                if (daemonPortNumberHasBeenSpecified)
                                {
                                    MessageBox.Show("You have specified 2 port numbers for MLM in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    analysisOfServerAndDaemonLinesFailed = true;
                                    return;
                                }
                                else
                                {
                                    daemonProperty2 = lineParts[4];
                                }
                            }
                            else
                            {
                                MessageBox.Show(generalDaemonOrPortErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                analysisOfServerAndDaemonLinesFailed = true;
                                return;
                            }
                        }
                    }
                    OutputTextBlock.Text += $"Daemon path: {daemonPath}.\r\ndprop1: {daemonProperty1}. dprop2: {daemonProperty2}.\r\n";
                }

                if (line.TrimStart().StartsWith("INCREMENT"))
                {
                    productLinesHaveBeenReached = true;
                }
            }

            // We have to do this after the loop so that it doesn't get stuck while counting to 3.
            if (serverLineCount == 2)
            {
                MessageBox.Show("Your license file has an invalid number of SERVER lines. Only 1 or 3 are accepted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                analysisOfServerAndDaemonLinesFailed = true;
                return;
            }

            if ((!daemonProperty1.Contains("options=")) && (!daemonProperty2.Contains("options=")))
            {
                MessageBox.Show("Your license file does not specify an options file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                analysisOfServerAndDaemonLinesFailed = true;
                return;
            }
        }
        private void CheckForValidProducts()
        {
            foreach (var optionsIncludeEntry in optionsIncludeIndex)
            {
                int optionsIncludeLineIndex = optionsIncludeEntry.Key;
                Tuple<string, string?, string?, string, string> optionsIncludeData = optionsIncludeEntry.Value;

                string includeProductName = optionsIncludeData.Item1;
                string licenseProductName = "brokenProductNameCheckForValidProducts";

                bool foundMatchingProduct = false;

                foreach (var licenseFileEntry in licenseFileIndex)
                {
                    int lineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                    licenseProductName = licenseFileData.Item1;

                    // Check for a matching includeProductName and productName.
                    if (includeProductName == licenseProductName)
                    {
                        foundMatchingProduct = true;
                        break;
                    }
                }
                if (!foundMatchingProduct)
                {
                    MessageBox.Show($"The following product is not in your license file: \"{includeProductName}\". Product names are case-sensitive " +
                        $"and must match the product name in the license file, which can be found after the word INCREMENT.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    analysisOfOptionsFileProductsFailed = true;
                    return;
                }
            }
        }
    }
}
