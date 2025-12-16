using System;

namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Represents a position in the grid.
    /// Row 0 is at the bottom, Column 0 is at the left.
    /// </summary>
    [Serializable]
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int Row;
        public int Column;

        public GridPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public bool Equals(GridPosition other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        public static bool operator ==(GridPosition left, GridPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridPosition left, GridPosition right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({Row}, {Column})";
        }

        /// <summary>
        /// Returns the position above this one
        /// </summary>
        public GridPosition Up() => new GridPosition(Row + 1, Column);

        /// <summary>
        /// Returns the position below this one
        /// </summary>
        public GridPosition Down() => new GridPosition(Row - 1, Column);

        /// <summary>
        /// Returns the position to the left
        /// </summary>
        public GridPosition Left() => new GridPosition(Row, Column - 1);

        /// <summary>
        /// Returns the position to the right
        /// </summary>
        public GridPosition Right() => new GridPosition(Row, Column + 1);
    }
}
