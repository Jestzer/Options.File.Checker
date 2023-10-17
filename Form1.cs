using static System.Windows.Forms.LinkLabel;
using System.Text.RegularExpressions;

namespace Options.File.Checker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LicenseFileLocationTextBox.Text = Properties.Settings.Default.LicenseFilePathSetting;
            OptionsFileLocationTextBox.Text = Properties.Settings.Default.OptionsFilePathSetting;
        }
        private void LicenseFileBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select a License File";
                openFileDialog.Filter = "License Files (*.dat;*.lic)|*.dat;*.lic|All Files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = openFileDialog.FileName;
                    LicenseFileLocationTextBox.Text = selectedFile;

                    // I probably should put in more checks than this, but I imagine this'll take care of 99.9% of invalid files.
                    bool containsINCREMENT = System.IO.File.ReadAllText(selectedFile).Contains("INCREMENT");

                    if (!containsINCREMENT)
                    {
                        MessageBox.Show("The selected file is either not a license file or is corrupted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    string fileContents = System.IO.File.ReadAllText(selectedFile);
                    if (fileContents.Contains("lo=IN") || fileContents.Contains("lo=DC") || fileContents.Contains("lo=CIN"))
                    {
                        MessageBox.Show("The file you've selected is likely an Individual or Designated Computer license (or has been tampered), which cannot use " +
                            "an options file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    bool containsCONTRACT_ID = System.IO.File.ReadAllText(selectedFile).Contains("CONTRACT_ID=");
                    if (containsCONTRACT_ID)
                    {
                        MessageBox.Show("The selected license file is not a MathWorks license file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    bool missingServer = System.IO.File.ReadAllText(selectedFile).Contains("SERVER");
                    bool missingDaemon = System.IO.File.ReadAllText(selectedFile).Contains("DAEMON");
                    if (!missingDaemon && !missingServer) 
                    { 
                        MessageBox.Show("The selected license file is missing the SERVER and/or DAEMON line.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }
                }
            }
            Properties.Settings.Default.LicenseFilePathSetting = LicenseFileLocationTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void OptionsFileBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select an Options File";
                openFileDialog.Filter = "Options Files (*.opt)|*.opt|All Files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = openFileDialog.FileName;
                    OptionsFileLocationTextBox.Text = selectedFile;

                    string fileContent = System.IO.File.ReadAllText(selectedFile);
                    bool isEmptyOrWhiteSpace = string.IsNullOrWhiteSpace(fileContent);

                    if (isEmptyOrWhiteSpace)
                    {
                        MessageBox.Show("The selected file is either empty or only contains white space.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }
                }
            }
            Properties.Settings.Default.OptionsFilePathSetting = OptionsFileLocationTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void SaveOutputButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save Output";
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    // Save the text to the selected file
                    System.IO.File.WriteAllText(filePath, OutputTextBox.Text);
                }
            }
        }

        private void LicenseFileLocationTextBox_TextChanged(object sender, EventArgs e)
        {
            string licensePath = LicenseFileLocationTextBox.Text;
            bool isLicenseFileValid = System.IO.File.Exists(licensePath);
            bool isOptionsFileValid = System.IO.File.Exists(OptionsFileLocationTextBox.Text);
            AnalyzerButton.Enabled = isLicenseFileValid && isOptionsFileValid;
        }

        private void OptionsFileLocationTextBox_TextChanged(object sender, EventArgs e)
        {
            string optionsPath = OptionsFileLocationTextBox.Text;
            bool isLicenseFileValid = System.IO.File.Exists(LicenseFileLocationTextBox.Text);
            bool isOptionsFileValid = System.IO.File.Exists(optionsPath);
            AnalyzerButton.Enabled = isLicenseFileValid && isOptionsFileValid;
        }
        private void AnalyzerButton_Click(object sender, EventArgs e)
        {
            //check for errors with a try
            //have some hand written errors
            //collect the information from the license file in a way that'll make sense to compare with the options file
            //go through the options file line by line
            //compare them with some functions
            //output the issues/lack there of to the output text box
            //profit???
            //start with some simple comparsion files.
            //use counting and not dlls because you're a shit programmer.

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
                    MessageBox.Show("The selected options file is either empty or only contains white space.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    OptionsFileLocationTextBox.Text = string.Empty;
                    return;
                }

                bool containsINCREMENT = System.IO.File.ReadAllText(LicenseFileLocationTextBox.Text).Contains("INCREMENT");
                if (!containsINCREMENT)
                {
                    MessageBox.Show("The selected file is either not a license file or is corrupted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                if (filteredLicenseFileContents.Contains("lo=IN") || filteredLicenseFileContents.Contains("lo=DC") || filteredLicenseFileContents.Contains("lo=CIN"))
                {
                    MessageBox.Show("The file you've selected is likely an Individual or Designated Computer license (or has been tampered), which cannot use " +
                        "an options file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                bool containsCONTRACT_ID = System.IO.File.ReadAllText(LicenseFileLocationTextBox.Text).Contains("CONTRACT_ID=");
                if (containsCONTRACT_ID)
                {
                    MessageBox.Show("The selected license file is not a MathWorks license file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LicenseFileLocationTextBox.Text = string.Empty;
                    return;
                }

                // Congrats. You passed.
                // Get the things we care about from the license file so that we can go through the options file appropriately.
                OutputTextBox.Text = null;
                string? productName = null;
                int seatCount = 0;
                Dictionary<string, Tuple<int, string, string, string>> productIndex = new Dictionary<string, Tuple<int, string, string, string>>();

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
                                MessageBox.Show("Invalid license file format. Product key is missing for at least 1 product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                                MessageBox.Show("Trial licenses are currently unsupported.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                            MessageBox.Show("Invalid license file format. License Offering is missing for at least 1 product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                            MessageBox.Show("Invalid license file format. License Number is missing from at least 1 product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                                MessageBox.Show("Invalid license file format. License Number is missing from at least 1 product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                OptionsFileLocationTextBox.Text = string.Empty;
                                return;
                            }
                        }

                        //OutputTextBox.Text += $"{productName}: {seatCount} {productKey} {licenseOffering} {licenseNumber}\r\n";
                        productIndex[productName] = Tuple.Create(seatCount, productKey, licenseOffering, licenseNumber);
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

                // Group dictionary.
                Dictionary<string, Tuple<string, string, int>> optionsGroupIndex = new Dictionary<string, Tuple<string, string, int>>();
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

                        //OutputTextBox.Text += $"{groupName}: {groupUsers}. User count: {groupUserCount}\r\n";
                        optionsGroupIndex[groupName] = Tuple.Create(groupName, groupUsers, groupUserCount);
                    }
                }

                // INCLUDE dictionary.
                Dictionary<string, Tuple<string?, string?, string, string>> optionsIncludeIndex = new Dictionary<string, Tuple<string?, string?, string, string>>();
                for (int optionsIncludeLineIndex = 0; optionsIncludeLineIndex < filteredOptionsFileLines.Length; optionsIncludeLineIndex++)
                {
                    string line = filteredOptionsFileLines[optionsIncludeLineIndex];
                    string includeProductName = "brokenProductName";
                    string? includeLicenseNumber = "brokenLicenseNumber";
                    string? includeProductKey = "brokenProductKey";
                    string includeClientType = "brokenClientType";
                    string includeClientSpecified = "brokenClientSpecified";
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
                        else
                        {
                            includeClientType = lineParts[2];
                            includeClientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                            includeLicenseNumber = null;
                            includeProductKey = null;
                        }

                        OutputTextBox.Text += $"{includeProductName}: SN:{includeLicenseNumber}. PK:{includeProductKey} CT:{includeClientType} CS: {includeClientSpecified}\r\n";
                        optionsIncludeIndex[includeProductName] = Tuple.Create(includeLicenseNumber, includeProductKey, includeClientType, includeClientSpecified);
                    }
                }
                OutputTextBox.Text += $"Is case sensitivity turned off?: {caseSensitivity.ToString()}";
                // RESERVE dictionary.

                // MAX dictionary.

                // EXCLUDE dictionary.

            }
            catch (Exception ex)
            { MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            return;
        }
    }
}