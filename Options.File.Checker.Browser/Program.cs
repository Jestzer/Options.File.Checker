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
}