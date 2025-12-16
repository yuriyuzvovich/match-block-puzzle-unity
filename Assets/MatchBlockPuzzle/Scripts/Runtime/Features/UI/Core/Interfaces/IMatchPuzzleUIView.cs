using UnityEngine;

namespace MatchPuzzle.Features.UI
{
    /// <summary>
    /// Interface for the UI elements in the match puzzle game
    /// </summary>
    public interface IMatchPuzzleUIView
    {
        Canvas Canvas { get; }

        event System.Action RestartClicked;
        event System.Action NextClicked;

        void Show();
        void Hide();
    }
}