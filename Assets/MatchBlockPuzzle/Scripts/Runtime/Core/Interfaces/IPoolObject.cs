using System;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Marker interface for objects that can be returned to a pool and reset.
    /// </summary>
    public interface IPoolObject
    {
        /// <summary>
        /// Reset the object's runtime state before returning to the pool.
        /// </summary>
        void ResetState();
    }
}
