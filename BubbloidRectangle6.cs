using System.Drawing;

namespace BubbleSwerve
{
    /// <summary>
    /// Represents a specific Bubbloid shape: Rectangle6.
    /// This shape forms a solid 3-row by 2-column rectangle made of 6 bubbles.
    /// </summary>
    public class BubbloidRectangle6 : AbstractBubbloid
    {
        // Row offsets relative to the initial row:
        // Forms three vertical positions: -1, 0, 1
        // Each row has one bubble in column 0 and one in column 1
        private static readonly int[] RowOffset = { -1, 0, 1, -1, 0, 1 };

        // Column offsets relative to the initial column:
        // First three bubbles are in column 0, next three are in column 1
        private static readonly int[] ColOffset = {  0, 0, 0,  1, 1, 1 };

        // Alternate test shape (commented out) for potential L-variant experimentation
        //private static readonly int[] RowOffset = { 0, 1, 1, 1, 1 };
        //private static readonly int[] ColOffset = { 0, 0, 1, 2, 3 };

        /// <summary>
        /// Constructor defines the 3x2 rectangle shape using offsets.
        /// </summary>
        public BubbloidRectangle6(int initialRow, int initialColumn, GameGrid grid, Avalonia.Media.Color color) 
            : base(initialRow, initialColumn, grid, color, RowOffset, ColOffset) 
        {
            // Rectangle shape constructed — no additional logic needed
        }
    }
}
