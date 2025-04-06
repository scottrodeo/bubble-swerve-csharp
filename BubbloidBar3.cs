using System.Drawing;

namespace BubbleSwerve
{
    /// <summary>
    /// Represents a specific Bubbloid shape: Bar3.
    /// This shape forms a horizontal line of 3 bubbles.
    /// </summary>
    public class BubbloidBar3 : AbstractBubbloid
    {
        // Vertical offsets for the shape.
        // All three active bubbles are on the same row (offset = 0)
        private static readonly int[] RowOffset = { 0, 0, 0, 9, 9, 9 };

        // Horizontal offsets for the shape.
        // Bubbles placed at columns -1, 0, +1 relative to initial column
        private static readonly int[] ColOffset = { -1, 0, 1, 9, 9, 9 };

        /// <summary>
        /// Constructor passes shape parameters to the abstract base class.
        /// </summary>
        public BubbloidBar3(int initialRow, int initialColumn, GameGrid grid, Avalonia.Media.Color color) 
            : base(initialRow, initialColumn, grid, color, RowOffset, ColOffset) 
        {
            // Shape definition handled entirely via offset arrays
        }
    }
}
