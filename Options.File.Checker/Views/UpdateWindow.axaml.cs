using Avalonia.Controls;
using System.Net.Http;
using System.Text.Json;
using System;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia;

namespace Options.File.Checker.Views
{
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
        {
            InitializeComponent();
            CheckForUpdate();
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

        private bool updateIsAvailable = false;

        private async void CheckForUpdate()
        {
            Version currentVersion = new(PackageVersion);

            // GitHub API URL for the latest release.
            string latestReleaseUrl = "https://api.github.com/repos/Jestzer/options.file.checker/releases/latest";

            // Use HttpClient to fetch the latest release data.
            using HttpClient client = new();

            // GitHub API requires a user-agent. I'm adding the extra headers to reduce HTTP error 403s.
            client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("options.file.checker", PackageVersion));
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
                    Version latestVersion = new(latestVersionString);

                    // Compare the current version with the latest version.
                    if (currentVersion.CompareTo(latestVersion) < 0)
                    {
                        // A newer version is available!
                        updateIsAvailable = true;
                        DownloadButtonTextBlock.Text = "Download";
                        UpdateTextBlock.Text = "A new version is available! Download it using the button below.";
                    }
                    else
                    {
                        // The current version is up-to-date.
                        UpdateTextBlock.Text = "You are using the latest release available.";
                    }
                }
                catch (JsonException ex)
                {
                    UpdateTextBlock.Text = "The Json code in this program didn't work. Here's the automatic error message it made: \"" + ex.Message + "\"";
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    UpdateTextBlock.Text = "HTTP error 403: GitHub is saying you're sending them too many requests, so... slow down, I guess? " +
                        "Here's the automatic error message: \"" + ex.Message + "\"";
                }
                catch (HttpRequestException ex)
                {
                    UpdateTextBlock.Text = "HTTP error. Here's the automatic error message: \"" + ex.Message + "\"";
                }
            }
            catch (Exception ex)
            {
                UpdateTextBlock.Text = "Oh dear, it looks this program had a hard time making the needed connection to GitHub. Make sure you're connected to the internet " +
                    "and your lousy firewall/VPN isn't blocking the connection. Here's the automated error message: \"" + ex.Message + "\"";
            }

            DownloadButton.IsEnabled = true;

            if (!updateIsAvailable)
            {
                DownloadButtonTextBlock.Text = "OK";
            }
            
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (updateIsAvailable)
            {
                var url = "https://github.com/Jestzer/options.file.checker/releases/latest";

                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true // Necessary to use the system's default web browser.
                    };

                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception ex)
                {
                    UpdateTextBlock.Text = $"Error opening URL: {ex.Message}";
                }
            }
            else
            {
                Close();
            }
        }
    }
}
