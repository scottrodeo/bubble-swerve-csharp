/// <summary>
/// Represents possible movement directions for bubbles and shapes in the game.
/// Used for translation, collision detection, and rotation logic.
/// </summary>
public enum Direction
{
    LEFT,   // Move or check one column to the left (col - 1)
    RIGHT,  // Move or check one column to the right (col + 1)
    DOWN,   // Move or check one row down (row + 1)
    UP      // Move or check one row up (row - 1)
}
