using System.Collections.Generic;

namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Represents the game grid.
    /// Row 0 is at the bottom, Column 0 is at the left.
    /// </summary>
    public class Grid
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }

        private Block[,] _cells; // [row, column]

        public Grid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            _cells = new Block[rows, columns];
        }

        /// <summary>
        /// Gets the block at the specified position
        /// </summary>
        public Block GetBlock(GridPosition position)
        {
            if (!IsValidPosition(position))
                return null;

            return _cells[position.Row, position.Column];
        }

        /// <summary>
        /// Sets a block at the specified position
        /// </summary>
        public void SetBlock(GridPosition position, Block block)
        {
            if (!IsValidPosition(position))
                return;

            _cells[position.Row, position.Column] = block;

            if (block != null)
            {
                block.Position = position;
            }
        }

        /// <summary>
        /// Removes the block at the specified position
        /// </summary>
        public void RemoveBlock(GridPosition position)
        {
            SetBlock(position, null);
        }

        /// <summary>
        /// Checks if the position is within grid bounds
        /// </summary>
        public bool IsValidPosition(GridPosition position)
        {
            return position.Row >= 0 && position.Row < Rows &&
                position.Column >= 0 && position.Column < Columns;
        }

        /// <summary>
        /// Checks if the position is empty (no block)
        /// </summary>
        public bool IsEmpty(GridPosition position)
        {
            return IsValidPosition(position) && GetBlock(position) == null;
        }

        /// <summary>
        /// Gets all blocks in the grid (non-null)
        /// </summary>
        public List<Block> GetAllBlocks()
        {
            var blocks = new List<Block>();

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    var block = _cells[row, col];
                    if (block != null)
                    {
                        blocks.Add(block);
                    }
                }
            }

            return blocks;
        }

        /// <summary>
        /// Swaps two blocks
        /// </summary>
        public void SwapBlocks(GridPosition posA, GridPosition posB)
        {
            var blockA = GetBlock(posA);
            var blockB = GetBlock(posB);

            SetBlock(posA, blockB);
            SetBlock(posB, blockA);
        }

        /// <summary>
        /// Gets the neighbor position in a direction
        /// </summary>
        public GridPosition GetNeighbor(GridPosition position, Direction direction)
        {
            return direction switch {
                Direction.Up => position.Up(),
                Direction.Down => position.Down(),
                Direction.Left => position.Left(),
                Direction.Right => position.Right(),
                _ => position
            };
        }

        /// <summary>
        /// Gets all valid neighbors (4-way)
        /// </summary>
        public List<GridPosition> GetNeighbors(GridPosition position)
        {
            var neighbors = new List<GridPosition>();

            var directions = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

            foreach (var dir in directions)
            {
                var neighbor = GetNeighbor(position, dir);
                if (IsValidPosition(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Clears the entire grid
        /// </summary>
        public void Clear()
        {
            _cells = new Block[Rows, Columns];
        }

        /// <summary>
        /// Checks if grid is empty
        /// </summary>
        public bool IsGridEmpty()
        {
            return GetAllBlocks().Count == 0;
        }
    }
}