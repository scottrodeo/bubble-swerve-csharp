using System.Drawing;

namespace BubbleSwerve
{
    /// <summary>
    /// Represents a specific Bubbloid shape: Cross5.
    /// This shape forms a plus (+) or cross with 5 bubbles:
    /// one center, one to each cardinal direction.
    /// </summary>
    public class BubbloidCross5 : AbstractBubbloid
    {
        // Vertical offsets from the initial row.
        // Bubbles placed at:
        // - center (0)
        // - left/right on same row (0)
        // - one above (-1) and one below (+1)
        private static readonly int[] RowOffset = { 0, 0, 0, 1, -1, 9 };

        // Horizontal offsets from the initial column.
        // Bubbles at center (0), left (-1), right (+1), and one directly above/below (0)
        private static readonly int[] ColOffset = { -1, 0, 1, 0, 0, 9 };

        /// <summary>
        /// Constructor passes shape definition to the abstract base class.
        /// </summary>
        public BubbloidCross5(int initialRow, int initialColumn, GameGrid grid, Avalonia.Media.Color color) 
            : base(initialRow, initialColumn, grid, color, RowOffset, ColOffset) 
        {
            // Cross shape with 5 connected segments defined by offset arrays
        }
    }
}
