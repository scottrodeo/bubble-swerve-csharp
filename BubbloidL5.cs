using System.Drawing;

namespace BubbleSwerve
{
    /// <summary>
    /// Represents a specific Bubbloid shape: L5.
    /// This shape forms a vertical line of 4 bubbles with a rightward "foot" at the bottom — like an "L".
    /// </summary>
    public class BubbloidL5 : AbstractBubbloid
    {
        // Row offsets for bubble positions relative to the initial row.
        // Bubbles placed at -1, 0, 1, and 2 (vertical stem),
        // with an additional bubble at (2, 1) forming the bottom-right foot.
        private static readonly int[] RowOffset = { -1, 0, 1, 2, 2, 9 };

        // Column offsets for the shape.
        // First four bubbles aligned vertically in the same column (0),
        // fifth bubble offsets to the right at the base (column +1).
        private static readonly int[] ColOffset = {  0, 0, 0, 0, 1, 9 };

        // Alternate version (commented out) creates a flat L shape rotated 90°
        //private static readonly int[] RowOffset = { 0, 1, 1, 1, 1 };
        //private static readonly int[] ColOffset = { 0, 0, 1, 2, 3 };

        /// <summary>
        /// Constructs the L-shaped Bubbloid using the predefined offsets.
        /// </summary>
        public BubbloidL5(int initialRow, int initialColumn, GameGrid grid, Avalonia.Media.Color color) 
            : base(initialRow, initialColumn, grid, color, RowOffset, ColOffset) 
        {
            // L5 piece is now ready, built via inherited base logic
        }
    }
}
