using UnityEngine;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Interface for the root view
    /// </summary>
    public interface IMatchPuzzleRoot
    {
        Transform ThisTransform { get; }
        GameObject ThisGameObject { get; }

        void Show();
        void Hide();
        void Cleanup();
    }
}