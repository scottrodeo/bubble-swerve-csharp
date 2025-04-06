using Avalonia.Controls;
using Avalonia.Media;

/// <summary>
/// Represents a single bubble unit on the game grid.
/// Each bubble has a row/column position, color, and can move or rotate.
/// </summary>
public class Bubble : UserControl
{
    private GameGrid grid; // Reference to the grid the bubble exists in
    public int row, col;   // Current position of the bubble in the grid
    public Color color;    // Visual color of the bubble

    /// <summary>
    /// Constructor to create a new bubble at a given position with a given color.
    /// </summary>
    public Bubble(GameGrid grid, int row, int col, Color c, bool mobile)
    {
        this.grid = grid;
        this.row = row;
        this.col = col;
        this.color = c;
        //this.ableToMove = mobile; // Unused mobility flag

        InitializeBubble(); // Sets up visual styling
    }

    /// <summary>
    /// Initializes the appearance of the bubble (visual rendering).
    /// </summary>
    private void InitializeBubble()
    {
        // Optional fixed sizing (commented out for now)
        // Width = BubbleSwerve.GamePanel.cellsize;
        // Height = BubbleSwerve.GamePanel.cellsize;
        // Margin = new Avalonia.Thickness(2);

        // Create a smooth radial gradient to make the bubble look more spherical
        var radialGradientBrush = new RadialGradientBrush
        {
            GradientStops =
            {
                new GradientStop { Color = Colors.White, Offset = 0.0 },  // Bright center
                new GradientStop { Color = color, Offset = 0.8 },         // Base color
                new GradientStop { Color = Colors.Black, Offset = 1.0 }   // Dark edge
            }
        };

        Background = radialGradientBrush;                 // Apply gradient background
        CornerRadius = new Avalonia.CornerRadius(20);     // Round corners to make it circular
        BorderBrush = Brushes.Black;                      // Black outline
        BorderThickness = new Avalonia.Thickness(1);      // Thin black border
    }

    /// <summary>
    /// Rotates the bubble to a new absolute grid position.
    /// </summary>
    public void Rotate(int x, int y)
    {
        System.Diagnostics.Debug.WriteLine($"Bubble: Rotate(int x, int y): {x}, {y}"); 
        col = x;
        row = y;
    }

    /// <summary>
    /// Updates the bubble's position on the UI canvas based on new row/col.
    /// </summary>
    public void UpdatePosition(int newRow, int newCol)
    {
        Canvas.SetLeft(grid, newCol * grid.cellSize); // Position X
        Canvas.SetTop(grid, newRow * grid.cellSize);  // Position Y
    }

    /// <summary>
    /// Moves the bubble one step in a given direction.
    /// </summary>
    public void Move(Direction direction)
    {
        switch (direction)
        {
            case Direction.DOWN:
                row += 1;
                break;
            case Direction.UP:
                row -= 1;
                break;
            case Direction.RIGHT:
                col += 1;
                break;
            case Direction.LEFT:
                col -= 1;
                break;
        }
    }
}
