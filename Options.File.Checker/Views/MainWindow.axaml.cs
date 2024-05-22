using Avalonia.Controls;
using Avalonia.Interactivity;
using Options.File.Checker.ViewModels;
using System.Linq;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia;
using System.Text.Json;

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
            errorWindow.ErrorTextBlock.Text = "The error window broke somehow. Please make an issue for this in GitHub.";
        }
    }

    private void CheckForUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        var updateWindow = new UpdateWindow();

        updateWindow.Show();
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

            // Open the file dialog and await the user's response
            var result = await mainWindow.StorageProvider.OpenFilePickerAsync(filePickerOptions);

            if (result != null && result.Any())
            {
                var selectedFile = result[0];
                if (selectedFile != null)
                {
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

    }
}
