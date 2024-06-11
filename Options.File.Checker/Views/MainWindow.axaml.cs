using Avalonia.Controls;
using Avalonia.Interactivity;
using Options.File.Checker.ViewModels;
using System.Linq;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia;
using System.Text.Json;
using System.IO;
using System.Text;
using System;

namespace Options.File.Checker.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(); // For printing the version number.

        var settings = LoadSettings();
        LicenseFileLocationTextBox.Text = settings.LicenseFilePathSetting;
        OptionsFileLocationTextBox.Text = settings.OptionsFilePathSetting;

        Closing += MainWindow_Closing;
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
    public static void SaveSettings(Settings settings)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(settings, options);
        System.IO.File.WriteAllText("settings.json", jsonString);
    }

    public static Settings LoadSettings()
    {
        if (!System.IO.File.Exists("settings.json"))
        {
            return new Settings(); // Return default settings if file not found.
        }
        string jsonString = System.IO.File.ReadAllText("settings.json");
        return JsonSerializer.Deserialize<Settings>(jsonString) ?? new Settings();
    }

    private void LicenseFileLocationTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (LicenseFileLocationTextBox == null || OptionsFileLocationTextBox == null) return;

        bool isLicenseFileValid = System.IO.File.Exists(LicenseFileLocationTextBox.Text);
        bool isOptionsFileValid = System.IO.File.Exists(OptionsFileLocationTextBox.Text);
        AnalyzerButton.IsEnabled = isLicenseFileValid && isOptionsFileValid;
    }

    private void OptionsFileLocationTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        bool isLicenseFileValid = System.IO.File.Exists(LicenseFileLocationTextBox.Text);
        bool isOptionsFileValid = System.IO.File.Exists(OptionsFileLocationTextBox.Text);
        AnalyzerButton.IsEnabled = isLicenseFileValid && isOptionsFileValid;
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        var settings = LoadSettings();
        if (LicenseFileLocationTextBox.Text != null)
        {
            settings.LicenseFilePathSetting = LicenseFileLocationTextBox.Text;
        }

        if (OptionsFileLocationTextBox.Text != null)
        {
            settings.OptionsFilePathSetting = OptionsFileLocationTextBox.Text;
        }

        SaveSettings(settings);
    }

    private async void ShowErrorWindow(string errorMessage)
    {
        OutputTextBlock.Text = string.Empty;
        ErrorWindow errorWindow = new();
        errorWindow.ErrorTextBlock.Text = errorMessage;

        // Check if VisualRoot is not null and is a Window before casting
        if (this.VisualRoot is Window window)
        {
            await errorWindow.ShowDialog(window);
        }
        else
        {
            OutputTextBlock.Text = "The error window broke somehow. Please make an issue for this in GitHub.";
        }
    }

    private async void CheckForUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        var updateWindow = new UpdateWindow();

        await updateWindow.ShowDialog(this); // Putting this here, otherwise it won't center it on the MainWindow. Sorryyyyy.
    }

    private async void LicenseFileBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var mainWindow = desktop.MainWindow;

            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = "Select a license file",
                AllowMultiple = false,
                FileTypeFilter =
                [
                new FilePickerFileType("License Files") { Patterns = ["*.lic", "*.dat"] },
                new FilePickerFileType("All Files") { Patterns = ["*"] }
                ]
            };

            // Open the file dialog and await the user's response
            var result = await mainWindow.StorageProvider.OpenFilePickerAsync(filePickerOptions);

            if (result != null && result.Any())
            {
                var selectedFile = result[0];
                if (selectedFile != null)
                {
                    // Read the file contents.
                    string fileContents;
                    using (var stream = await selectedFile.OpenReadAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        fileContents = await reader.ReadToEndAsync();
                    }

                    // Check the file size indirectly via its content length.
                    long fileSizeInBytes = fileContents.Length;
                    const long FiftyMegabytes = 50L * 1024L * 1024L;

                    if (fileSizeInBytes > FiftyMegabytes)
                    {
                        ShowErrorWindow("There is an issue with the license file: it is over 50 MB and therefore, likely (hopefully) not a license file.");
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (!fileContents.Contains("INCREMENT"))
                    {
                        ShowErrorWindow("There is an issue with the license file: it is either not a license file or it is corrupted.");
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (fileContents.Contains("lo=IN") || fileContents.Contains("lo=DC") || fileContents.Contains("lo=CIN"))
                    {
                        ShowErrorWindow("There is an issue with the license file: it contains an Individual or Designated Computer license, " +
                            "which cannot use an options file.");
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (fileContents.Contains("CONTRACT_ID="))
                    {
                        ShowErrorWindow("There is an issue with the license file: it contains at least 1 non-MathWorks product.");
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (!fileContents.Contains("SERVER") || !fileContents.Contains("DAEMON"))
                    {
                        ShowErrorWindow("There is an issue with the license file: it is missing the SERVER and/or DAEMON line.");
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    // Gotta convert some things, ya know?
                    var rawFilePath = selectedFile.TryGetLocalPath;
                    string? filePath = rawFilePath();
                    LicenseFileLocationTextBox.Text = filePath;
                }
            }
        }
        else
        {
            ShowErrorWindow("idk man, you really broke something. Make an issue on GitHub for this.");
        }
    }

    private async void SaveOutputButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var mainWindow = desktop.MainWindow;

                var fileSaveOptions = new FilePickerSaveOptions
                {
                    Title = "Save Output",
                    DefaultExtension = ".txt",
                    FileTypeChoices =
                    [
                    new FilePickerFileType("Text Files") { Patterns = [ "*.txt" ] }
                    ]
                };

                var result = await mainWindow.StorageProvider.SaveFilePickerAsync(fileSaveOptions);

                if (result != null)
                {
                    string? fileContents = OutputTextBlock.Text;

                    // Save the file contents.
                    using var stream = await result.OpenWriteAsync();
                    using var writer = new StreamWriter(stream);
                    await writer.WriteAsync(fileContents);
                }
            }
            else
            {
                ShowErrorWindow("Unable to open save output dialog. Please report this issue on GitHub.");
            }
        }
        catch (Exception ex)
        {
            ShowErrorWindow($"You managed to break something. How? Here's the automatic message: {ex.Message}");
        }
    }

    private async void OptionsFileBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var mainWindow = desktop.MainWindow;

            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = "Select an options file",
                AllowMultiple = false,
                FileTypeFilter =
                [
                new FilePickerFileType("Options Files") { Patterns = ["*.opt"] },
                new FilePickerFileType("All Files") { Patterns = ["*"] }
                ]
            };

            // Open the file dialog.
            var result = await mainWindow.StorageProvider.OpenFilePickerAsync(filePickerOptions);

            if (result != null && result.Any())
            {
                var selectedFile = result[0];
                if (selectedFile != null)
                {
                    // Read the file contents.
                    string fileContents;
                    using (var stream = await selectedFile.OpenReadAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        fileContents = await reader.ReadToEndAsync();
                    }

                    // Check the file size indirectly via its content length.
                    long fileSizeInBytes = fileContents.Length;
                    const long FiftyMegabytes = 50L * 1024L * 1024L;

                    if (fileSizeInBytes > FiftyMegabytes)
                    {
                        ShowErrorWindow("The selected file is over 50 MB, which is unexpectedly large for an options file. I will assume is this not an options file.");
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(fileContents))
                    {
                        ShowErrorWindow("There is an issue with the options file: it is either empty or only contains white space.");
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    if (!fileContents.Contains("INCLUDE") && !fileContents.Contains("EXCLUDE") && !fileContents.Contains("GROUP") && !fileContents.Contains("LOG") &&
                        !fileContents.Contains("MAX") && !fileContents.Contains("TIMEOUT") && !fileContents.Contains("RESERVE") && !fileContents.Contains("BORROW") &&
                        !fileContents.Contains("LINGER") && !fileContents.Contains("DEFAULT") && !fileContents.Contains("HIDDEN"))
                    {
                        ShowErrorWindow("There is an issue with the options file: it contains no recognized options.");
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    // You've made it this far, but I still don't trust you didn't select a pptx.
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

                    if (ContainsNonPlaintext(fileContents))
                    {
                        ShowErrorWindow("There is an issue with the options file: it contains non-plaintext characters and therefore, is likely not an options file.");
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    // Gotta convert some things, ya know?
                    var rawFilePath = selectedFile.TryGetLocalPath;
                    string? filePath = rawFilePath();
                    OptionsFileLocationTextBox.Text = filePath;
                }
            }
        }
        else
        {
            ShowErrorWindow("idk man, you really broke something. Make an issue on GitHub for this.");
        }
    }

    private void AnalyzerButton_Click(object sender, RoutedEventArgs e)
    {
        string licenseFilePath = string.Empty;
        string optionsFilePath = string.Empty; // Thank you for the lousy suggestion to remove this Visual Studio.

        if (!string.IsNullOrEmpty(LicenseFileLocationTextBox.Text))
        {
            licenseFilePath = LicenseFileLocationTextBox.Text;
        }
        else
        {
            ShowErrorWindow("You did not specify a path to a license file.");
            return;
        }

        if (!string.IsNullOrEmpty(OptionsFileLocationTextBox.Text))
        {
            optionsFilePath = OptionsFileLocationTextBox.Text;
        }
        else
        {
            ShowErrorWindow("You did not specify a path to an options file.");
            return;
        }


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
            output.AppendLine("Warning: case sensitivity is enabled for users defined in GROUPs and HOST_GROUPs.\n");
        }

        // Warn the user if they didn't specify a license number or product key in their seat-subtracting option entries.
        if (unspecifiedLicenseOrProductKey)
        {
            output.AppendLine("Please note: you did not specify a license number or product key for either one of your INCLUDE or RESERVE lines. This means we will subtract the seat from the first " +
                "license the product appears on.\n");
        }

        // Print seatCount.
        if (licenseFileDictionary != null)
        {
            bool overdraftCNWarningHit = false;
            bool includeAllNNUWarningHit = false;
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
                    if (item.Value.Item4.Contains("NNU")) // This is not an else if because I want the seat count to still print out the same.
                    {
                        if (!includeAllNNUWarningHit)
                        {
                            if (includeAllDictionary != null)
                            {
                                if (includeAllDictionary.Count == 0)
                                {
                                    output.AppendLine("Warning: INCLUDEALL cannot be used NNU licenses and will not count towards their seat count.\n");
                                }
                            }
                            includeAllNNUWarningHit = true;
                        }
                    }
                    // Finally, print the stuff we want to see!
                    output.AppendLine($"{item.Value.Item1} has {item.Value.Item2} unassigned {seatOrSeats} on license number {item.Value.Item5} (product key {item.Value.Item3}).");
                }
            }
        }
        else
        {
            ShowErrorWindow("The license file dictionary is null. Please report this error on GitHub.");
            return;
        }

        // Show us the goods, since we didn't hit any critical errors.
        OutputTextBlock.Text = output.ToString();
    }
}
