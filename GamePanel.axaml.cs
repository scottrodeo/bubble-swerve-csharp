using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace BubbleSwerve;

/// <summary>
/// GamePanel hosts the GameGrid and handles game input, resizing, and orientation-aware controls.
/// </summary>
public partial class GamePanel : UserControl
{
    private Grid _container;             // Container to host the game grid
    public GameGrid _gameGrid;           // The main grid where gameplay occurs
    public MainWindow _mainWindow;       // Reference to the main window (for callbacks)

    private double _panelWidth;
    private double _panelHeight;

    public int cellSize;                 // Size of each bubble cell

    public GamePanel(MainWindow mw)
    {
        this.Focusable = true;
        _mainWindow = mw;

        this.Background = Brushes.Black; // Set the panel background to black

        InitializeComponent();           // Load associated XAML
        initializeGamePanel();          // Set up internal grid and game state
    }

    /// <summary>
    /// Initializes the container and game grid, and hooks up event handlers.
    /// </summary>
    public void initializeGamePanel()
    {
        _container = new Grid();
        this.Content = _container;

        _gameGrid = new GameGrid(12, 22, _mainWindow); // Initialize the grid with dimensions

        _container.Children.Add(_gameGrid); // Add the grid to the panel

        this.SizeChanged += OnSizeChanged; // Hook resize event
        this.KeyDown += OnKeyDown;         // Hook key press event
        this.Focusable = true;
        this.Focus();                      // Request keyboard focus
    }

    /// <summary>
    /// When the panel size changes, adjust grid cell size to fit.
    /// </summary>
    private void OnSizeChanged(object? sender, System.EventArgs e)
    {
        getCellSize(); // Recalculate cell size
        _gameGrid.SetCellSize(cellSize); // Apply to game grid
        _gameGrid.Width = _gameGrid.gridWidth * cellSize;
        _gameGrid.Height = _gameGrid.gridHeight * cellSize;
    }

    /// <summary>
    /// Dynamically calculates optimal cell size based on panel size.
    /// </summary>
    public void getCellSize()
    {
        _panelWidth = this.Bounds.Width;
        _panelHeight = this.Bounds.Height;

        int windowMax = (int)System.Math.Min(_panelWidth - 200, _panelHeight - 200); // Leave padding
        cellSize = (int)System.Math.Min((double)windowMax / _gameGrid.gridWidth, (double)windowMax / _gameGrid.gridHeight);

        Debug.WriteLine($"GamePanel Width: {_panelWidth}, Height: {_panelHeight}");
        Debug.WriteLine($"CellSize: {cellSize}");
    }

    /// <summary>
    /// Main keyboard input handler for rotation, movement, hard drop, and pause.
    /// Orientation-aware layout means controls are mapped according to gravity.
    /// </summary>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.R:
                _gameGrid?.RotateBoardClockwise();
                break;

            case Key.Space:
                _gameGrid?.HardDrop();
                break;

            case Key.Q:
                _gameGrid?.HandleRotate(1);// "Rotate Counter Clockwise"
                break;

            case Key.RightShift:
                _gameGrid?.HandleRotate(1);// "Rotate Counter Clockwise"
                break;

