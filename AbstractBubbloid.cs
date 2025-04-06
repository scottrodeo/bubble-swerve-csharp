using System;
using System.Collections.Generic;
using System.Drawing;
using Avalonia.Controls;

namespace BubbleSwerve
{
    /// <summary>
    /// Abstract base class for Bubbloid shapes in the game.
    /// Defines common behaviors and properties of all Bubbloids.
    /// </summary>
    public abstract class AbstractBubbloid
    {
        // List of current Bubble objects making up this Bubbloid
        public List<Bubble> Bubbles { get; private set; }

        // List of Bubbles from the previous frame or state
        public List<Bubble> BubblesLast { get; private set; }

        // Reference to the game grid for position validation
        protected GameGrid _gameGrid { get; private set; }

        // Whether the Bubbloid is actively moving
        public bool IsMoving;

        /// <summary>
        /// Constructor to initialize a new Bubbloid.
        /// </summary>
        protected AbstractBubbloid(int initialRow, int initialColumn, GameGrid gameGrid, Avalonia.Media.Color color, int[] rowOffset, int[] colOffset)
        {
            _gameGrid = gameGrid;
            Bubbles = new List<Bubble>();
            BubblesLast = new List<Bubble>();

            // Create the bubble shapes based on the provided offsets
            CreateBubbles(initialRow, initialColumn, color, rowOffset, colOffset);
            this.IsMoving = false;
        }

        /// <summary>
        /// Create the bubbles based on pivot and offset arrays.
        /// </summary>
        private void CreateBubbles(int initialRow, int initialColumn, Avalonia.Media.Color color, int[] rowOffset, int[] colOffset)
        {
            System.Diagnostics.Debug.WriteLine($"CreateBubbles() {rowOffset} {colOffset}");

            for (int i = 0; i < rowOffset.Length; i++)
            {
                if (rowOffset[i] != 9) // Special value 9 possibly indicates "do not place a bubble"
                {
                    System.Diagnostics.Debug.WriteLine($"new Bubble() {_gameGrid}, {initialRow} + {rowOffset[i]}, {initialColumn} + {colOffset[i]}, {color}, {true}");
                    Bubbles.Add(new Bubble(_gameGrid, initialRow + rowOffset[i], initialColumn + colOffset[i], color, true));
                }
            }
        }

        /// <summary>
        /// Attempts to move the Bubbloid in a specified direction.
        /// </summary>
        public void Move(Direction direction)
        {
            if (CanMove(direction))
            {
                foreach (var bubble in Bubbles)
                {
                    bubble.Move(direction); // Move each bubble
                }
            }
            else if (direction == Direction.DOWN)
            {
                // Could trigger lock-in behavior here if needed
                //AbleToMove = false;
            }
        }

        /// <summary>
        /// Check if movement is possible in the given direction.
        /// </summary>
        public bool CanMove(Direction direction)
        {
            foreach (var bubble in Bubbles)
            {
                int newRow = bubble.row;
                int newCol = bubble.col;

                // Adjust coordinates based on direction
                switch (direction)
                {
                    case Direction.DOWN:  newRow++; break;
                    case Direction.UP:    newRow--; break;
                    case Direction.LEFT:  newCol--; break;
                    case Direction.RIGHT: newCol++; break;
                }

                // If any destination is blocked, the whole shape can’t move
                if (!_gameGrid.IsValidAndEmpty(newRow, newCol))
                {
                    return false;
                }
            }

            return true; // All positions are valid
        }

        /// <summary>
        /// Attempts to rotate the Bubbloid clockwise (0) or counter-clockwise (1).
        /// </summary>
        public void Rotate(int clockDirection)
        {
            System.Diagnostics.Debug.WriteLine($"Rotate called: IsMoving = {this.IsMoving}");

            if (this.IsMoving)
                return; // Prevent rotation if already moving

            this.IsMoving = true;

            // Get proposed new coordinates from rotation
            int[,] coords = GetRotatedCoords(clockDirection);

            if (CanRotate(coords))
            {
                int i = 0;
                foreach (var bubble in Bubbles)
                {
                    bubble.Rotate(coords[i, 0], coords[i, 1]);
                    System.Diagnostics.Debug.WriteLine($"AbstractBubbloid: Rotate(): x:{i}:{coords[i, 0]}, y:{i}:{coords[i, 1]}");
                    i++;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Rotate called: cant rotate");
            }

            this.IsMoving = false;
        }

        /// <summary>
        /// Calculates rotated coordinates around a pivot (usually Bubbles[1]).
        /// </summary>
        public int[,] GetRotatedCoords(int clockDirection)
        {
            var pivot = Bubbles[1]; // Middle piece is the pivot
            int originX = pivot.col;
            int originY = pivot.row;

            int[,] rotatedCoords = new int[Bubbles.Count, 2];

            int i = 0;
            foreach (var bubble in Bubbles)
            {
                // Translate to origin
                int squareCartX = bubble.col - originX;
                int squareCartY = bubble.row - originY;

                int newX = 0, newY = 0;

                // Rotate clockwise or counter-clockwise
                if (clockDirection == 0) // Clockwise
                {
                    newX = originX - squareCartY;
                    newY = originY + squareCartX;
                }
                else if (clockDirection == 1) // Counter-clockwise
                {
                    newX = originX + squareCartY;
                    newY = originY - squareCartX;
                }

                rotatedCoords[i, 0] = newX;
                rotatedCoords[i, 1] = newY;
                i++;
            }

            return rotatedCoords;
        }

        /// <summary>
        /// Checks whether the rotated coordinates are valid and unblocked.
        /// </summary>
        public bool CanRotate(int[,] coords)
        {
            if (coords.GetLength(0) != Bubbles.Count)
                throw new ArgumentException("Mismatch between coordinates and bubbles.");

            int i = 0;
            foreach (var bubble in Bubbles)
            {
                int x = coords[i, 0];
                int y = coords[i, 1];

                // Check bounds
                if (x < 0 || x >= _gameGrid.gridWidth || y < 0 || y >= _gameGrid.gridHeight)
                {
                    return false;
                }

                // Check if space is occupied
                if (!_gameGrid.IsValidAndEmpty(y, x))
                {
                    return false;
                }

                i++;
            }

            return true; // All positions are valid
        }
    }
}
