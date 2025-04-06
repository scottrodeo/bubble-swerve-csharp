using System;
using Avalonia.Controls;
using System.Runtime.InteropServices;

namespace BubbleSwerve;

public partial class MainWindow : Window
{
    // Fields for UI panels
    public MenuPanel _menuPanel { get; set; }
    public GamePanel _gamePanel { get; set; }

    public MainWindow()
    {
        InitializeComponent(); // Load XAML components

        this.WindowState = WindowState.Maximized; // Start the window in maximized mode

        // Find the Grid control named "MainGrid" defined in MainWindow.xaml
        var grid = this.FindControl<Grid>("MainGrid");
        if (grid == null)
        {
            Console.WriteLine("MainGrid not found!");
            return; // Early exit if the grid isn't found (this prevents a crash)
        }

        // Instantiate the game and menu panels
        _gamePanel = new GamePanel(this);
        _menuPanel = new MenuPanel(this);

        // Place each panel in its respective row
        Grid.SetRow(_menuPanel, 0); // Top row (e.g., for UI like score, pause)
        Grid.SetRow(_gamePanel, 1); // Bottom row (gameplay area)

        // Add panels to the main grid
        grid.Children.Add(_menuPanel);
        grid.Children.Add(_gamePanel);

        Console.WriteLine("MenuPanel and GamePanel added to grid.");

        // Force layout to update immediately
        grid.InvalidateMeasure();
        grid.InvalidateArrange();
        grid.InvalidateVisual();

        // Automatically focus the game panel when it's ready
        _gamePanel.AttachedToVisualTree += (_, __) =>
        {
            _gamePanel.Focus();
        };
    }
}
