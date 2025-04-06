using Avalonia;
using System;

namespace BubbleSwerve;

/// <summary>
/// Entry point for the BubbleSwerve application.
/// This class bootstraps the Avalonia UI framework and starts the main application.
/// </summary>
class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        // Starts the app using the classic desktop lifetime (multi-window desktop app)
        .StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// Configures and builds the Avalonia application.
    /// </summary>
    /// <returns>Configured AppBuilder instance.</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>() // Specify the root App class
            .UsePlatformDetect()        // Automatically select the correct platform (Windows, Linux, macOS)
            .WithInterFont()            // Use Inter as the default UI font
            .LogToTrace();              // Enable Avalonia diagnostic logging to the trace output
}
