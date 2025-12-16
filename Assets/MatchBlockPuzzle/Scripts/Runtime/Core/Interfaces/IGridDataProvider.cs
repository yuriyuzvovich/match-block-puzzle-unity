using UnityEngine;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Provides read-only access to grid configuration values.
    /// Typically backed by a ScriptableObject such as GridSettings.
    /// </summary>
    public interface IGridDataProvider
    {
        /// <summary>
        /// Size of a single grid cell in world units.
        /// </summary>
        float CellSize { get; }

        /// <summary>
        /// World-space offset of the grid origin (bottom-left corner).
        /// </summary>
        Vector2 GridOffset { get; }
    }
}
