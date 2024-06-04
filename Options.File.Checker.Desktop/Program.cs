using System;
using System.Linq;
using Avalonia;

namespace Options.File.Checker.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Check if the program was launched with a specific argument to run without the GUI
        if (args.Contains("-nogui"))
        {
            RunNonGUIMode(args);
        }
        else
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    // Method for handling non-GUI mode logic
    private static void RunNonGUIMode(string[] args)
    {
        // Your non-GUI logic here
        Console.WriteLine("Running in non-GUI mode");
        // Example: Process command-line arguments and perform operations accordingly

        // Since we're in non-GUI mode, you might want to explicitly exit the program when done
        Environment.Exit(0);
    }
}