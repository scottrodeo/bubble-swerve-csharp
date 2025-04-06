namespace BubbleSwerve;

public class Engine
{
    private MainWindow? mainWindow;

    public Engine()
    {
        // Constructor does nothing for now (good practice)
    }

    /// <summary>
    /// Bootstraps the game: sets up panels, grid, active piece, and shows main window.
    /// Called once from App.OnFrameworkInitializationCompleted().
    /// </summary>
    public void Start()
    {
        // Initialize the core game systems here
        //GameGrid = new GameGrid(12, 22, this);
        //peice = new BubbloidBar1(0, 4, GameGrid, Avalonia.Media.Colors.Blue);
        //Game = new Game(this);
        //GamePanel = new GamePanel(this);
        //MenuPanel = new MenuPanel(GamePanel);

        // Create and show main window
        mainWindow = new MainWindow();
        //mainWindow.Content = MenuPanel; // Optional: set a root content panel
        mainWindow.Show();

        //Debug.WriteLine("Engine started successfully.");
    }
}
