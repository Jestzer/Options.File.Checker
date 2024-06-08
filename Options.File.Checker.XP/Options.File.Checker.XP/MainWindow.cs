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
            var updateWindow = new UpdateWindow();
            updateWindow.StartPosition = FormStartPosition.CenterParent;
            updateWindow.ShowDialog(this);
        }

        private void AnalyzerButton_Click(object sender, EventArgs e)
        {

        }     
    }
}
