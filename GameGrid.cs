using System;
using System.Diagnostics;
using System.Globalization;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using BubbleSwerve;

/// <summary>
/// Represents the interactive grid for the Bubble Swerve game.
/// Handles game logic, rendering, scoring, orientation, and input.
/// </summary>
public class GameGrid : UserControl
{
    // Logical grid representing the current state of the board (null = empty cell)
    private Bubble[,] board;

    // Pixel size of each cell on the board (used for rendering)
    public int cellSize;

    // Number of columns and rows in the current grid layout
    public int gridWidth;
    public int gridHeight;

    // Previous visual grid, used when swapping or redrawing
    private Grid gridPanelPrevious;

    // The currently active visual grid displaying bubbles
    public Grid gridPanel;

    // Gravity timer that controls how often pieces fall
    public Timer timer;

    // Reference to the main application window (used for focus and UI management)
    public MainWindow _mainWindow;

    // Timestamp used to throttle piece rotation (prevents rapid key repeat abuse)
    private DateTime lastRotationTime = DateTime.MinValue;

    // Time delay required between consecutive rotations
    private readonly TimeSpan rotationCooldown = TimeSpan.FromMilliseconds(50);

    // Optional event that fires whenever a new piece is created
    public event Action<AbstractBubbloid>? PieceCreated;

    // The currently falling active piece
    public AbstractBubbloid? piece;

    // Current board orientation (determines gravity direction and rotation behavior)
    public Orientation currentOrientation = Orientation.DOWN; // Default is downward

    // Predefined colors for randomized Bubbloid shapes
    public static Color b1 = Color.FromArgb(255, 175, 18, 202);
    public static Color b2 = Color.FromArgb(255, 101, 216, 246);
    public static Color b3 = Color.FromArgb(255, 74, 125, 255);
    public static Color b4 = Color.FromArgb(255, 79, 255, 254);
    public static Color b5 = Color.FromArgb(255, 230, 56, 174);
    public static Color b6 = Color.FromArgb(255, 249, 116, 122);
    public static Color b7 = Color.FromArgb(255, 45, 50, 116);
    public static Color b8 = Color.FromArgb(255, 99, 32, 178);
    public static Color b9 = Color.FromArgb(255, 65, 84, 203);

    // Temporary position markers used during operations like MoveActivePiece
    private int bubble_col;
    private int bubble_row;

    // Used as a buffer during board rotation
    private Bubble[,] newBoard;

    // Game state flags
    public bool isPaused;       // True if game is paused
    public bool isGameOver;     // True if game has ended

    // Scorekeeping
    public int Score { get; set; } = 0;              // Player's current score
    public int Level { get; set; } = 1;              // Current level (affects scoring)
    public int LinesCleared { get; private set; } = 0; // Total cleared lines (read-only externally)

    /// <summary>
    /// Initializes a new game grid with the specified width and height,
    /// connects it to the main window, and starts the game loop.
    /// </summary>
    public GameGrid(int gridWidth, int gridHeight, MainWindow mw)
    {
        // Store reference to the main window (used for UI focus, access to game panel, etc.)
        _mainWindow = mw;
        
        // Store grid dimensions
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;

        // Initialize game state
        this.isPaused = false;
        this.isGameOver = false;

        // Log grid size for debug purposes
        System.Diagnostics.Debug.WriteLine($"GameGrid(): {gridWidth}, {gridHeight}");

        // Allocate the logical board (rows × columns)
        board = new Bubble[gridHeight, gridWidth];

        // (Optional debug line — could be used to manually test a placed bubble)
        // var bubble = new Bubble(this, 2, 3, Colors.Blue, true);
        // this.AddBubble(2, 3, bubble); // Adds a bubble to both logic and visual grid

        // Drop the first random Bubbloid piece into the board
        SpawnNewPiece();

        // Begin the gravity timer (causes pieces to fall)
        StartTimer();

        // Placeholder for a future game logic manager instance
        // this.Game = new Game(this);
    }

