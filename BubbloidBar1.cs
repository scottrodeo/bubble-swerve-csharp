using System.Drawing;

namespace BubbleSwerve
{
    /// <summary>
    /// Represents a specific Bubbloid shape: Bar1.
    /// In this case, it's a single block (1x1) with the rest of the offsets set to 9 (ignored).
    /// </summary>
    public class BubbloidBar1 : AbstractBubbloid
    {
        // Defines the relative vertical offsets from the initial row
        private static readonly int[] RowOffset = { 0, 9, 9, 9, 9, 9 };

        // Defines the relative horizontal offsets from the initial column
        private static readonly int[] ColOffset = { 0, 9, 9, 9, 9, 9 };

        /// <summary>
        /// Constructor passes shape parameters to the abstract base class.
        /// </summary>
        public BubbloidBar1(int initialRow, int initialColumn, GameGrid grid, Avalonia.Media.Color color) 
            : base(initialRow, initialColumn, grid, color, RowOffset, ColOffset) 
        { 
            // No additional logic needed — shape is defined by the offsets
        }
    }
}
