using System.Drawing;

namespace BubbleSwerve
{
    /// <summary>
    /// Represents a specific Bubbloid shape: VDiscon2.
    /// This is a disconnected V-shape composed of two bubbles:
    /// one on the top left, and one below to the right.
    /// </summary>
    public class BubbloidVDiscon2 : AbstractBubbloid
    {
        // Row offsets from the initial row:
        // Bubble 0 at row 0
        // Bubble 2 at row +1
        // Remaining entries are ignored (value 9)
        private static readonly int[] RowOffset = { 0, 9, 1, 9, 9, 9 };

        // Column offsets from the initial column:
        // Bubble 0 at column -1
        // Bubble 2 at column 0
        private static readonly int[] ColOffset = { -1, 9, 0, 9, 9, 9 };

        // Alt shape (commented out) was likely from earlier experiments
        //private static readonly int[] RowOffset = { 0, 1, 1, 1, 1 };
        //private static readonly int[] ColOffset = { 0, 0, 1, 2, 3 };

        /// <summary>
        /// Constructs the VDiscon2 bubbloid using preset offsets.
        /// </summary>
        public BubbloidVDiscon2(int initialRow, int initialColumn, GameGrid grid, Avalonia.Media.Color color) 
            : base(initialRow, initialColumn, grid, color, RowOffset, ColOffset) 
        {
            // Disconnected two-bubble piece defined by sparse offsets
        }
    }
}