    /// <summary>
    /// Resets the game to its initial state. Clears the board, resets dimensions,
    /// orientation, UI, and spawns a new piece. Also restarts the game loop timer.
    /// </summary>
    public void RestartGame()
    {
        // Set the board dimensions back to default
        this.gridWidth = 12;
        this.gridHeight = 22;

        // Remove the current active piece and reset game-over flag
        this.piece = null;
        this.isGameOver = false;

        // Re-initialize the logical board
        this.board = new Bubble[gridHeight, gridWidth];

        // Reset orientation to default (DOWN) for a consistent restart
        this.currentOrientation = Orientation.DOWN;

        // Update the visual dimensions of the GameGrid control
        this.Width = gridWidth * cellSize;
        this.Height = gridHeight * cellSize;

        // Rebuild the UI grid structure
        InitializeGrid();

        // Redraw the visual grid to match the new logical board
        UpdateVisualGridFromBoard();

        // Alternative reset logic that was previously commented out
        // Score = 0;    // Optionally reset score here
        // Level = 1;    // Reset level

        // Spawn a fresh piece to begin gameplay
        SpawnNewPiece();

        // Restart gravity loop
        StartTimer();

        // Set keyboard focus back to the game panel (for input handling)
        _mainWindow._gamePanel.Focus();
    }

    /// <summary>
    /// Initializes or restarts the gravity timer that controls automatic piece falling.
    /// Called at game start and during restarts.
    /// </summary>
    public void StartTimer()
    {
        // Check if the timer hasn't been created yet
        if (timer == null)
        {
            // Create a new timer that triggers every 2 seconds (2000 ms)
            timer = new Timer(2000);

            // Hook up the Elapsed event to apply gravity to the active piece
            timer.Elapsed += applyGravity;

            // Ensure the timer keeps running continuously
            timer.AutoReset = true;

            // Start the timer
            timer.Start();
        }
        else
        {
            // Timer already exists — stop and restart to reset its cycle
            timer.Stop();
            timer.Start();
        }
    }

    /// <summary>
    /// Called on each timer tick to apply gravity logic. Attempts to move the active piece
    /// in the current orientation's direction. If blocked, the piece is locked into place.
    /// </summary>
    public void applyGravity(object? sender, ElapsedEventArgs e)
    {
        // Exit early if the game is over
        if (isGameOver)
        {
            return;
        }

        if (piece != null)
        {
            int retryCount = 0; // Tracks retry attempts for IsMoving state
            const int maxRetries = 5; // Max allowed retries before giving up

            // If the piece is currently flagged as moving, wait until it finishes
            while (piece.IsMoving && retryCount < maxRetries)
            {
                System.Diagnostics.Debug.WriteLine($"applyGravity retrying: attempt {retryCount + 1}");
                retryCount++;

                // Wait briefly to avoid rapid reentry (note: blocks thread!)
                System.Threading.Thread.Sleep(10);
            }

            // Give up if retries exceeded to avoid stalling or infinite loop
            if (retryCount >= maxRetries)
            {
                System.Diagnostics.Debug.WriteLine("applyGravity aborted: max retries reached");
                return;
            }

            // Mark piece as in-motion to prevent other actions during this tick
            piece.IsMoving = true;
        }

        // Try to move the piece in the current orientation direction
        if (piece != null)
        {
            switch (currentOrientation)
            {
                case Orientation.DOWN:
                    if (piece.CanMove(Direction.DOWN))
                        piece.Move(Direction.DOWN);
                    else
                        LockPiece(); // Can't move — lock it in
                    break;

                case Orientation.LEFT:
                    if (piece.CanMove(Direction.LEFT))
                        piece.Move(Direction.LEFT);
                    else
                        LockPiece();
                    break;

                case Orientation.RIGHT:
                    if (piece.CanMove(Direction.RIGHT))
                        piece.Move(Direction.RIGHT);
                    else
                        LockPiece();
                    break;

                case Orientation.UP:
                    if (piece.CanMove(Direction.UP))
                        piece.Move(Direction.UP);
                    else
                        LockPiece();
                    break;
            }
        }

        // Schedule UI update for the moved or locked piece
        Dispatcher.UIThread.Post(ActivePieceUpdate);

        // Reset movement lock
        if (piece != null)
        {
            piece.IsMoving = false;
        }
    }

