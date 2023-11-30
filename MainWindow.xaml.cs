using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Options.File.Checker.WPF
{
    public partial class MainWindow : Window
    {

        // Debug stuff.
        private bool debug;

        // INCLUDE dictionary from the options file.
        Dictionary<int, Tuple<string, string, string, string, string>> optionsIncludeIndex = new Dictionary<int, Tuple<string, string, string, string, string>>();

        // INCLUDEALL
        Dictionary<int, Tuple<string, string>> optionsIncludeAllIndex = new Dictionary<int, Tuple<string, string>>();

        // RESERVE
        Dictionary<int, Tuple<int, string, string, string, string, string>> optionsReserveIndex = new Dictionary<int, Tuple<int, string, string, string, string, string>>();

        // EXCLUDE
        Dictionary<int, Tuple<string, string, string, string, string>> optionsExcludeIndex = new Dictionary<int, Tuple<string, string, string, string, string>>();

        // GROUP
        Dictionary<int, Tuple<string, string, int>> optionsGroupIndex = new Dictionary<int, Tuple<string, string, int>>();

        // License file dictionary.
        Dictionary<int, Tuple<string, int, string, string, string>> licenseFileIndex = new Dictionary<int, Tuple<string, int, string, string, string>>();

        // End operations if needed.
        bool analysisOfServerAndDaemonLinesFailed = false;
        bool analysisOfOptionsFileProductsFailed = false;

        bool cnuIsUsed = false;

        public MainWindow()
        {
            InitializeComponent();

            // Check if debug mode is enabled.
            debug = ((App)Application.Current).Debug;

            // Load previously opened files.
            LicenseFileLocationTextBox.Text = Properties.Settings.Default.LicenseFilePathSetting;
            OptionsFileLocationTextBox.Text = Properties.Settings.Default.OptionsFilePathSetting;
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

        private void LicenseFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
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
                    //MessageBox.Show("The selected file is either not a license file or is corrupted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ErrorWindow errorWindow = new();
                    errorWindow.ErrorTextBlock.Text = "There is an issue with the selected license file: it is either not a license file or is corrupted.";
                    errorWindow.ShowDialog();
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                string fileContents = System.IO.File.ReadAllText(selectedFile);
                if (fileContents.Contains("lo=IN") || fileContents.Contains("lo=DC") || fileContents.Contains("lo=CIN"))
                {
                    MessageBox.Show("The file you've selected contains an Individual or Designated Computer license. " +
                        "These License Offerings cannot use an options file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (string.IsNullOrWhiteSpace(fileContent))
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

        private void TrialWindowButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseLicenseOfferingWindow chooseLicenseOfferingWindow = new ChooseLicenseOfferingWindow();
            chooseLicenseOfferingWindow.Show();
        }
        private void AnalyzerButton_Click(object sender, RoutedEventArgs e)
        {
            // Prevent previous entries/modified files from skewing data.
            optionsIncludeIndex.Clear();
            optionsIncludeAllIndex.Clear();
            optionsExcludeIndex.Clear();
            optionsReserveIndex.Clear();
            optionsGroupIndex.Clear();
            licenseFileIndex.Clear();
            analysisOfServerAndDaemonLinesFailed = false;
            analysisOfOptionsFileProductsFailed = false;
            OutputTextBlock.Text = string.Empty;
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
                if (string.IsNullOrWhiteSpace(filteredOptionsFileContents))
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
                    MessageBox.Show("The file you've selected likely contains an Individual or Designated Computer license, which cannot use " +
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

                // Look for issues with your SERVER or DAEMON lines.
                AnalyzeServerAndDaemonLine();
                if (analysisOfServerAndDaemonLinesFailed)
                {
                    OutputTextBlock.Text = string.Empty;
                    return;
                }

                // Get the things we care about from the license file so that we can go through the options file appropriately.

                string productName = string.Empty;
                int seatCount = 0;

                for (int lineIndex = 0; lineIndex < filteredLicenseFileLines.Length; lineIndex++)
                {
                    string line = filteredLicenseFileLines[lineIndex];

                    if (line.TrimStart().StartsWith("INCREMENT"))
                    {
                        string[] lineParts = line.Split(' ');
                        productName = lineParts[1];
                        string productExpirationDate = lineParts[4];
                        string productKey = lineParts[6];
                        string licenseOffering = "licenseOfferingIsBroken";
                        string licenseNumber = "licenseNumberisBroken";
                        bool trialLicenseIsUsed = false;
                        int.TryParse(lineParts[5], out seatCount);
                        string rawSeatCount = lineParts[5];

                        // If you're using a PLP license, then we don't care about this product.
                        if (productName == "TMW_Archive")
                        {
                            continue;
                        }

                        // You bastards put the product key (that nobody ever uses but I still have to check just in case) on a different line /sometimes/.
                        if (productKey == "\\")
                        {
                            // Check if there is a next line.
                            if (lineIndex + 1 < filteredLicenseFileLines.Length)
                            {
                                string nextLine = filteredLicenseFileLines[lineIndex + 1];
                                // Parse the next line as needed.
                                string[] nextLineParts = nextLine.Split(' ');
                                string fixedProductKey = nextLineParts[0];
                                fixedProductKey = fixedProductKey.Replace("\t", "");
                                productKey = fixedProductKey;
                                lineIndex++; // Move to the next line since it has been processed. I don't know why this needs to only be in this If statement.
                            }
                            else
                            {
                                MessageBox.Show("Invalid license file format. Product key is missing for at least 1 product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                LicenseFileLocationTextBox.Text = string.Empty;
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
                                // We'll deal with the trial once we get the trial license number.
                                trialLicenseIsUsed = true;
                            }

                            if (licenseOffering.Contains("ei="))
                            {
                                licenseOffering = vendorLineParts[3];
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid license file format. License Offering is missing for at least 1 product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            LicenseFileLocationTextBox.Text = string.Empty;
                            return;
                        }

                        lineIndex++;

                        // Get the license number.
                        // IMPORTANT NOTE: There is a reason the license number is not being recorded as an integer. That's because sometimes it contains letters.
                        // Yes, I understand that means it isn't a number. I don't make these rules.
                        if (lineIndex + 1 < filteredLicenseFileLines.Length)
                        {
                            string licenseNumberLine = filteredLicenseFileLines[lineIndex + 1];
                            string[] licenseNumberLineParts = licenseNumberLine.Split(" ");
                            string unprocessedLicenseNumber = licenseNumberLineParts[1];
                            licenseNumber = Regex.Replace(unprocessedLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                            if (licenseNumber.Contains("USER_BASED="))
                            {
                                unprocessedLicenseNumber = licenseNumberLineParts[3];

                                // Make sure you don't have a line break ruining our day.
                                if (unprocessedLicenseNumber == "\\")
                                {
                                    string currentLine = filteredLicenseFileLines[lineIndex + 2];
                                    string pattern = @"asset_info=(\S+)";

                                    Regex regex = new Regex(pattern);
                                    Match match = regex.Match(currentLine);

                                    if (match.Success)
                                    {
                                        licenseNumber = match.Groups[1].Value;
                                    }
                                }
                                else
                                {
                                    licenseNumber = Regex.Replace(unprocessedLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid license file format. License Number is missing from at least 1 product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            LicenseFileLocationTextBox.Text = string.Empty;
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

                                // Trials really seem to make getting license numbers even worse.
                                if (nextLine.Contains("SN=DEMO"))
                                {
                                    string currentLine = filteredLicenseFileLines[lineIndex];
                                    string pattern = @"asset_info=(\S+)";

                                    Regex regex = new Regex(pattern);
                                    Match match = regex.Match(currentLine);

                                    if (match.Success)
                                    {
                                        licenseNumber = match.Groups[1].Value;
                                    }
                                    else
                                    {
                                        currentLine = filteredLicenseFileLines[lineIndex - 1];
                                        pattern = @"ei=([^:]+)";

                                        regex = new Regex(pattern);
                                        match = regex.Match(currentLine);

                                        if (match.Success)
                                        {
                                            licenseNumber = match.Groups[1].Value;
                                        }
                                        else
                                        {
                                            OutputTextBlock.Text = string.Empty;
                                            MessageBox.Show($"Your license number could not be correctly processed. It is be read as {licenseNumber} for the product {productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                            return;
                                        }

                                    }
                                }
                                // If you're not using a trial.
                                else
                                {
                                    // Ensure "SN=" and 8 characters are present. Otherwise... look elsewhere for the license number.
                                    if (startIndex != -1 && startIndex + 11 <= nextLine.Length)
                                    {
                                        string unprocessedLicenseNumber = nextLine.Substring(startIndex + 3, 8); // I imagine someday that 8 will need to be increased.
                                        licenseNumber = Regex.Replace(unprocessedLicenseNumber, "[^0-9]", "");
                                    }
                                    else
                                    {
                                        string currentLine = filteredLicenseFileLines[lineIndex];
                                        string pattern = @"asset_info=(\S+)";

                                        Regex regex = new Regex(pattern);
                                        Match match = regex.Match(currentLine);

                                        if (match.Success)
                                        {
                                            licenseNumber = match.Groups[1].Value;
                                        }
                                        else
                                        {
                                            currentLine = filteredLicenseFileLines[lineIndex + 2];
                                            pattern = @"SN=(\S+)";

                                            regex = new Regex(pattern);
                                            match = regex.Match(currentLine);

                                            if (match.Success)
                                            {
                                                licenseNumber = match.Groups[1].Value;
                                            }
                                            else
                                            {
                                                OutputTextBlock.Text = string.Empty;
                                                MessageBox.Show($"Your license number could not be correctly processed. It is be read as {licenseNumber} for the product {productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Invalid license file format. License Number is missing from at least 1 product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                LicenseFileLocationTextBox.Text = string.Empty;
                                return;
                            }
                        }
                        // Really would kill us to put this license number in one place, wouldn't it?
                        else if (licenseNumber.Contains("DUP_GROUP="))
                        {
                            string currentLine = filteredLicenseFileLines[lineIndex + 1];
                            string pattern = @"asset_info=(\S+)";

                            Regex regex = new Regex(pattern);
                            Match match = regex.Match(currentLine);

                            if (match.Success)
                            {
                                licenseNumber = match.Groups[1].Value;
                            }
                            else
                            {
                                OutputTextBlock.Text = string.Empty;
                                MessageBox.Show($"Your license number could not be correctly processed. It is be read as {licenseNumber} for the product {productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        else if (licenseNumber == "\\")
                        {
                            string currentLine = filteredLicenseFileLines[lineIndex + 2];
                            string pattern = @"SN=(\S+)";

                            Regex regex = new Regex(pattern);
                            Match match = regex.Match(currentLine);

                            if (match.Success)
                            {
                                licenseNumber = match.Groups[1].Value;
                            }
                            else
                            {
                                OutputTextBlock.Text = string.Empty;
                                MessageBox.Show($"Your license number could not be correctly processed. It is be read as {licenseNumber} for the product {productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        // Check the product's expiration date.
                        // Convert/parse the productExpirationDate string to a DateTime object.

                        if (productExpirationDate == "01-jan-0000")
                        {
                            productExpirationDate = "01-jan-2999";
                        }

                        DateTime expirationDate = DateTime.ParseExact(productExpirationDate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);

                        // Get the current system date.
                        DateTime currentDate = DateTime.Now.Date;

                        if (expirationDate < currentDate)
                        {
                            OutputTextBlock.Text = string.Empty;
                            MessageBox.Show($"The product {productName} on license number {licenseNumber} expired on {productExpirationDate}. " +
                                "Please update your license file appropriately before proceeding.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Figure out what your license offering is, if you're using a trial.
                        if (trialLicenseIsUsed)
                        {
                            bool matchingTrialFound = false;
                            for (int i = 1; i <= 5; i++)
                            {
                                string? trialNumberSpecified = Properties.Settings.Default[$"TrialNumber{i}"].ToString();
                                if (licenseNumber == trialNumberSpecified)
                                {
                                    matchingTrialFound = true;

                                    string? trialLicenseOfferingSpecified = Properties.Settings.Default[$"TrialLicenseOffering{i}"].ToString();
                                    licenseOffering = trialLicenseOfferingSpecified ?? "licenseOfferingIsBroken";
                                    break;
                                }
                            }
                            if (!matchingTrialFound)
                            {
                                OutputTextBlock.Text = string.Empty;
                                MessageBox.Show($"Your license file has at least 1 trial license, {licenseNumber}, but none of the trial numbers you specified in this program's settings match the trial " +
                                    "license number(s) found in the license. Please enter any trial license numbers you're using on the startup screen.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        if (licenseOffering.Contains("NNU"))
                        {
                            seatCount /= 2;
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
                            OutputTextBlock.Text = string.Empty;
                            MessageBox.Show($"Your license file contains a Designated Computer license ({licenseNumber}) that is incorrectly labeled as a Concurrent license.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (!licenseOffering.Contains("CNU") && rawSeatCount == "uncounted")
                        {
                            OutputTextBlock.Text = string.Empty;
                            MessageBox.Show("The file you've selected likely contains an Individual or Designated Computer license, which cannot use " +
                                $"an options file. The license number is question is {licenseNumber}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (seatCount < 1)
                        {
                            OutputTextBlock.Text = string.Empty;
                            MessageBox.Show($"{productName} on license {licenseNumber} is reading with a seat count of zero from just the license file. " +
                                $"If you're using a trial license, make sure you selecting the correct License Offering. " +
                                $"Otherwise, your license file is corrupt.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Before proceeding, make sure the values we've collected are valid.
                        if (string.IsNullOrWhiteSpace(productName))
                        {
                            OutputTextBlock.Text = string.Empty;
                            MessageBox.Show("A product name could not be detected correctly in your license file. It is being detected as blank.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (licenseNumber.Contains("broken") || string.IsNullOrWhiteSpace(licenseNumber) || Regex.IsMatch(licenseNumber, @"^[^Rab_\d]+$"))
                        {
                            OutputTextBlock.Text = string.Empty;
                            MessageBox.Show($"An invalid license number was detected in your license file for {productName}. " +
                                $"The invalid license number detected was \"{licenseNumber}\".", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (licenseOffering.Contains("broken") || string.IsNullOrWhiteSpace(licenseOffering))
                        {
                            OutputTextBlock.Text = string.Empty;
                            MessageBox.Show($"A license offering could not be detected in your license file for {productName} " +
                                $"on license number {licenseNumber}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(productKey))
                        {
                            OutputTextBlock.Text = string.Empty;
                            MessageBox.Show($"A product key could not be detected in your license file for {productName} " +
                                $"on license number {licenseNumber}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (debug)
                        {
                            OutputTextBlock.Text += $"INCREMENT {productName}: {seatCount} {productKey} {licenseOffering} {licenseNumber}\r\n";
                        }
                        licenseFileIndex[lineIndex] = Tuple.Create(productName, seatCount, productKey, licenseOffering, licenseNumber);
                    }
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

                        if (debug)
                        {
                            OutputTextBlock.Text += $"{optionsGroupLineIndex}, {groupName}: {groupUsers}. User count: {groupUserCount}\r\n";
                        }
                        optionsGroupIndex[optionsGroupLineIndex] = Tuple.Create(groupName, groupUsers, groupUserCount);
                    }
                }

                // INCLUDE dictionary.
                for (int optionsIncludeLineIndex = 0; optionsIncludeLineIndex < filteredOptionsFileLines.Length; optionsIncludeLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsIncludeLineIndex];
                    string includeProductName = "brokenIncludeProductName";
                    string includeLicenseNumber = "brokenIncludeLicenseNumber";
                    string includeProductKey = "brokenIncludeProductKey";
                    string includeClientType = "brokenIncludeClientType";
                    string includeClientSpecified = "brokenIncludeClientSpecified";

                    // Put in code that deals with this line: "MATLAB:asset_info=112233" GROUP 112233_MATLAB
                    if (line.TrimStart().StartsWith("INCLUDE "))
                    {
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        includeProductName = lineParts[1];
                        if (includeProductName.Contains('"'))
                        {
                            includeProductName = includeProductName.Replace("\"", "");
                            includeLicenseNumber = lineParts[2];
                            if (!includeProductName.Contains(':'))
                            {
                                if (includeLicenseNumber.Contains("key="))
                                {
                                    includeProductKey = lineParts[2];
                                    string unfixedIncludeProductKey = includeProductKey;
                                    string quotedIncludeProductKey = unfixedIncludeProductKey.Replace("key=", "");
                                    includeProductKey = quotedIncludeProductKey.Replace("\"", "");
                                    includeLicenseNumber = string.Empty;
                                }
                                // asset_info=
                                else
                                {
                                    string unfixedIncludeLicenseNumber = includeLicenseNumber;
                                    string quoteIncludeLicenseNumber = Regex.Replace(unfixedIncludeLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                    includeLicenseNumber = quoteIncludeLicenseNumber.Replace("\"", "");
                                    includeProductKey = string.Empty;
                                }

                                includeClientType = lineParts[3];
                                includeClientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                            }
                            // If you have " and :
                            else
                            {
                                string[] colonParts = includeProductName.Split(":");
                                if (colonParts.Length != 2)
                                {
                                    MessageBox.Show($"One of your INCLUDE lines has a stray colon: {includeProductName}...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    OutputTextBlock.Text = string.Empty;
                                    return;
                                }
                                includeProductName = colonParts[0];
                                if (colonParts[1].Contains("key="))
                                {
                                    string unfixedIncludeProductKey = colonParts[1];
                                    includeProductKey = unfixedIncludeProductKey.Replace("key=", "");
                                    includeLicenseNumber = string.Empty;
                                }
                                else
                                {
                                    string unfixedIncludeLicenseNumber = colonParts[1];
                                    includeLicenseNumber = Regex.Replace(unfixedIncludeLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                    includeProductKey = string.Empty;
                                }
                                includeClientType = lineParts[2];
                                includeClientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                            }
                        }
                        // In case you decided to use a : instead of ""...
                        else if (includeProductName.Contains(':'))
                        {
                            string[] colonParts = includeProductName.Split(":");
                            if (colonParts.Length != 2)
                            {
                                MessageBox.Show($"One of your INCLUDE lines has a stray colon: {includeProductName}...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                OutputTextBlock.Text = string.Empty;
                                return;
                            }
                            includeProductName = colonParts[0];
                            if (colonParts[1].Contains("key="))
                            {
                                string unfixedIncludeProductKey = colonParts[1];
                                includeProductKey = unfixedIncludeProductKey.Replace("key=", "");
                                includeLicenseNumber = string.Empty;
                            }
                            else
                            {
                                string unfixedIncludeLicenseNumber = colonParts[1];
                                includeLicenseNumber = Regex.Replace(unfixedIncludeLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                includeProductKey = string.Empty;
                            }
                            includeClientType = lineParts[2];
                            includeClientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                        }
                        else
                        {
                            includeClientType = lineParts[2];
                            includeClientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                            includeLicenseNumber = string.Empty;
                            includeProductKey = string.Empty;
                        }
                        if (debug)
                        {
                            OutputTextBlock.Text += $"{includeProductName}: SN:{includeLicenseNumber}. PK:{includeProductKey} CT:{includeClientType} CS: {includeClientSpecified}\r\n";
                        }
                        optionsIncludeIndex[optionsIncludeLineIndex] = Tuple.Create(includeProductName, includeLicenseNumber, includeProductKey, includeClientType, includeClientSpecified);
                    }
                }
                OutputTextBlock.Text += $"Is case sensitivity turned off?: {caseSensitivity}\r\n";

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

                        reserveSeatsString = lineParts[1];
                        reserveProductName = lineParts[2];

                        // Convert the seat count from a string to a integer.
                        if (int.TryParse(reserveSeatsString, out reserveSeatCount))
                        {
                            // Parsing was successful.
                        }
                        else
                        {
                            MessageBox.Show($"You have an incorrectly specified the seat count for one of your RESERVE lines.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            OutputTextBlock.Text = string.Empty;
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
                                    MessageBox.Show($"One of your RESERVE lines has a stray colon: {reserveProductName}...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    OutputTextBlock.Text = string.Empty;
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
                                MessageBox.Show($"One of your RESERVE lines has a stray colon: {reserveProductName}...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                OutputTextBlock.Text = string.Empty;
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

                        if (debug)
                        {
                            OutputTextBlock.Text += $"Max seats: {maxSeats}. Product: {maxProductName}. CT: {maxClientType}. CS: {maxClientSpecified}\r\n";
                        }
                        optionsMaxIndex[maxProductName] = Tuple.Create(maxSeats, maxClientType, maxClientSpecified);
                    }
                }

                // EXCLUDE dictionary.
                for (int optionsExcludeLineIndex = 0; optionsExcludeLineIndex < filteredOptionsFileLines.Length; optionsExcludeLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsExcludeLineIndex];
                    string excludeProductName = "brokenProductName";
                    string excludeLicenseNumber = "brokenLicenseNumber";
                    string excludeProductKey = "brokenProductKey";
                    string excludeClientType = "brokenClientType";
                    string excludeClientSpecified = "brokenClientSpecified";
                    if (line.TrimStart().StartsWith("EXCLUDE "))
                    {
                        excludeLinesAreUsed = true;
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        excludeProductName = lineParts[1];
                        if (excludeProductName.Contains('"'))
                        {
                            excludeProductName = excludeProductName.Replace("\"", "");
                            excludeLicenseNumber = lineParts[2];
                            if (!excludeProductName.Contains(':'))
                            {
                                if (excludeLicenseNumber.Contains("key="))
                                {
                                    excludeProductKey = lineParts[2];
                                    string unfixedExcludeProductKey = excludeProductKey;
                                    string quotedExcludeProductKey = unfixedExcludeProductKey.Replace("key=", "");
                                    excludeProductKey = quotedExcludeProductKey.Replace("\"", "");
                                    excludeLicenseNumber = string.Empty;
                                }
                                // asset_info=
                                else
                                {
                                    string unfixedExcludeLicenseNumber = excludeLicenseNumber;
                                    string quoteExcludeLicenseNumber = Regex.Replace(unfixedExcludeLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                    excludeLicenseNumber = quoteExcludeLicenseNumber.Replace("\"", "");
                                    excludeProductKey = string.Empty;
                                }

                                excludeClientType = lineParts[3];
                                excludeClientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                            }
                            // If you have " and :
                            else
                            {
                                string[] colonParts = excludeProductName.Split(":");
                                if (colonParts.Length != 2)
                                {
                                    MessageBox.Show($"One of your EXCLUDE lines has a stray colon: {excludeProductName}...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    OutputTextBlock.Text = string.Empty;
                                    return;
                                }
                                excludeProductName = colonParts[0];
                                if (colonParts[1].Contains("key="))
                                {
                                    string unfixedExcludeProductKey = colonParts[1];
                                    excludeProductKey = unfixedExcludeProductKey.Replace("key=", "");
                                    excludeLicenseNumber = string.Empty;
                                }
                                else
                                {
                                    string unfixedExcludeLicenseNumber = colonParts[1];
                                    excludeLicenseNumber = Regex.Replace(unfixedExcludeLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                    excludeProductKey = string.Empty;
                                }
                                excludeClientType = lineParts[2];
                                excludeClientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                            }
                        }
                        // In case you decided to use a : instead of ""...
                        else if (excludeProductName.Contains(':'))
                        {
                            string[] colonParts = excludeProductName.Split(":");
                            if (colonParts.Length != 2)
                            {
                                MessageBox.Show($"One of your EXCLUDE lines has a stray colon: {excludeProductName}...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                OutputTextBlock.Text = string.Empty;
                                return;
                            }
                            excludeProductName = colonParts[0];
                            if (colonParts[1].Contains("key="))
                            {
                                string unfixedExcludeProductKey = colonParts[1];
                                excludeProductKey = unfixedExcludeProductKey.Replace("key=", "");
                                excludeLicenseNumber = string.Empty;
                            }
                            else
                            {
                                string unfixedExcludeLicenseNumber = colonParts[1];
                                excludeLicenseNumber = Regex.Replace(unfixedExcludeLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                excludeProductKey = string.Empty;
                            }
                            excludeClientType = lineParts[2];
                            excludeClientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                        }
                        else
                        {
                            excludeClientType = lineParts[2];
                            excludeClientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                            excludeLicenseNumber = string.Empty;
                            excludeProductKey = string.Empty;
                        }

                        if (debug)
                        {
                            OutputTextBlock.Text += $"{excludeProductName}: SN:{excludeLicenseNumber}. PK:{excludeProductKey} CT:{excludeClientType} CS: {excludeClientSpecified}\r\n";
                        }
                        optionsExcludeIndex[optionsExcludeLineIndex] = Tuple.Create(excludeProductName, excludeLicenseNumber, excludeProductKey, excludeClientType, excludeClientSpecified);
                    }
                }

                // HOST_GROUP dictionary.
                Dictionary<int, Tuple<string, string>> optionsHostGroupIndex = new Dictionary<int, Tuple<string, string>>();
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

                    if (line.TrimStart().StartsWith("EXCLUDEALL "))
                    {
                        excludeAllLinesAreUsed = true;
                        return;
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
                        includeAllClientType = lineParts[1];
                        includeAllClientSpecified = string.Join(" ", lineParts.Skip(2));

                        if (debug)
                        {
                            OutputTextBlock.Text += $"INCLUDEALL: {includeAllClientType}. Clients: {includeAllClientSpecified}\r\n";
                        }
                        optionsIncludeAllIndex[optionsIncludeAllLineIndex] = Tuple.Create(includeAllClientType, includeAllClientSpecified);
                    }
                }


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
                    OutputTextBlock.Text += "\r\nYou are using at least 1 HOST_GROUP line in your options file.\r\n";
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
                                    MessageBox.Show($"You have specified a USER to be able to use {productName} for license {licenseNumber}, but you did not define the USER.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                        MessageBox.Show($"You have specified too many users to be able to use {productName} for license {licenseNumber}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                    MessageBox.Show($"You have specified a GROUP to be able to use {productName} for license {licenseNumber}, but you did not specify which GROUP.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                                MessageBox.Show($"You have specified too many users to be able to use {productName} for license {licenseNumber}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                    MessageBox.Show($"You have specified a USER to be able to use {productName}, but you did not define the USER.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                        MessageBox.Show($"You have specified too many users to be able to use {productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                    MessageBox.Show($"You have specified a GROUP to be able to use {productName}, but you did not specify which GROUP.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                                MessageBox.Show($"You have specified too many users to be able to use {productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                OutputTextBlock.Text = string.Empty;
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
                    MessageBox.Show($"You have specified too many users to be able to use {includeProductName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                MessageBox.Show($"You have specified too many users to be able to use {productName}. Don't forget about your INCLUDEALL line(s).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show($"You have specified to use a GROUP on an INCLUDEALL line, but you did not specify which GROUP.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                        MessageBox.Show($"You have specified too many users to be able to use {productName}. Don't forget that you are using at least 1 " +
                                            "INCLUDEALL line.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    // There is no math that can be done because you've specified an client type that can be shared between any number of users.
                }
                else
                {
                    OutputTextBlock.Text = string.Empty;
                    MessageBox.Show($"You specified an invalid client type for an INCLUDEALL line.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                    MessageBox.Show($"You have specified a USER to be able to use {productName} for license {licenseNumber}, but you did not define the USER.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                        MessageBox.Show($"You have specified too many users to be able to use {productName} for license {licenseNumber}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                            if (reserveClientType == "GROUP")
                            {

                                // Check that a group has actually been specified.
                                if (string.IsNullOrWhiteSpace(reserveClientSpecified))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    MessageBox.Show($"You have specified a GROUP to be able to use {productName} for license {licenseNumber}, but you did not specify which GROUP.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                                MessageBox.Show($"You have specified too many users to be able to use {productName} for license {licenseNumber}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                OutputTextBlock.Text = string.Empty;
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
                        }
                        else
                        {
                            if (debug)
                            {
                                // We'll get to the uncategorized products later.
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
                                    MessageBox.Show($"You have specified a USER to be able to use {productName}, but you did not define the USER.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                        MessageBox.Show($"You have specified too many users to be able to use {productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                            if (reserveClientType == "GROUP")
                            {

                                // Check that a group has actually been specified.
                                if (string.IsNullOrWhiteSpace(reserveClientSpecified))
                                {
                                    OutputTextBlock.Text = string.Empty;
                                    MessageBox.Show($"You have specified a GROUP to be able to use {productName}, but you did not specify which GROUP.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }

                                foreach (var optionsGroupEntry in optionsGroupIndex)
                                {
                                    // Load GROUP specifications.
                                    int optionsGroupLineIndex = optionsGroupEntry.Key;
                                    Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;

                                    string groupName = optionsGroupData.Item1;
                                    string groupUsers = optionsGroupData.Item2;

                                    if (groupName == reserveClientSpecified)
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
                                                OutputTextBlock.Text = string.Empty;
                                                MessageBox.Show($"You have specified too many users to be able to use {productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show($"You have specified too many users to be able to use {reserveProductName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Putting this here rather than with the other private voids because it means I don't have to add failing variables in the code above.
            PrintUsefulInformation();
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
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("Your SERVER line(s) need to come before your products are listed in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
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
                            "the specified port number will not be used. The SERVER port number will be randomly choosen each time you restart it.\r\n";

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
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("Your DAEMON line(s) need to come before your products are listed in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    // There should only be one DAEMON line.
                    daemonLineCount++;
                    if (daemonLineCount > 1)
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("You have too many DAEMON lines in your license file. You only need 1, even if you have 3 SERVER lines.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    // port= and options= should only appear once.
                    int countPortEquals = Regex.Matches(line, "port=", RegexOptions.IgnoreCase).Count;
                    int countOptionsEquals = Regex.Matches(line, "options=", RegexOptions.IgnoreCase).Count;
                    int countCommentedBeginLines = Regex.Matches(line, "# BEGIN--------------", RegexOptions.IgnoreCase).Count;

                    if (countCommentedBeginLines > 0)
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("You have content that is intended to be commented out in your DAEMON line. Please remove it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    if (countPortEquals > 1)
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("You have specified more than 1 port number for MLM.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    if (countOptionsEquals > 1)
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("You have specified the path to more than 1 options file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    if (countOptionsEquals == 0)
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("You did not specify the path to the options file. If you included the path, but did not use options= to specify it, " +
                            "MathWorks licenses ask that you do so, even if they technically work without options=.\r\n\r\n" +
                            "If you used a \\ to indicate a line break on your DAEMON line, this program currently does not support this.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    // daemonProperty1 and 2 could either be a port number or path to an options file.
                    string[] lineParts = line.Split(' ');

                    // Just having the word "DAEMON" isn't enough.
                    if (lineParts.Length == 1)
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("You have a DAEMON line, but did not specify the daemon to be used (MLM) nor the path to it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    // Checking for the vendor daemon MLM.
                    string daemonVendor = lineParts[1];

                    if (string.IsNullOrWhiteSpace(daemonVendor))
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("There are too many spaces between \"DAEMON\" and \"MLM\".", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    // The vendor daemon needs to MLM. Not mlm or anything else.
                    if (daemonVendor != "MLM")
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("You have incorrectly specified the vendor daemon MLM.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    // Just specifying "DAEMON MLM" isn't enough.
                    if (lineParts.Length == 2)
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("You did not specify the path to the vendor daemon MLM.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    if (lineParts.Length == 3)
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("You did not specify the path to the options file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show("You are missing a quotation mark at the end of the full path to MLM.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfServerAndDaemonLinesFailed = true;
                        return;
                    }

                    if (daemonPath.EndsWith("\""))
                    {
                        OutputTextBlock.Text = string.Empty;
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
                        OutputTextBlock.Text = string.Empty;
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
                                if (lineParts[daemonLinePartNumber + 2].TrimStart().StartsWith("options=", StringComparison.OrdinalIgnoreCase))
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
                                    OutputTextBlock.Text = string.Empty;
                                    MessageBox.Show(generalDaemonOrPortErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    analysisOfServerAndDaemonLinesFailed = true;
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
                                    MessageBox.Show("You have specified 2 options files in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    analysisOfServerAndDaemonLinesFailed = true;
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
                                MessageBox.Show(generalDaemonOrPortErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                analysisOfServerAndDaemonLinesFailed = true;
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
                                        MessageBox.Show("You have specified 2 options files in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        analysisOfServerAndDaemonLinesFailed = true;
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
                                    OutputTextBlock.Text = string.Empty;
                                    MessageBox.Show(generalDaemonOrPortErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    analysisOfServerAndDaemonLinesFailed = true;
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
                                    MessageBox.Show("You have specified 2 options files in your license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    analysisOfServerAndDaemonLinesFailed = true;
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
                    string daemonProperty2Edited = Regex.Replace(daemonProperty2, "port=", "", RegexOptions.IgnoreCase);
                    daemonProperty2Edited = Regex.Replace(daemonProperty2Edited, "options=", "", RegexOptions.IgnoreCase);

                    if (daemonProperty1.Contains("options"))
                    {
                        if (daemonProperty2.Contains("port"))
                        {
                            OutputTextBlock.Text += $"\r\nDAEMON path: {daemonPath}.\r\nOptions path: {daemonProperty1Edited}. DAEMON port: {daemonProperty2Edited}.\r\n";
                        }
                        else
                        {
                            OutputTextBlock.Text += $"\r\nDAEMON path: {daemonPath}.\r\nOptions path: {daemonProperty1Edited}.\r\n";
                        }
                    }
                    else
                    {
                        OutputTextBlock.Text += $"\r\nDAEMON path: {daemonPath}.\r\nDAEMON port: {daemonProperty1Edited}. Options path: {daemonProperty2Edited}.\r\n";
                    }
                }

                if (line.TrimStart().StartsWith("USE_SERVER", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("You have USE_SERVER in your license file. This should be in the end user's network.lic file, " +
                        "not the Network License Manager's license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    analysisOfServerAndDaemonLinesFailed = true;
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
                MessageBox.Show("Your license file has an invalid number of SERVER lines. Only 1 or 3 are accepted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                analysisOfServerAndDaemonLinesFailed = true;
                return;
            }

            // These have to happen outside the loop to get a proper tally.
            if (daemonLineCount == 0)
            {
                OutputTextBlock.Text = string.Empty;
                MessageBox.Show("You have no DAEMON line in your license file. You should not be using VENDOR lines for MathWorks licenses, " +
                    "if you are.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                analysisOfServerAndDaemonLinesFailed = true;
                return;
            }

            if ((!daemonProperty1.Contains("options=", StringComparison.OrdinalIgnoreCase)) && (!daemonProperty2.Contains("options=", StringComparison.OrdinalIgnoreCase)))
            {
                OutputTextBlock.Text = string.Empty;
                MessageBox.Show("Your license file does not specify an options file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                analysisOfServerAndDaemonLinesFailed = true;
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
                    MessageBox.Show($"The following product is not in your license file: \"{includeProductName}\". Product names are case-sensitive " +
                        $"and must match the product name in the license file, which can be found after the word INCREMENT.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                            MessageBox.Show($"You have an INCLUDE line that specifies a product key, {includeProductKey}, " +
                                $"but the product on your INCLUDE line, {includeProductName}, does not match the product it is tied to in the license file, " +
                                $"{productName}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show($"You have an INCLUDE line that specifies a license number, {includeLicenseNumber}, " +
                            $"that does not exist in the specified license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfOptionsFileProductsFailed = true;
                        return;
                    }
                    else
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show($"You have an INCLUDE line that specifies a product key, {includeProductKey}, " +
                            $"that does not exist in the specified license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show($"You have an EXCLUDE line that specifies a license number, {excludeLicenseNumber}, " +
                            $"that does not exist in the specified license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfOptionsFileProductsFailed = true;
                        return;
                    }
                    else
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show($"You have an EXCLUDE line that specifies a product key, {excludeProductKey}, " +
                            $"that does not exist in the specified license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show($"You have a RESERVE line that specifies a license number, {reserveLicenseNumber}, " +
                            $"that does not exist in the specified license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfOptionsFileProductsFailed = true;
                        return;
                    }
                    else
                    {
                        OutputTextBlock.Text = string.Empty;
                        MessageBox.Show($"You have a RESERVE line that specifies a product key, {reserveProductKey}, " +
                            $"that does not exist in the specified license file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        analysisOfOptionsFileProductsFailed = true;
                        return;
                    }
                }
            }
        }
        private void PrintUsefulInformation()
        {
            OutputTextBlock.Text += "\r\nPlease note: if you did not specify a license for an INCLUDE or RESERVE line, it will subtract the seat from the first " +
                "license the product appears on. Product names are listed with their FlexLM names.\r\n";

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
                    if (overdraftCNWarningHit && licenseOffering == "lo=CN")
                    {
                        // No need to repeat information we're about to get.
                    }
                    else
                    {
                        OutputTextBlock.Text += $"The product {productName} has {seatCount} seats remaining on license {licenseNumber}.\r\n";
                    }
                }
              
                if (licenseOffering == "lo=CN" && seatCount < 0)
                {
                    if (!overdraftCNWarningHit)
                    {
                        OutputTextBlock.Text += $"\r\nWARNING: You have specified more users on license {licenseNumber} for the product {productName} than you have seats for (counting at {seatCount}.)  " +
                            $"This is acceptable since it is a Concurrent license, but if all seats are being used, then a user you've specified to be able to use the product will not be able to " +
                            $"access this product until a seat is available.\r\n\r\nTHE WARNING ABOVE WILL ONLY PRINT ONCE.\r\n";
                        overdraftCNWarningHit = true;
                    }
                    else
                    {
                        OutputTextBlock.Text += $"\r\nYou have specified more users on license {licenseNumber} for the product {productName} than you have seats for (counting at {seatCount}.)\r\n";
                    }
                }
            }
        }
    }
}
