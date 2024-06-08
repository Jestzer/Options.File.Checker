using Newtonsoft;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Options.File.Checker.XP
{
    public partial class UpdateWindow : Form
    {
        public UpdateWindow()
        {
            InitializeComponent();
            CheckForUpdate();
        }

        public static string GetAssemblyVersion()
        {
            // Get the current assembly.
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Get the version number from the assembly.
            Version version = assembly.GetName().Version;

            return version.ToString();
        }

        private void CheckForUpdate()
        {
            string currentVersionString = GetAssemblyVersion();
            Version currentVersion = new Version(currentVersionString);

            // GitHub API URL for the latest release.
            string latestReleaseUrl = "https://api.github.com/repos/Jestzer/options.file.checker/releases/latest";

            using (WebClient client = new WebClient())
            {
                // GitHub API requires a user-agent.
                client.Headers["User-Agent"] = "options.file.checker/" + currentVersion;

                try
                {
                    // Make the latest release a JSON string.
                    string jsonString = client.DownloadString(latestReleaseUrl);

                    // Parse the JSON to get the tag_name (version number).
                    var latestVersionObj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);
                    string latestVersionString = (string)latestVersionObj["tag_name"];

                    // Remove 'v' prefix if present in the tag name.
                    latestVersionString = latestVersionString.TrimStart('v');

                    // Parse the version string.
                    Version latestVersion = new Version(latestVersionString);

                    // Compare the current version with the latest version.
                    if (currentVersion.CompareTo(latestVersion) < 0)
                    {
                        // A newer version is available!
                        UpdateTextBlock.Text = "A new update is available";
                    }
                    else
                    {
                        // The current version is up-to-date.
                        UpdateTextBlock.Text = "You are using the latest release available.";
                    }
                }
                catch (WebException ex)
                {
                    string message = ex.ToString();
                    DownloadButton.Text = "OK";

                    if (message.Contains("System.Net.WebException: The remote name could not be resolved: 'api.github.com'"))
                    {
                        UpdateTextBlock.Text = "api.github.com could not be reached. You either don't have an internet connection, a lousy firewall, " +
                        "lousy antivirus software, or you screwed around with your computer/network badly. Go fix it.";
                    }
                    else
                    {
                        UpdateTextBlock.Text = message;
                    }                    
                }
                catch (Exception ex)
                {
                    string message = ex.ToString();
                    UpdateTextBlock.Text = message;
                }
            }
        }
    }
}