            case Key.Escape:
                _gameGrid?.Pause();// "Rotate Counter Clockwise"
                break;
        }


        if (_gameGrid.isPaused) return;

        switch (_gameGrid.currentOrientation) {

            case Orientation.DOWN:

                switch (e.Key)
                {
                    case Key.Left:
                        _gameGrid?.HandleMove(Direction.LEFT);
                        break;
                    case Key.A:
                        _gameGrid?.HandleMove(Direction.LEFT);
                        break;

                    case Key.Right:
                        _gameGrid?.HandleMove(Direction.RIGHT);
                        break;
                    case Key.D:
                        _gameGrid?.HandleMove(Direction.RIGHT);
                        break;

                    case Key.Up:
                        _gameGrid?.HandleRotate(0);// "Rotate Clockwise"
                        break;
                    case Key.W:
                        _gameGrid?.HandleRotate(0);// "Rotate Clockwise"
                        break;

                    case Key.Down:
                        _gameGrid?.HandleMove(Direction.DOWN);
                        _gameGrid.Score += 1;
                        break;
                    case Key.S:
                        _gameGrid?.HandleMove(Direction.DOWN);
                        _gameGrid.Score += 1;
                        break;
                }
                break;

            case Orientation.RIGHT:

                switch (e.Key)
                {
                    case Key.Left:
                        _gameGrid?.HandleRotate(0);// "Rotate Clockwise"
                        break;
                    case Key.A:
                        _gameGrid?.HandleRotate(0);// "Rotate Clockwise"
                        break;

                    case Key.Right:
                        _gameGrid?.HandleMove(Direction.RIGHT);
                        _gameGrid.Score += 1;
                        break;
                    case Key.D:
                        _gameGrid?.HandleMove(Direction.RIGHT);
                        _gameGrid.Score += 1;
                        break;

                    case Key.Up:
                        _gameGrid?.HandleMove(Direction.UP);
                        break;
                    case Key.W:
                        _gameGrid?.HandleMove(Direction.UP);
                        break;

                    case Key.Down:
                        _gameGrid?.HandleMove(Direction.DOWN);
                        break;
                    case Key.S:
                        _gameGrid?.HandleMove(Direction.DOWN);
                        break;
                }
                break;

            case Orientation.UP:

                switch (e.Key)
                {
                    case Key.Left:
                        _gameGrid?.HandleMove(Direction.LEFT);
                        break;
                    case Key.A:
                        _gameGrid?.HandleMove(Direction.LEFT);
                        break;

                    case Key.Right:
                        _gameGrid?.HandleMove(Direction.RIGHT);
                        break;
                    case Key.D:
                        _gameGrid?.HandleMove(Direction.RIGHT);
                        break;

                    case Key.Up:
                        _gameGrid?.HandleMove(Direction.UP);
                        _gameGrid.Score += 1;
                        break;
                    case Key.W:
                        _gameGrid?.HandleMove(Direction.UP);
                        _gameGrid.Score += 1;
                        break;

                    case Key.Down:
                        _gameGrid?.HandleRotate(0);// "Rotate Clockwise"
                        break;
                    case Key.S:
                        _gameGrid?.HandleRotate(0);// "Rotate Clockwise"
                        break;
                }
                break;

            case Orientation.LEFT:

                switch (e.Key)
                {
                    case Key.Left:
                        _gameGrid?.HandleMove(Direction.LEFT);
                        _gameGrid.Score += 1;
                        break;
                    case Key.A:
                        _gameGrid?.HandleMove(Direction.LEFT);
                        _gameGrid.Score += 1;
                        break;

                    case Key.Right:
                        _gameGrid?.HandleRotate(0);// "Rotate Clockwise"	;
                        break;
                    case Key.D:
                        _gameGrid?.HandleRotate(0);// "Rotate Clockwise"	;
                        break;

                    case Key.Up:
                        _gameGrid?.HandleMove(Direction.UP);
                        break;
                    case Key.W:
                        _gameGrid?.HandleMove(Direction.UP);
                        break;

                    case Key.Down:
                        _gameGrid?.HandleMove(Direction.DOWN);
                        break;
                    case Key.S:
                        _gameGrid?.HandleMove(Direction.DOWN);
                        break;
                }
                break;
        }
        _gameGrid.ActivePieceUpdate();
    }

    /// <summary>
    /// Maps keys to directions and rotations depending on orientation.
    /// </summary>
    private void HandleStandardInput(KeyEventArgs e, Direction left, Direction right, Direction down, Direction rotate)
    {
        switch (e.Key)
        {
            case Key.Left:
            case Key.A:
                _gameGrid?.HandleMove(left);
                if (rotate == Direction.LEFT || rotate == Direction.RIGHT) _gameGrid.Score += 1;
                break;

            case Key.Right:
            case Key.D:
                _gameGrid?.HandleMove(right);
                if (rotate == Direction.LEFT || rotate == Direction.RIGHT) _gameGrid.Score += 1;
                break;

            case Key.Down:
            case Key.S:
                _gameGrid?.HandleMove(down);
                if (down == Direction.DOWN || down == Direction.UP) _gameGrid.Score += 1;
                break;

            case Key.Up:
            case Key.W:
                _gameGrid?.HandleRotate(0); // Clockwise
                break;
        }
    }
}
