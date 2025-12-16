namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Represents a block movement (gravity)
    /// </summary>
    public struct BlockMove
    {
        public GridPosition From;
        public GridPosition To;

        public BlockMove(GridPosition from, GridPosition to)
        {
            From = from;
            To = to;
        }
    }
}