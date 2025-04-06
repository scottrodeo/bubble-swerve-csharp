using System.Drawing;

namespace BubbleSwerve
{
    /// <summary>
    /// Represents a specific Bubbloid shape: V3.
    /// This shape forms a small "V" with three connected bubbles:
    /// two side-by-side on the top row, and one below the center bubble.
    /// </summary>
    public class BubbloidV3 : AbstractBubbloid
    {
        // Row offsets from the initial row:
        // Two bubbles on the same row (0), one on the row below (+1)
        private static readonly int[] RowOffset = { 0, 0, 1, 9, 9, 9 };

        // Column offsets from the initial column:
        // Bubbles at columns -1 and 0 (top row), and 0 (bottom row)
        private static readonly int[] ColOffset = { -1, 0, 0, 9, 9, 9 };

        // Alternate test offsets (commented out) — unrelated to this shape
        //private static readonly int[] RowOffset = { 0, 1, 1, 1, 1 };
        //private static readonly int[] ColOffset = { 0, 0, 1, 2, 3 };

        /// <summary>
        /// Constructs the V3 shape by providing the predefined offsets to the base class.
        /// </summary>
        public BubbloidV3(int initialRow, int initialColumn, GameGrid grid, Avalonia.Media.Color color) 
            : base(initialRow, initialColumn, grid, color, RowOffset, ColOffset) 
        {
            // Shape defined via offset arrays — base class handles the rest
        }
    }
}
