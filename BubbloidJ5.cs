using System.Drawing;

namespace BubbleSwerve
{
    /// <summary>
    /// Represents a specific Bubbloid shape: J5.
    /// This shape resembles a vertical bar with a single block extending leftward at the base — like a "J".
    /// </summary>
    public class BubbloidJ5 : AbstractBubbloid
    {
        // Row offsets from the initial row.
        // Creates a vertical line: rows -1, 0, 1, 2
        // And a fifth bubble at (2, -1) forming the base hook to the left.
        private static readonly int[] RowOffset = { -1, 0, 1, 2, 2, 9 };

        // Column offsets from the initial column.
        // All main column positions are the same (0), except the bottom hook at -1
        private static readonly int[] ColOffset = {  0, 0, 0, 0, -1, 9 };

        // Alternate layout (commented out) would create an L-shape facing right instead.
        //private static readonly int[] RowOffset = { 0, 1, 1, 1, 1 };
        //private static readonly int[] ColOffset = { 0, 0, 1, 2, 3 };

        /// <summary>
        /// Constructor defines shape offsets and passes them to the base class.
        /// </summary>
        public BubbloidJ5(int initialRow, int initialColumn, GameGrid grid, Avalonia.Media.Color color) 
            : base(initialRow, initialColumn, grid, color, RowOffset, ColOffset) 
        {
            // Shape is constructed via offsets — no extra logic required
        }
    }
}