    /// <summary>
    /// Locks the current piece into the board grid, checks for full lines, and rotates the board.
    /// Also determines if the game is over after locking.
    /// </summary>
    public void LockPiece()
    {
        // Safety check — don't proceed if there's no active piece
        if (piece == null) return;

        // Exit early if game is already over (prevents further updates)
        if (isGameOver) return;

        System.Diagnostics.Debug.WriteLine("GameGrid: LockPiece()");

        // Commit each bubble from the piece into the logical board
        foreach (var bubble in piece.Bubbles)
        {
            // Only place the bubble if the target cell is valid and empty
            if (IsValidAndEmpty(bubble.row, bubble.col))
            {
                board[bubble.row, bubble.col] = bubble;
            }
            else
            {
                // Collision or out-of-bounds means the game should end
                System.Diagnostics.Debug.WriteLine("Game Over: Piece out of bounds or collision detected.");
                //EndGame(); // Could be hooked here
                return;
            }
        }

        // Clear reference to the now-locked piece
        piece = null;

        // Check for completed rows or columns and clear them
        ClearFullRows();

        // Trigger board rotation on lock to create unique rotational gameplay
        Dispatcher.UIThread.Post(RotateBoardClockwise);

        // Recheck for game-over after the lock
        if (IsGameOver())
        {
            isGameOver = true;
            timer.Stop(); // Stop game progression if game has ended
        }
    }

    /// <summary>
    /// Checks whether the game should be considered over based on whether the spawn zone
    /// for the current orientation is already occupied.
    /// </summary>
    public bool IsGameOver()
    {
        // Log the current orientation being checked
        System.Diagnostics.Debug.WriteLine($"IsGameOver() {currentOrientation}");

        // Determine which row or column represents the spawn zone for the current orientation
        switch (currentOrientation)
        {
            case Orientation.DOWN:
                // If pieces spawn from the top, check if row 4 is already occupied
                if (IsRowOccupied(4))
                {
                    System.Diagnostics.Debug.WriteLine("IsGameOver() IsRowOccupied(3)");
                    return true;
                }
                break;

            case Orientation.UP:
                // If pieces spawn from the bottom, check near the bottom edge
                if (IsRowOccupied(gridHeight - 5))
                {
                    System.Diagnostics.Debug.WriteLine("IsGameOver() IsRowOccupied(gridHeight - 4)");
                    return true;
                }
                break;

            case Orientation.LEFT:
                // If pieces spawn from the right, check the rightmost column
                if (IsColOccupied(gridWidth - 5))
                {
                    System.Diagnostics.Debug.WriteLine("IsGameOver() IsColOccupied(gridWidth - 4)");
                    return true;
                }
                break;

            case Orientation.RIGHT:
                // If pieces spawn from the left, check column 4
                if (IsColOccupied(4))
                {
                    System.Diagnostics.Debug.WriteLine("IsGameOver() IsColOccupied(3)");
                    return true;
                }
                break;
        }

        // No blocking pieces detected — game can continue
        System.Diagnostics.Debug.WriteLine("IsGameOver() not over");
        return false;
    }

    /// <summary>
    /// Checks if the specified row contains any bubbles (i.e., is not completely empty).
    /// Used primarily to detect game-over conditions in the spawn zone.
    /// </summary>
    /// <param name="row">The index of the row to check.</param>
    /// <returns>True if any cell in the row is occupied; otherwise, false.</returns>
    private bool IsRowOccupied(int row)
    {
        // Iterate through each column in the specified row
        for (int col = 0; col < gridWidth; col++)
        {
            // If any cell is occupied by a bubble, the row is considered occupied
            if (board[row, col] != null)
            {
                return true;
            }
        }

        // If all cells are empty, the row is not occupied
        return false;
    }

    /// <summary>
    /// Checks if the specified column contains any bubbles (i.e., is not completely empty).
    /// Used primarily to detect game-over conditions when pieces spawn from the side.
    /// </summary>
    /// <param name="col">The index of the column to check.</param>
    /// <returns>True if any cell in the column is occupied; otherwise, false.</returns>
    private bool IsColOccupied(int col)
    {
        // Iterate through each row in the specified column
        for (int row = 0; row < gridHeight; row++)
        {
            // If any cell is occupied by a bubble, the column is considered occupied
            if (board[row, col] != null)
            {
                return true;
            }
        }

        // If all cells are empty, the column is not occupied
        return false;
    }

