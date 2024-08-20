using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Options.File.Checker.XP
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

            // Print the version number.
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            VersionLabel.Text = "v" + version.ToString();

            // For remembering your licenes and options file locations.
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LicenseFileLocationTextBox.Text = Properties.Settings.Default.LicenseFilePathSetting;
            OptionsFileLocationTextBox.Text = Properties.Settings.Default.OptionsFilePathSetting;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.LicenseFilePathSetting = LicenseFileLocationTextBox.Text;
            Properties.Settings.Default.OptionsFilePathSetting = OptionsFileLocationTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void LicenseFileBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select a license file";
                openFileDialog.Filter = "License Files (*.lic;*.dat)|*.lic;*.dat|All Files (*.*)|*.*";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    // Read the file contents.
                    string fileContents = System.IO.File.ReadAllText(filePath);

                    // Check the file size indirectly via its content length.
                    long fileSizeInBytes = fileContents.Length;
                    const long FiftyMegabytes = 50L * 1024L * 1024L;

                    if (fileSizeInBytes > FiftyMegabytes)
                    {
                        MessageBox.Show("There is an issue with the license file: it is over 50 MB and therefore, likely (hopefully) not a license file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (!fileContents.Contains("INCREMENT"))
                    {
                        MessageBox.Show("There is an issue with the license file: it is either not a license file or it is corrupted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (fileContents.Contains("lo=IN") || fileContents.Contains("lo=DC") || fileContents.Contains("lo=CIN"))
                    {
                        MessageBox.Show("There is an issue with the license file: it contains an Individual or Designated Computer license, " +
                            "which cannot use an options file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (fileContents.Contains("CONTRACT_ID="))
                    {
                        MessageBox.Show("There is an issue with the license file: it is not a MathWorks license file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (!fileContents.Contains("SERVER") || !fileContents.Contains("DAEMON"))
                    {
                        MessageBox.Show("There is an issue with the license file: it is missing the SERVER and/or DAEMON line.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    // If all checks pass, set the file path to the text box.
                    LicenseFileLocationTextBox.Text = filePath;
                }
            }
        }

        private void OptionsFileBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select an options file";
                openFileDialog.Filter = "Options Files (*.opt)|*.opt;* |All Files (*.*)|*.*";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    // Read the file contents.
                    string fileContents = System.IO.File.ReadAllText(filePath);

                    // Check the file size indirectly via its content length.
                    long fileSizeInBytes = fileContents.Length;
                    const long FiftyMegabytes = 50L * 1024L * 1024L;

                    if (fileSizeInBytes > FiftyMegabytes)
                    {
                        MessageBox.Show("The selected file is over 50 MB, which is unexpectedly large for an options file. I will assume is this not an options file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(fileContents))
                    {
                        MessageBox.Show("There is an issue with the options file: it is either empty or only contains white space.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (!fileContents.Contains("INCLUDE") && !fileContents.Contains("EXCLUDE") && !fileContents.Contains("GROUP") && !fileContents.Contains("LOG") &&
                        !fileContents.Contains("MAX") && !fileContents.Contains("TIMEOUT") && !fileContents.Contains("RESERVE") && !fileContents.Contains("BORROW") &&
                        !fileContents.Contains("LINGER") && !fileContents.Contains("DEFAULT") && !fileContents.Contains("HIDDEN"))
                    {
                        MessageBox.Show("There is an issue with the options file: it contains no recognized options.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (ContainsNonPlaintext(fileContents))
                    {
                        MessageBox.Show("There is an issue with the options file: it contains non-plaintext characters and therefore, is likely not an options file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    // If all checks pass, set the file path to the text box.
                    OptionsFileLocationTextBox.Text = filePath;
                }
            }
        }

        static bool ContainsNonPlaintext(string fileContents)
        {
            foreach (char c in fileContents)
            {
                // Check if the character is non-printable/control character (excluding newline and tab) or beyond the ASCII range.
                if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t' || c > 127)
                {
                    return true;
                }
            }
            return false;
        }

        private void SaveOutputButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Title = "Save Output";
                    saveFileDialog.DefaultExt = "txt";
                    saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string fileContents = OutputTextBlock.Text;

                        // Save the file contents
                        System.IO.File.WriteAllText(saveFileDialog.FileName, fileContents);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred. Here's the automatic message: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckForUpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/Jestzer/options.file.checker/releases/latest");
            }
            catch (Exception ex)
            {
                // Handle the exception if the browser cannot be opened.
                // For example, show a message box with the error.
                MessageBox.Show("Unable to open the URL: " + ex.Message);
            }
        }

        private void AnalyzerButton_Click(object sender, EventArgs e)
        {
            string licenseFilePath = string.Empty;
            string optionsFilePath = string.Empty;

            if (!string.IsNullOrEmpty(LicenseFileLocationTextBox.Text))
            {
                licenseFilePath = LicenseFileLocationTextBox.Text;
            }
            else
            {
                MessageBox.Show("You did not specify a path to a license file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!string.IsNullOrEmpty(OptionsFileLocationTextBox.Text))
            {
                optionsFilePath = OptionsFileLocationTextBox.Text;
            }
            else
            {
                MessageBox.Show("You did not specify a path to an options file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AnalyzeDataResult analyzeResult = LicenseAndOptionsFileDataAnalyzer.AnalyzeData(licenseFilePath, optionsFilePath);

            // Check if there was an error.
            if (!analyzeResult.Success)
            {
                MessageBox.Show(analyzeResult.ErrorMessage ?? "An unknown error occurred.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Process the returned data.
            StringBuilder output = new StringBuilder();

            if (!LicenseAndOptionsFileDataAnalyzer.serverLineHasPort)
            {
                output.AppendLine("Warning: you did not specify a port number on your SERVER line.\n");
            }

            if (!LicenseAndOptionsFileDataAnalyzer.daemonLineHasPort)
            {
                output.AppendLine("Warning: you did not specify a port number on your DAEMON line. This means a random port will be chosen each time you restart FlexLM.\n");
            }

            if (LicenseAndOptionsFileDataAnalyzer.caseSensitivity)
            {
                output.AppendLine("Warning: case sensitivity is enabled for users defined in GROUPs and HOST_GROUPs.\n");
            }

            // Warn the user if they didn't specify a license number or product key in their seat-subtracting option entries.
            if (LicenseAndOptionsFileDataAnalyzer.unspecifiedLicenseOrProductKey)
            {
                output.AppendLine("Please note: you did not specify a license number or product key for either one of your INCLUDE or RESERVE lines. This means we will subtract the seat from the first " +
                    "license the product appears on.\n");
            }

            if (LicenseAndOptionsFileDataAnalyzer.optionsFileUsesMatlabParallelServer)
            {
                output.AppendLine("Warning: you are including MATLAB Parallel Server in your options file. Keep in mind that the username must correspond to the username as it is on the cluster. " +
            "This does not prevent users from accessing the cluster.\n");
            }

            if (LicenseAndOptionsFileDataAnalyzer.wildcardsAreUsed)
            {
                output.AppendLine("Warning: you are using at least 1 wild card in your options file. These may be unreliable or cause other issues.\n");
            }

            if (!LicenseAndOptionsFileDataAnalyzer.ipAddressesAreUsed)
            {
                output.AppendLine("Warning: you are using an IP address in your options file. IP addresses are often dynamic and therefore cannot be reliably used to identify users.\n");
            }

            // Print seatCount.
            if (analyzeResult.LicenseFileDictionary != null)
            {
                bool overdraftCNWarningHit = false;
                bool includeAllNNUWarningHit = false;
                bool alreadyYelledToCNUAboutPORTFormat = false;

                foreach (var item in analyzeResult.LicenseFileDictionary)
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

                    if (!analyzeResult.DaemonPortIsCNUFriendly && analyzeResult.DaemonLineHasPort && item.Value.Item4.Contains("CNU"))
                    {
                        if (!alreadyYelledToCNUAboutPORTFormat)
                        {
                            output.AppendLine("Please note: your license file contains a CNU license and you've specified a DAEMON port, but you did not specifically specify your DAEMON port with \"PORT=\", which is case-sensitive and recommended to do so.\n");
                            alreadyYelledToCNUAboutPORTFormat = true;
                        }
                    }

                    if (item.Value.Item4.Contains("CNU") && item.Value.Item2 == 9999999)
                    {
                        output.AppendLine(item.Value.Item1 + " has unlimited seats on license " + item.Value.Item5 + "\n");
                    }
                    else if (item.Value.Item4.Contains("CN") && item.Value.Item2 < 0)
                    {
                        if (!overdraftCNWarningHit)
                        {
                            output.AppendLine("\r\nWARNING: you have specified more users on license " + item.Value.Item5 + " for the product " + item.Value.Item1 + " than you have seats for. " +
                                "If every user included was using the product at once then the seat count would technically be at " + item.Value.Item2 + ". " +
                                "This is acceptable since it is a Concurrent license, but if all seats are being used, then a user you've specified to be able to use the product will not be able to " +
                                "access this product until a seat is available.\r\n\r\nTHE WARNING ABOVE WILL ONLY PRINT ONCE FOR THIS SINGLE PRODUCT.\r\n");
                            overdraftCNWarningHit = true;
                        }
                        else
                        {
                            output.AppendLine("You have specified more users on Concurrent license " + item.Value.Item5 + " for the product " + item.Value.Item1 + " than you have seats for (technically counting at " + item.Value.Item2 + " seats.)");
                        }
                    }
                    else
                    {
                        if (item.Value.Item4.Contains("NNU")) // This is not an else if because I want the seat count to still print out the same.
                        {
                            if (!includeAllNNUWarningHit)
                            {
                                if (analyzeResult.IncludeAllDictionary != null)
                                {
                                    if (analyzeResult.IncludeAllDictionary.Count != 0)
                                    {
                                        output.AppendLine("Warning: INCLUDEALL cannot be used NNU licenses and will not count towards their seat count.\n");
                                    }
                                }
                                includeAllNNUWarningHit = true;
                            }
                        }
                        // Finally, print the stuff we want to see!
                        output.AppendLine(item.Value.Item1 + " has " + item.Value.Item2 + " unassigned " + seatOrSeats + " on license number " + item.Value.Item5 + " (product key " + item.Value.Item3 + ").");
                    }
                }
            }
            else
            {
                MessageBox.Show("The license file dictionary is null. Please report this error on GitHub.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Show us the goods, since we didn't hit any critical errors.
            OutputTextBlock.Text = output.ToString();

        }
    }
}
