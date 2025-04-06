using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace BubbleSwerve;

/// <summary>
/// The main application class. This is the entry point for your Avalonia app,
/// where the XAML and the engine are initialized.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Loads the App.xaml styles and global UI configuration.
    /// Called early during startup.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this); // Loads App.xaml
    }

    /// <summary>
    /// Called once the application framework is fully initialized.
    /// This is where you create and show your main window or start game logic.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        // Check if the app is using classic desktop lifetime (vs single-view or mobile styles)
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Instead of opening MainWindow directly...
            // var MainWindow = new MainWindow();
            // MainWindow.Start();

            // ...you start your custom game engine which will show the window
            var engine = new Engine();
            engine.Start(); // Boot up the Bubble Swerve engine and game loop

            // Optional: if you need cross-reference between UI and engine
            // engine._menuPanel._engine = engine;
        }

        base.OnFrameworkInitializationCompleted(); // Let base class finish setup
    }
}
