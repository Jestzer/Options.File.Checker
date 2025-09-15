using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;

namespace Options.File.Checker.Browser;

internal static partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
        .WithInterFont()
        .StartBrowserAppAsync("out");

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();

    [JSExport]
    internal static Task AnalyzeAsync(string licensePath, string optionsPath)
    {
        // use the static method AnalyzeData, not the class name
        _ = LicenseAndOptionsFileDataAnalyzer.AnalyzeData(licensePath, optionsPath);
        return Task.CompletedTask;
    }


}