using System.Drawing;

namespace BubbleSwerve
{
    /// <summary>
    /// Represents a specific Bubbloid shape: Bar2.
    /// This shape forms a horizontal 2-block line.
    /// </summary>
    public class BubbloidBar2 : AbstractBubbloid
    {
        // Vertical offsets for each bubble in the shape.
        // Only first two entries are used (0, 0) — placing two bubbles on the same row.
        private static readonly int[] RowOffset = { 0, 0, 9, 9, 9, 9 };

        // Horizontal offsets for each bubble in the shape.
        // Bubbles are at (0, 0) and (0, -1), forming a horizontal bar 2 units wide.
        private static readonly int[] ColOffset = { 0, -1, 9, 9, 9, 9 };

        /// <summary>
        /// Constructor passes shape parameters to the abstract base class.
        /// </summary>
        public BubbloidBar2(int initialRow, int initialColumn, GameGrid grid, Avalonia.Media.Color color) 
            : base(initialRow, initialColumn, grid, color, RowOffset, ColOffset) 
        { 
            // No extra setup needed — shape defined entirely by offsets.
        }
    }
}