    /// <summary>
    /// Detects and clears fully filled rows or columns depending on the current orientation.
    /// Shifts remaining bubbles to fill the gaps, updates visuals and scoring.
    /// </summary>
    public void ClearFullRows()
    {
        int rowsCleared = 0; // Keeps track of how many lines were cleared this cycle

        // Determine clearing logic based on the board's orientation
        switch (currentOrientation)
        {
            case Orientation.DOWN:
                // Loop from bottom to top to simulate gravity pulling pieces down
                for (int row = gridHeight - 1; row >= 0; row--)
                {
                    if (IsRowFull(row))
                    {
                        ClearRow(row);          // Clear the filled row
                        rowsCleared++;
                        ShiftRowsDown(row);    // Shift everything above it downward
                        row++; // Re-check this row, since contents from above moved into it
                    }
                }
                break;

            case Orientation.RIGHT:
                // Traverse left to right, checking if any column is full
                for (int col = 0; col < gridWidth; col++)
                {
                    if (IsColFull(col))
                    {
                        ClearCol(col);         // Clear the filled column
                        rowsCleared++;
                        ShiftColsRight(col);   // Shift columns left into the cleared space
                        col++; // Re-check the same column index now that it has shifted content
                    }
                }
                break;

            case Orientation.UP:
                // Loop from top to bottom (gravity reversed)
                for (int row = 0; row < gridHeight; row++)
                {
                    if (IsRowFull(row))
                    {
                        ClearRow(row);         // Clear the filled row
                        rowsCleared++;
                        ShiftRowsUp(row);      // Push upper content downward
                        row++; // Re-check this row, now filled by the one above
                    }
                }
                break;

            case Orientation.LEFT:
                // Traverse right to left for leftward gravity
                for (int col = gridWidth - 1; col >= 0; col--)
                {
                    if (IsColFull(col))
                    {
                        ClearCol(col);         // Clear the filled column
                        rowsCleared++;
                        ShiftColsLeft(col);    // Shift content rightward into the cleared column
                        col++; // Re-check this column as new content just moved in
                    }
                }
                break;
        }

        // If any rows or columns were cleared, update visuals and score
        if (rowsCleared > 0)
        {
            System.Diagnostics.Debug.WriteLine($"Cleared {rowsCleared} rows!");

            // Post the visual update to the UI thread (non-blocking)
            Dispatcher.UIThread.Post(UpdateVisualGridFromBoard);

            // Increase score and update level based on how many lines were cleared
            UpdateScore(rowsCleared);
        }
    }

    /// <summary>
    /// Checks if a given row is completely filled with bubbles (no empty cells).
    /// Used to determine when a row should be cleared.
    /// </summary>
    /// <param name="row">The index of the row to check.</param>
    /// <returns>True if every cell in the row is occupied; otherwise, false.</returns>
    private bool IsRowFull(int row)
    {
        // Iterate across all columns in the specified row
        for (int col = 0; col < gridWidth; col++)
        {
            // If any cell is empty, the row is not full
            if (board[row, col] == null)
            {
                return false;
            }
        }

        // All cells in the row are filled — it's a full row
        return true;
    }

    /// <summary>
    /// Checks if a given column is completely filled with bubbles (no empty cells).
    /// Used to determine when a column should be cleared based on orientation.
    /// </summary>
    /// <param name="col">The index of the column to check.</param>
    /// <returns>True if every cell in the column is occupied; otherwise, false.</returns>
    private bool IsColFull(int col)
    {
        // Iterate through each row in the specified column
        for (int row = 0; row < gridHeight; row++)
        {
            // If any cell is empty, the column is not full
            if (board[row, col] == null)
            {
                return false;
            }
        }

        // All cells in the column are filled — it's a full column
        return true;
    }

    /// <summary>
    /// Clears all cells in the specified row by setting them to null.
    /// </summary>
    /// <param name="row">The index of the row to clear.</param>
    private void ClearRow(int row)
    {
        for (int col = 0; col < gridWidth; col++)
        {
            board[row, col] = null; // Remove the bubble at (row, col)
        }
    }

    /// <summary>
    /// Clears all cells in the specified column by setting them to null.
    /// </summary>
    /// <param name="col">The index of the column to clear.</param>
    private void ClearCol(int col)
    {
        for (int row = 0; row < gridHeight; row++)
        {
            board[row, col] = null; // Remove the bubble at (row, col)
        }
    }

