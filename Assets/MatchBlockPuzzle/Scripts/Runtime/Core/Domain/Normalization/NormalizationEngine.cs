using System;
using System.Collections.Generic;

namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Handles grid normalization: gravity and matching
    /// </summary>
    public class NormalizationEngine
    {
        private readonly Grid _grid;
        private readonly MatchFinder _matchFinder;
        private readonly int _minMatchLength;

        public NormalizationEngine(Grid grid, int minMatchLength)
        {
            _grid = grid;
            _minMatchLength = Math.Max(2, minMatchLength);
            _matchFinder = new MatchFinder(grid, _minMatchLength);
        }

        /// <summary>
        /// Performs one complete normalization cycle:
        /// 1. Apply gravity (move hanging blocks down)
        /// 2. Find and mark matches
        /// Returns the result containing moves and matches
        /// </summary>
        public NormalizationResult Normalize()
        {
            var result = new NormalizationResult();

            // Step 1: Apply gravity
            var moves = ApplyGravity();
            result.Moves.AddRange(moves);

            // Step 2: Find matches
            var matchedAreas = _matchFinder.FindMatchAreas();
            result.MatchedAreas.AddRange(matchedAreas);

            return result;
        }

        /// <summary>
        /// Applies gravity: moves all hanging blocks down.
        /// Returns list of moves that were made.
        /// </summary>
        private List<BlockMove> ApplyGravity()
        {
            var moves = new List<BlockMove>();
            bool isChangesOccurred;

            do
            {
                isChangesOccurred = false;

                // Process from bottom to top, left to right
                for (int row = 0; row < _grid.Rows; row++)
                {
                    for (int col = 0; col < _grid.Columns; col++)
                    {
                        var position = new GridPosition(row, col);
                        var block = _grid.GetBlock(position);
                        if (block == null)
                            continue;

                        // Check if block can fall
                        var below = position.Down();

                        if (_grid.IsEmpty(below))
                        {
                            // Block falls
                            _grid.SwapBlocks(position, below);
                            moves.Add(new BlockMove(position, below));
                            isChangesOccurred = true;
                        }
                    }
                }

            }
            while (isChangesOccurred);

            return moves;
        }

        /// <summary>
        /// Destroys blocks at the specified positions
        /// </summary>
        public void DestroyBlocks(List<HashSet<GridPosition>> matchedAreas)
        {
            foreach (var area in matchedAreas)
            {
                foreach (var position in area)
                {
                    _grid.RemoveBlock(position);
                }
            }
        }
    }
}