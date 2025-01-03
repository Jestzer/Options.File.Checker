﻿using Avalonia.Controls;
using Avalonia.Interactivity;
using Options.File.Checker.ViewModels;
using System.Linq;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Text;
using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Options.File.Checker.Views;

// Need this and the partial class below so we can use our Json goodies in AOT.
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Settings))]

internal partial class SourceGenerationContext : JsonSerializerContext { }

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        TreeViewShouldBeCleared = true;
        DataContext = new MainViewModel(); // For printing the version number.

        var settings = LoadSettings();
        LicenseFileLocationTextBox.Text = settings.LicenseFilePathSetting;
        OptionsFileLocationTextBox.Text = settings.OptionsFilePathSetting;

        Closing += MainWindow_Closing;
    }

    private static string SettingsPath()
    {
        string settingsPath;

        if (OperatingSystem.IsWindows())
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            settingsPath = Path.Combine(appDataPath, "Jestzer.Programs", "Options.File.Checker", "settings-opt-file-checker.json");
        }
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        {
            string homePath = Environment.GetEnvironmentVariable("HOME") ?? "/tmp";
            settingsPath = Path.Combine(homePath, ".config", "Jestzer.Programs", "Options.File.Checker", "settings-opt-file-checker.json");
        }
        else
        {
            settingsPath = "settings-opt-file-checker.json";
            Console.WriteLine("Warning: your operating system has been detected as something other than Windows, Linux, or macOS. " +
            "Your settings will be saved in your current directory.");
        }

        string directoryPath = Path.GetDirectoryName(settingsPath) ?? "";
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if (string.IsNullOrEmpty(directoryPath))
        {
            Console.WriteLine("Warning: your settings' directory path is null or empty. Settings may not work correctly.");
        }

        return settingsPath;
    }

    private static void SaveSettings(Settings settings)
    {
        string settingsPath = SettingsPath();
        string jsonString = JsonSerializer.Serialize(settings, SourceGenerationContext.Default.Settings);
        System.IO.File.WriteAllText(settingsPath, jsonString);
    }

    private static Settings LoadSettings()
    {
        string settingsPath = SettingsPath();
        if (!System.IO.File.Exists(settingsPath))
        {
            return new Settings(); // Return default settings if file not found.
        }
        string jsonString = System.IO.File.ReadAllText(settingsPath);
        return JsonSerializer.Deserialize(jsonString, SourceGenerationContext.Default.Settings) ?? new Settings();
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

    private bool TreeViewShouldBeCleared { get; set; }

    private void ShowErrorWindow(string errorMessage)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            // Generally speaking, the treeview should be cleared because otherwise, it'll show up after an error appears. The exception is when NNU seats are still being reported.
            OutputTextBlock.Text = string.Empty;
            if (DataContext is MainViewModel viewModel && TreeViewShouldBeCleared)
            {
                viewModel.TreeViewItems.Clear();
            }

            AnalyzerButton.IsEnabled = false;
            SaveOutputButton.IsEnabled = false;
            CheckForUpdateButton.IsEnabled = false;
            LicenseFileBrowseButton.IsEnabled = false;
            OptionsFileBrowseButton.IsEnabled = false;

            ErrorWindow errorWindow = new();
            errorWindow.ErrorTextBlock.Text = errorMessage;
            OutputTextBlock.Text = "Error: " + errorMessage;
            TreeViewShouldBeCleared = true;

            // Check if VisualRoot is not null and is a Window before casting.
            if (VisualRoot is Window window)
            {
                await errorWindow.ShowDialog(window);
            }
            else
            {
                OutputTextBlock.Text = "The error window broke somehow. Please make an issue for this in GitHub.";
            }

            AnalyzerButton.IsEnabled = true;
            SaveOutputButton.IsEnabled = true;
            CheckForUpdateButton.IsEnabled = true;
            LicenseFileBrowseButton.IsEnabled = true;
            OptionsFileBrowseButton.IsEnabled = true;
        });
    }

    private async void CheckForUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        AnalyzerButton.IsEnabled = false;
        SaveOutputButton.IsEnabled = false;
        CheckForUpdateButton.IsEnabled = false;
        LicenseFileBrowseButton.IsEnabled = false;
        OptionsFileBrowseButton.IsEnabled = false;

        var updateWindow = new UpdateWindow();

        await updateWindow.ShowDialog(this); // Putting this here, otherwise it won't center it on the MainWindow. Sorryyyyy.

        AnalyzerButton.IsEnabled = true;
        SaveOutputButton.IsEnabled = true;
        CheckForUpdateButton.IsEnabled = true;
        LicenseFileBrowseButton.IsEnabled = true;
        OptionsFileBrowseButton.IsEnabled = true;
    }

    private async void LicenseFileBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktop)
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

            // Open the file dialog and await the user's response.
            var result = await mainWindow.StorageProvider.OpenFilePickerAsync(filePickerOptions);

            if (!result.Any()) return;
            var selectedFile = result[0];
            {
                // Get file properties.
                var properties = await selectedFile.GetBasicPropertiesAsync();
                if (properties.Size != null)
                {
                    long fileSizeInBytes = (long)properties.Size;
                    const long fiftyMegabytes = 50L * 1024L * 1024L;

                    // Check the file size.
                    if (fileSizeInBytes > fiftyMegabytes)
                    {
                        ShowErrorWindow("There is an issue with the license file: it is over 50 MB and therefore, (hopefully) not a license file.");
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    // If size is within the limit, read the contents.
                    string fileContents;
                    await using (var stream = await selectedFile.OpenReadAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        fileContents = await reader.ReadToEndAsync();
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

                    if (!fileContents.Contains("SERVER") || (!fileContents.Contains("DAEMON") && !fileContents.Contains("VENDOR")))
                    {
                        ShowErrorWindow("There is an issue with the license file: it is missing the SERVER and/or DAEMON line.");
                        LicenseFileLocationTextBox.Text = string.Empty;
                        return;
                    }
                }

                // Gotta convert some things, ya know?
                var rawFilePath = selectedFile.TryGetLocalPath;
                string? filePath = rawFilePath();
                LicenseFileLocationTextBox.Text = filePath;
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
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktop)
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

                if (result == null) return;
                // Start with the contents of the OutputTextBlock
                string? fileContents = OutputTextBlock.Text;

                // Now append the contents of the TreeView.
                if (DataContext is MainViewModel viewModel)
                {
                    foreach (var item in viewModel.TreeViewItems)
                    {
                        fileContents += $"\n{item.Title}";
                        AppendChildItems(item, ref fileContents, 1);
                    }
                }

                // Save the file contents.
                await using var stream = await result.OpenWriteAsync();
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(fileContents);
            }
            else
            {
                ShowErrorWindow("Unable to open save output dialog. Please report this issue on GitHub.");
            }
        }
        catch (Exception ex)
        {
            ShowErrorWindow($"You managed to break something in the MainWindow. How? Here's the automatic message: {ex.Message} Please submit this issue on GitHub.");
        }
    }

    // Used with the SaveOutputButton method to write the contents of the TreeView to the output file.
    private static void AppendChildItems(MainWindowTreeViewItemModel parentItem, ref string fileContents, int indentLevel)
    {
        foreach (var child in parentItem.Children)
        {
            fileContents += $"\n{new string(' ', indentLevel * 2)}{child.Title}";
            AppendChildItems(child, ref fileContents, indentLevel + 1);
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

            if (!result.Any()) return;
            var selectedFile = result[0];
            {
                // Get file properties.
                var properties = await selectedFile.GetBasicPropertiesAsync();
                if (properties.Size != null)
                {
                    long fileSizeInBytes = (long)properties.Size;
                    const long fiftyMegabytes = 50L * 1024L * 1024L;

                    // Check the file size.
                    if (fileSizeInBytes > fiftyMegabytes)
                    {
                        ShowErrorWindow("There is an issue with the options file: it is over 50 MB and therefore, (hopefully) not an options file.");
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    // If size is within the limit, read the contents.
                    string fileContents;
                    await using (var stream = await selectedFile.OpenReadAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        fileContents = await reader.ReadToEndAsync();
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

                    if (fileContents.Contains("Server's System Date and Time:"))
                    {
                        ShowErrorWindow("There is an issue with the options file: it appears to be a log file, not a options file.");
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }

                    // You've made it this far, but I still don't trust you didn't select a pptx.
                    static bool ContainsNonPlaintext(string fileContents)
                    {
                        return fileContents.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t' || c > 127);
                    }

                    if (ContainsNonPlaintext(fileContents))
                    {
                        ShowErrorWindow("There is an issue with the options file: it contains non-plaintext characters and therefore, is likely not an options file.");
                        OptionsFileLocationTextBox.Text = string.Empty;
                        return;
                    }
                }

                // Gotta convert some things, ya know?
                var rawFilePath = selectedFile.TryGetLocalPath;
                string? filePath = rawFilePath();
                OptionsFileLocationTextBox.Text = filePath;
            }
        }
        else
        {
            ShowErrorWindow("idk man, you really broke something. Make an issue on GitHub for this.");
        }
    }

    private async void AnalyzerButton_Click(object sender, RoutedEventArgs e)
    {
        OutputTextBlock.Text = "Loading, please wait.";

        // Run the analysis on a background thread so that the loading message actually appears.
        // Reversing this results in the opposite happening, unfortunately, so I can't just put the loading message in the task.


        // Clear the TreeView.
        if (DataContext is MainViewModel treeViewModel)
        {
            treeViewModel.TreeViewItems.Clear();
        }


        string licenseFilePath = string.Empty;
        string optionsFilePath = string.Empty;
        bool nnuOverdraftWarningDisplayed = false;
        bool cnOverdraftWarningDisplayed = false;


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
            daemonPortIsCnuFriendly,
            caseSensitivity,
            unspecifiedLicenseOrProductKey,
            optionsFileUsesMatlabParallelServer,
            wildcardsAreUsed,
            ipAddressesAreUsed,
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

        // Check if there was an error during the analysis or gathering.
        if (!string.IsNullOrEmpty(err))
        {
            ShowErrorWindow(err);
            return;
        }
        await Task.Run(() =>
        {
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

            if (unspecifiedLicenseOrProductKey)
            {
                output.AppendLine("Please note: you did not specify a license number or product key for either one of your INCLUDE or RESERVE lines. This means we will subtract the seat from the first " +
                    "license the product appears on.\n");
            }

            if (optionsFileUsesMatlabParallelServer)
            {
                output.AppendLine("Warning: you are including MATLAB Parallel Server in your options file. Keep in mind that the username must correspond to the username as it is on the cluster. " +
                "This does not prevent users from accessing the cluster.\n");
            }

            if (wildcardsAreUsed)
            {
                output.AppendLine("Warning: you are using at least 1 wildcard in your options file. These may be unreliable or cause other issues.\n");
            }

            if (ipAddressesAreUsed)
            {
                output.AppendLine("Warning: you are using an IP address in your options file. IP addresses are often dynamic and therefore cannot be reliably used to identify users.\n");
            }

            if (licenseFileDictionary != null)
            {
                bool includeAllNNUWarningHit = false;
                bool alreadyYelledToCnuAboutPortFormat = false;

                foreach (var item in licenseFileDictionary)
                {
                    string seatOrSeats = item.Value.Item2 == 1 ? "seat" : "seats";

                    if (!daemonPortIsCnuFriendly && daemonLineHasPort && item.Value.Item4.Contains("CNU") && !alreadyYelledToCnuAboutPortFormat)
                    {
                        output.AppendLine("Please note: your license file contains a CNU license and you've specified a DAEMON port, but you did not specifically specify your DAEMON port with \"PORT=\", which is case-sensitive and recommended to do so.\n");
                        alreadyYelledToCnuAboutPortFormat = true;
                    }

                    string licenseFileLicenseOffering = item.Value.Item4;
                    if (licenseFileLicenseOffering == "lo=CN") { licenseFileLicenseOffering = "CN"; }
                    else if (licenseFileLicenseOffering == "lo=IN") { licenseFileLicenseOffering = "PLP-era Individual"; }
                    else if (licenseFileLicenseOffering == "lo=DC") { licenseFileLicenseOffering = "PLP-era DC"; }

                    if (item.Value.Item4.Contains("NNU")) // This is not an else if because I want the seat count to still print out the same.
                    {
                        if (!includeAllNNUWarningHit)
                        {
                            if (includeAllDictionary != null && includeAllDictionary.Count != 0)
                            {
                                output.AppendLine("Warning: INCLUDEALL cannot be used with NNU licenses and will not count towards their seat count.\n");
                            }
                            includeAllNNUWarningHit = true;
                        }
                    }

                    if (licenseFileLicenseOffering == "NNU" && item.Value.Item2 < 0 && !nnuOverdraftWarningDisplayed)
                    {
                        TreeViewShouldBeCleared = false;
                        ShowErrorWindow("There is an issue with the options file: you have specified more users than available on at least 1 of your NNU products. " +
                        "Please close this window and see the full output in the main window for more information.");
                        output.AppendLine("ERROR: There is an issue with the options file: you have specified more users than available on at least 1 of your NNU products. " +
                        "Please see the full output below for more information.\n");
                        nnuOverdraftWarningDisplayed = true;
                    }
                    else if (licenseFileLicenseOffering == "CN" && item.Value.Item2 < 0 && !cnOverdraftWarningDisplayed)
                    {
                        output.AppendLine("Warning: you have specified more users on a CN license than the number of seats available. " +
                        "This is introduces the possibility of License Manager Error -4 appearing, since there is not a seat available for every user to use at once.\n");
                        cnOverdraftWarningDisplayed = true;
                    }

                    Dispatcher.UIThread.Post(() =>
                    {
                        // Finally, print the stuff we want to see!
                        if (DataContext is not MainViewModel viewModel) return;
                        
                        // Main product/seat info.
                        var mainItem = new MainWindowTreeViewItemModel
                        {
                            Title = $"{item.Value.Item1} has {item.Value.Item2}/{item.Value.Item7} unassigned {seatOrSeats} on {licenseFileLicenseOffering} license number {item.Value.Item5} (product key {item.Value.Item3})."
                        };

                        // Add subitems that display what INCLUDE, INCLUDEALL, and RESERVE lines are subtracting from seat count.
                        // In case you don't look at the other subclasses, the latter 2 options will not be accepted for NNU products.
                        foreach (var subItem in item.Value.Item6)
                        {
                            mainItem.Children.Add(new MainWindowTreeViewItemModel { Title = subItem });
                        }

                        // Actually add the whole item to the tree now.
                        viewModel.TreeViewItems.Add(mainItem);
                    });
                }
            }
            else
            {
                Dispatcher.UIThread.Post(() => ShowErrorWindow("The license file dictionary is null. Please report this error on GitHub."));
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                OutputTextBlock.Text = output.ToString();
                if (string.IsNullOrWhiteSpace(OutputTextBlock.Text) && string.IsNullOrWhiteSpace(err))
                {
                    OutputTextBlock.Text = "No errors detected and no warnings needed to be printed! See the box below for information on your options file.";
                }
            });
        });
    }
}
