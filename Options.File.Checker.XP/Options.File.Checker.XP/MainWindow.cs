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

        }

        private void SaveOutputButton_Click(object sender, EventArgs e)
        {

        }

        private void AnalyzerButton_Click(object sender, EventArgs e)
        {

        }
    }
}
