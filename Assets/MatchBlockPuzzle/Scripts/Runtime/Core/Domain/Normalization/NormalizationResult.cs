using System.Collections.Generic;

namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Result of a normalization step
    /// </summary>
    public class NormalizationResult
    {
        public List<BlockMove> Moves { get; set; } = new List<BlockMove>();
        public List<HashSet<GridPosition>> MatchedAreas { get; set; } = new List<HashSet<GridPosition>>();
        public bool HasChanges => Moves.Count > 0 || MatchedAreas.Count > 0;
    }
}