    /// <summary>
    /// Shifts all rows above the cleared row downward by one step.
    /// Starts from the given row and goes upward.
    /// </summary>
    /// <param name="startRow">The row index that was just cleared.</param>
    private void ShiftRowsDown(int startRow)
    {
        for (int row = startRow; row > 0; row--)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                board[row, col] = board[row - 1, col]; // Copy bubble from the row above
                board[row - 1, col] = null;            // Clear the original position
            }
        }
    }

    /// <summary>
    /// Shifts all rows below the cleared row upward by one step.
    /// Starts from the given row and pushes content upward.
    /// </summary>
    /// <param name="startRow">The row index that was just cleared.</param>
    private void ShiftRowsUp(int startRow)
    {
        for (int row = startRow; row < gridHeight - 1; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                board[row, col] = board[row + 1, col]; // Pull bubble from the row below
                board[row + 1, col] = null;            // Clear the original position
            }
        }
    }

    /// <summary>
    /// Shifts all columns left of the cleared column to the right by one step.
    /// Starts at the cleared column and works leftward.
    /// </summary>
    /// <param name="startCol">The column index that was just cleared.</param>
    private void ShiftColsRight(int startCol)
    {
        for (int col = startCol; col > 0; col--)
        {
            for (int row = 0; row < gridHeight; row++)
            {
                board[row, col] = board[row, col - 1]; // Move bubble from the column to the left
                board[row, col - 1] = null;            // Clear the original position
            }
        }
    }

    /// <summary>
    /// Shifts all columns right of the cleared column to the left by one step.
    /// Starts at the cleared column and works rightward.
    /// </summary>
    /// <param name="startCol">The column index that was just cleared.</param>
    private void ShiftColsLeft(int startCol)
    {
        for (int col = startCol; col < gridWidth - 1; col++)
        {
            for (int row = 0; row < gridHeight; row++)
            {
                board[row, col] = board[row, col + 1]; // Move bubble from the column to the right
                board[row, col + 1] = null;            // Clear the original position
            }
        }
    }

    /// <summary>
    /// Rotates the entire game board 90 degrees clockwise.
    /// Transposes the board array, swaps dimensions, updates orientation,
    /// and refreshes both the logical and visual state.
    /// </summary>
    public void RotateBoardClockwise()
    {
        piece = null; // Clear the active piece before rotating to avoid transformation issues

        // Create a new board with transposed dimensions (width becomes height and vice versa)
        var newBoard = new Bubble[gridWidth, gridHeight];

        // Transpose the matrix and rotate it clockwise
        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                // Move each bubble to its new rotated position
                newBoard[col, gridHeight - 1 - row] = board[row, col];
            }
        }

        // Replace the old board with the rotated one
        board = newBoard;

        // Swap the board dimension values
        int temp = gridWidth;
        gridWidth = gridHeight;
        gridHeight = temp;

        // Update the visual size of the GameGrid to match the new dimensions
        this.Width = this.gridWidth * cellSize;
        this.Height = this.gridHeight * cellSize;

        // Cycle to the next clockwise orientation
        switch (currentOrientation)
        {
            case Orientation.DOWN:
                currentOrientation = Orientation.LEFT;
                break;
            case Orientation.RIGHT:
                currentOrientation = Orientation.DOWN;
                break;
            case Orientation.UP:
                currentOrientation = Orientation.RIGHT;
                break;
            case Orientation.LEFT:
                currentOrientation = Orientation.UP;
                break;
        }

        // Log the new orientation for debugging
        System.Diagnostics.Debug.WriteLine($"GameGrid: RotateBoardClockwise(): orietation: {currentOrientation}");

        // Rebuild the visual grid layout structure
        InitializeGrid();

        // Redraw the board and spawn a new piece
        Dispatcher.UIThread.Post(UpdateVisualGridFromBoard);
        Dispatcher.UIThread.Post(SpawnNewPiece);
    }

    /// <summary>
    /// Initializes the visual grid (gridPanel) with correct rows and columns
    /// based on the current grid dimensions.
    /// </summary>
    private void InitializeGrid()
    {
        System.Diagnostics.Debug.WriteLine($"GameGrid InitializeGrid(): gridWidth:{gridWidth}, gridHeight:{gridHeight}");

        // Preserve the old grid panel so bubbles can be removed from it
        gridPanelPrevious = gridPanel;

        // Create a fresh Grid for the UI
        gridPanel = new Grid();

        // Add the necessary number of row definitions (one per row)
        for (int i = 0; i < gridHeight; i++)
        {
            gridPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        }

        // Add the necessary number of column definitions (one per column)
        for (int i = 0; i < gridWidth; i++)
        {
            gridPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        }

        // Set the new visual grid as the content of the GameGrid
        this.Content = gridPanel;
    }

    /// <summary>
    /// Spawns a new random Bubbloid piece at an appropriate starting position based on current orientation.
    /// This runs on the UI thread to ensure proper visual updates.
    /// </summary>
    private void SpawnNewPiece()
    {
        // Ensure UI updates and object creation happen on the Avalonia UI thread
        Dispatcher.UIThread.Post(() =>
        {
            System.Diagnostics.Debug.WriteLine("spawnNewPiece()");

            // Default spawn location (may be adjusted based on orientation)
            int initialRow = 1;
            int initialColumn = gridWidth / 2 - 1;

            // Adjust spawn point depending on which direction the game board is currently oriented
            switch (currentOrientation)
            {
                case Orientation.DOWN:
                    initialRow = 2;
                    initialColumn = gridWidth / 2 - 1;
                    break;
                case Orientation.UP:
                    initialRow = gridHeight - 2;
                    initialColumn = gridWidth / 2 - 1;
                    break;
                case Orientation.LEFT:
                    initialRow = gridHeight / 2 - 1;
                    initialColumn = gridWidth - 2;
                    break;
                case Orientation.RIGHT:
                    initialRow = gridHeight / 2 - 1;
                    initialColumn = 1;
                    break;
            }

            // Generate a random number from 1 to 9 to determine which shape to spawn
            Random randomGenerator = new Random();
            int random = randomGenerator.Next(1, 10); // 1 through 9 inclusive

            // Instantiate a new Bubbloid shape based on the random result
            switch (random)
            {
                case 1:
                    this.piece = new BubbloidBar1(initialRow, initialColumn, this, GameGrid.b1);
                    break;
                case 2:
                    this.piece = new BubbloidBar3(initialRow, initialColumn, this, GameGrid.b2);
                    break;
                case 3:
                    this.piece = new BubbloidL5(initialRow, initialColumn, this, GameGrid.b3);
                    break;
                case 4:
                    this.piece = new BubbloidBar2(initialRow, initialColumn, this, GameGrid.b4);
                    break;
                case 5:
                    this.piece = new BubbloidCross5(initialRow, initialColumn, this, GameGrid.b5);
                    break;
                case 6:
                    this.piece = new BubbloidV3(initialRow, initialColumn, this, GameGrid.b6);
                    break;
                case 7:
                    this.piece = new BubbloidJ5(initialRow, initialColumn, this, GameGrid.b7);
                    break;
                case 8:
                    this.piece = new BubbloidRectangle6(initialRow, initialColumn, this, GameGrid.b8);
                    break;
                case 9:
                    this.piece = new BubbloidVDiscon2(initialRow, initialColumn, this, GameGrid.b9);
                    break;
            }

            System.Diagnostics.Debug.WriteLine("New piece created.");
        });
    }

    /// <summary>
    /// Sets the cell size used to render each bubble and refreshes the visual grid accordingly.
    /// </summary>
    public void SetCellSize(int newCellSize)
    {
        cellSize = newCellSize;              // Update the global cell size
        UpdateVisualGridFromBoard();         // Re-render the board with the new cell size
    }

    /// <summary>
    /// Redraws the entire grid panel based on the current state of the logical board.
    /// </summary>
    public void UpdateVisualGridFromBoard()
    {
        // If gridPanel hasn't been initialized yet, create it
        if (gridPanel == null)
        {
            InitializeGrid();
        }

        // Remove all current UI children (bubbles and cells)
        gridPanel.Children.Clear();

        // Iterate through the logical board and render each cell visually
        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                var bubble = board[row, col];

                if (bubble != null) // A bubble exists at this location
                {
                    // Set bubble size
                    bubble.Width = cellSize;
                    bubble.Height = cellSize;

                    // Position bubble in grid layout
                    Grid.SetRow(bubble, row);
                    Grid.SetColumn(bubble, col);

                    // Remove from previous grid if it exists to prevent visual duplication
                    if (gridPanelPrevious != null)
                    {
                        gridPanelPrevious.Children.Remove(bubble);
                    }

                    // Add bubble to the grid panel
                    gridPanel.Children.Add(bubble);
                }
                else
                {
                    // Optional: Render transparent placeholders with light borders for empty cells
                    var emptyCell = new Border
                    {
                        Background = Brushes.Transparent,
                        Width = cellSize,
                        Height = cellSize,
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Avalonia.Thickness(1)
                    };
                    Grid.SetRow(emptyCell, row);
                    Grid.SetColumn(emptyCell, col);

                    // Uncomment this line if you want to show all empty cells with borders
                    // gridPanel.Children.Add(emptyCell);
                }
            }
        }

        // Add the active falling piece to the grid so it appears above the static board
        if (piece != null)
        {
            System.Diagnostics.Debug.WriteLine("GameGrid: UpdateVisualGridFromBoard(): piece found");
            this.ActivePieceAdd();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("GameGrid: UpdateVisualGridFromBoard(): piece is null.");
        }
    }

    /// <summary>
    /// Moves the visual representation of the active piece by applying a render transform.
    /// (Currently unused or for experimental effects.)
    /// </summary>
    public void MoveActivePiece()
    {
        System.Diagnostics.Debug.WriteLine("GameGrid: ActivePieceAdd()");

        foreach (var bubble in piece.Bubbles)
        {
            System.Diagnostics.Debug.WriteLine($"Bubble at Row: {bubble.row}, Column: {bubble.col}");
            System.Diagnostics.Debug.WriteLine($"cellSize: {cellSize}");

            int bubble_col = bubble.col;
            int bubble_row = bubble.row;

            // Applies a translation effect to the bubble (visually shifts it on screen).
            // This is *not* a logical move; it just renders an offset.
            bubble.RenderTransform = new TranslateTransform(cellSize, cellSize);

            // Optional fallback to manual canvas repositioning (commented out)
            // bubble.UpdatePosition(bubble.row, bubble.col);
        }
    }

    /// <summary>
    /// Refreshes the visual representation of the active piece by first removing it,
    /// then adding it again. This ensures it's properly updated on screen.
    /// </summary>
    public void ActivePieceUpdate()
    {
        if (piece != null)
        {  
            ActivePieceRemove(); // Remove existing visuals
            ActivePieceAdd();    // Re-add updated visuals
            // piece.IsMoving = true; // (Optional future use)
        }
    }

    /// <summary>
    /// Adds the active piece's bubbles to the grid panel for display.
    /// Ensures proper sizing and grid positioning.
    /// </summary>
    public void ActivePieceAdd()
    {
        if (piece != null && gridPanel != null)
        {
            foreach (var bubble in piece.Bubbles)
            {
                bubble.Width = cellSize;
                bubble.Height = cellSize;

                // Assign bubble to its logical grid cell
                Grid.SetRow(bubble, bubble.row);
                Grid.SetColumn(bubble, bubble.col);

                // Remove it first in case it already exists to prevent duplicates
                gridPanel.Children.Remove(bubble);

                // Add it to the visual grid
                gridPanel.Children.Add(bubble);
            }
        }
    }

    /// <summary>
    /// Removes the active piece's bubbles from the visual grid.
    /// Used when redrawing or updating the piece.
    /// </summary>
    public void ActivePieceRemove()
    {
        if (piece != null && gridPanel != null)
        {
            foreach (var bubble in piece.Bubbles)
            {
                // Simply remove each bubble from the UI
                gridPanel.Children.Remove(bubble);
            }
        }
    }

    /// <summary>
    /// Adds a bubble to the logical board and updates the visual grid accordingly.
    /// </summary>
    public void AddBubble(int row, int col, Bubble bubble)
    {
        // Only add if within valid board bounds
        if (row >= 0 && row < gridHeight && col >= 0 && col < gridWidth)
        {
            board[row, col] = bubble;             // Place bubble in logical array
            UpdateVisualGridFromBoard();          // Sync visual state
        }
    }

    /// <summary>
    /// Removes a bubble from the logical board and updates the visual grid.
    /// </summary>
    public void RemoveBubble(int row, int col)
    {
        if (row >= 0 && row < gridHeight && col >= 0 && col < gridWidth)
        {
            board[row, col] = null;               // Clear cell from logic
            UpdateVisualGridFromBoard();          // Sync visual state
        }
    }

    /// <summary>
    /// External handler for directional input. Passes direction to movement logic.
    /// </summary>
    public void HandleMove(Direction direction)
    {
        // Delegates actual movement logic
        MovePiece(direction);
    }

    /// <summary>
    /// Moves the active piece in the given direction if one exists.
    /// </summary>
    public void MovePiece(Direction direction)
    {
        if (piece != null)
        {
            piece.Move(direction); // Move the piece logically (visuals will sync later)
        }
    }

    /// <summary>
    /// Checks if a cell at the given row and column is within bounds and empty.
    /// Used for collision and boundary checking.
    /// </summary>
    public bool IsValidAndEmpty(int row, int col)
    {
        // Ensure row and col are within board dimensions
        if (row < 0 || row >= gridHeight || col < 0 || col >= gridWidth)
            return false;

        // Return false if cell is already occupied
        if (board[row, col] != null)
            return false;

        return true; // Valid and empty
    }

    /// <summary>
    /// Toggles the game's paused state by stopping or starting the timer.
    /// </summary>
    public void Pause()
    {
        System.Diagnostics.Debug.WriteLine($"GameGrid: Pause() direction: timer.Enabled:{timer.Enabled}");

        if (timer.Enabled)
        {
            this.isPaused = true;
            timer.Stop(); // Pauses game loop
        }
        else
        {
            this.isPaused = false;
            timer.Start(); // Resumes game loop
        }
    }

    /// <summary>
    /// Called when the player presses a rotate key.
    /// Enforces a cooldown between rotations and delegates to rotation logic.
    /// </summary>
    public void HandleRotate(int clockDirection)
    {
        if (DateTime.Now - lastRotationTime < rotationCooldown) return;

        lastRotationTime = DateTime.Now;

        RotatePiece(clockDirection); // Clockwise or counter-clockwise rotation
    }

    /// <summary>
    /// Rotates the current piece (if present) using the given direction value.
    /// </summary>
    public void RotatePiece(int clockDirection)
    {
        piece.Rotate(clockDirection); // Delegate to piece's internal rotation logic
    }

    /// <summary>
    /// Instantly drops the active piece in the direction of gravity/orientation
    /// until it can no longer move. Then locks it into place and updates score.
    /// </summary>
    public void HardDrop() 
    {
        int cellsDropped = 0; // Tracks how many cells the piece moved

        // Determine direction to drop based on current orientation
        switch (currentOrientation) 
        {
            case Orientation.DOWN:
                while (piece != null && piece.CanMove(Direction.DOWN)) 
                {
                    piece.Move(Direction.DOWN);
                    cellsDropped++;
                    //checkPowerUpCollection(); // Placeholder for future feature
                }
                break;

            case Orientation.RIGHT:
                while (piece != null && piece.CanMove(Direction.RIGHT)) 
                {
                    piece.Move(Direction.RIGHT);
                    cellsDropped++;
                    //checkPowerUpCollection();
                }
                break;

            case Orientation.UP:
                while (piece != null && piece.CanMove(Direction.UP)) 
                {
                    piece.Move(Direction.UP);
                    cellsDropped++;
                    //checkPowerUpCollection();
                }
                break;

            case Orientation.LEFT:
                while (piece != null && piece.CanMove(Direction.LEFT)) 
                {
                    piece.Move(Direction.LEFT);
                    cellsDropped++;
                    //checkPowerUpCollection();
                }
                break;
        }

        // Award points for hard drop distance
        Score += cellsDropped * 2; // Example: 2 points per cell dropped instantly

        LockPiece(); // Commit the piece to the board
        UpdateVisualGridFromBoard(); // Redraw the board
    }

    /// <summary>
    /// Updates the score based on how many lines were cleared and handles leveling up.
    /// </summary>
    private void UpdateScore(int clearedLines)
    {
        // Scoring logic: higher reward for clearing multiple lines at once
        switch (clearedLines)
        {
            case 1: Score += 100 * Level; break;
            case 2: Score += 300 * Level; break;
            case 3: Score += 500 * Level; break;
            case 4: Score += 800 * Level; break;
        }

        // Increase level every 1000 points
        Level = (Score / 1000) + 1;

        // Maintain total line-clear count
        LinesCleared += clearedLines;

        // Print score details to console (debug/info purposes)
        Console.WriteLine($"Score: {Score}, Level: {Level}, Lines Cleared: {LinesCleared}");

        // Request redraw of this control (useful if you display score/level visually)
        this.InvalidateVisual();
    }

}
