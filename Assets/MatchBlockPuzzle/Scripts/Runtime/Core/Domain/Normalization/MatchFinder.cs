using System;
using System.Collections.Generic;

namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Finds matches in the grid.
    /// A match is any block that belongs to a horizontal or vertical line of 3+ blocks of the same type.
    /// </summary>
    public class MatchFinder
    {
        private readonly Grid _grid;
        private readonly int _minMatchLength;

        public MatchFinder(Grid grid, int minMatchLength)
        {
            _grid = grid;
            _minMatchLength = Math.Max(2, minMatchLength);
        }

        /// <summary>
        /// Finds all blocks that are part of 3+ lines and groups connected ones.
        /// Only blocks that belong to a valid line are returned.
        /// </summary>
        public List<HashSet<GridPosition>> FindMatchAreas()
        {
            var matchedPositions = new HashSet<GridPosition>();

            AddHorizontalMatches(matchedPositions);
            AddVerticalMatches(matchedPositions);

            return GroupMatchedPositions(matchedPositions);
        }

        /// <summary>
        /// Adds all blocks that form horizontal lines of 3+ of the same type.
        /// </summary>
        private void AddHorizontalMatches(HashSet<GridPosition> matchedPositions)
        {
            for (int row = 0; row < _grid.Rows; row++)
            {
                int col = 0;

                while (col < _grid.Columns)
                {
                    var startPos = new GridPosition(row, col);
                    var startBlock = _grid.GetBlock(startPos);

                    if (startBlock == null)
                    {
                        col++;
                        continue;
                    }

                    var length = 1;
                    var nextCol = col + 1;

                    while (nextCol < _grid.Columns)
                    {
                        var nextBlock = _grid.GetBlock(new GridPosition(row, nextCol));

                        if (nextBlock != null && nextBlock.Type == startBlock.Type)
                        {
                            length++;
                            nextCol++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (length >= _minMatchLength)
                    {
                        for (int c = col; c < col + length; c++)
                        {
                            matchedPositions.Add(new GridPosition(row, c));
                        }
                    }

                    col += length;
                }
            }
        }

        /// <summary>
        /// Adds all blocks that form vertical lines of 3+ of the same type.
        /// </summary>
        private void AddVerticalMatches(HashSet<GridPosition> matchedPositions)
        {
            for (int col = 0; col < _grid.Columns; col++)
            {
                int row = 0;

                while (row < _grid.Rows)
                {
                    var startPos = new GridPosition(row, col);
                    var startBlock = _grid.GetBlock(startPos);

                    if (startBlock == null)
                    {
                        row++;
                        continue;
                    }

                    var length = 1;
                    var nextRow = row + 1;

                    while (nextRow < _grid.Rows)
                    {
                        var nextBlock = _grid.GetBlock(new GridPosition(nextRow, col));

                        if (nextBlock != null && nextBlock.Type == startBlock.Type)
                        {
                            length++;
                            nextRow++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (length >= _minMatchLength)
                    {
                        for (int r = row; r < row + length; r++)
                        {
                            matchedPositions.Add(new GridPosition(r, col));
                        }
                    }

                    row += length;
                }
            }
        }

        /// <summary>
        /// Groups connected matched positions of the same type into areas.
        /// </summary>
        private List<HashSet<GridPosition>> GroupMatchedPositions(HashSet<GridPosition> matchedPositions)
        {
            var groupedAreas = new List<HashSet<GridPosition>>();
            var visited = new HashSet<GridPosition>();

            // For each matched position, perform a flood fill to find all connected blocks of the same type
            foreach (var position in matchedPositions)
            {
                if (visited.Contains(position))
                    continue;

                var startBlock = _grid.GetBlock(position);
                if (startBlock == null)
                    continue;

                var area = new HashSet<GridPosition>();
                var queue = new Queue<GridPosition>();
                queue.Enqueue(position);
                visited.Add(position);

                // Flood fill to find all connected blocks of the same type
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    area.Add(current);

                    foreach (var neighbor in _grid.GetNeighbors(current))
                    {
                        if (visited.Contains(neighbor))
                            continue;

                        if (!matchedPositions.Contains(neighbor))
                            continue;

                        var neighborBlock = _grid.GetBlock(neighbor);
                        if (neighborBlock == null || neighborBlock.Type != startBlock.Type)
                            continue;

                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }

                groupedAreas.Add(area);
            }

            return groupedAreas;
        }
    }
}