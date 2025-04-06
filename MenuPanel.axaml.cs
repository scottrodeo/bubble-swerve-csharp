using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace BubbleSwerve;

public partial class MenuPanel : UserControl
{     
    public MainWindow _mainWindow;       // Reference to the main application window
    public GamePanel _gamePanel;         // Reference to the game panel (not used here directly)

    public MenuPanel(MainWindow mw)
    {
        _mainWindow = mw;
        InitializeComponent();           // Loads the associated XAML (MenuPanel.xaml)
    }

    // Called when the "Restart Game" button is clicked
    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Restart button clicked!");

        if (_mainWindow == null)
        {
            Console.WriteLine("_mainWindow is null!");
            return;
        }

        if (_mainWindow._gamePanel._gameGrid == null)
        {
            Console.WriteLine("_gamePanel._gameGrid is null!");
            return;
        }

        // Call the RestartGame method to reset game state
        _mainWindow._gamePanel._gameGrid.RestartGame();
    }

    // Called when the "About" button is clicked
    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("About button clicked!");

        var aboutWindow = new AboutWindow(); // Create the about window

        // When the about window is closed, return focus to the game panel
        aboutWindow.Closed += (_, __) =>
        {
            Console.WriteLine("About window closed. Refocusing the game panel.");
            _mainWindow._gamePanel.Focus();
        };

        aboutWindow.Show();                  // Show the about window
        _mainWindow._gamePanel.Focus();      // Also try focusing game panel right after opening
    }

    // Custom rendering method, useful for drawing text like score and level
    public override void Render(DrawingContext context)
    {
        Console.WriteLine("MenuPanel Render(DrawingContext context)");
        base.Render(context);
        DrawScoreAndLevel(context);          // Draw score and level
    }

    // Render the player's score and level on the screen
    private void DrawScoreAndLevel(DrawingContext drawingContext)
    {
        var typeface = new Typeface("Arial");

        // Format the score text
        var scoreText = new FormattedText(
            $"Score: {_mainWindow._gamePanel._gameGrid.Score}",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            16,
            Brushes.White
        );

        // Format the level text
        var levelText = new FormattedText(
            $"Level: {_mainWindow._gamePanel._gameGrid.Level}",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            16,
            Brushes.White
        );

        Console.WriteLine($"MenuPanel scoreText {scoreText}");

        // Draw the score and level text at specific positions
        drawingContext.DrawText(scoreText, new Point(20, 50));
        drawingContext.DrawText(levelText, new Point(20, 80));
    }
}
