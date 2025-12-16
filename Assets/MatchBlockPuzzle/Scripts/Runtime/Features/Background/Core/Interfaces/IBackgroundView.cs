using UnityEngine;

namespace MatchPuzzle.Features.Background
{
    /// <summary>
    /// Interface for background view component.
    /// Kept dumb so presenter can drive all logic.
    /// </summary>
    public interface IBackgroundView
    {
        SpriteRenderer SpriteRenderer { get; }
        Transform GroundLine { get; }
        Transform RootTransform { get; }

        void SetActive(bool isActive);
        void SetScale(Vector3 scale);
        void SetPosition(Vector3 position);
        void Move(Vector3 delta);
    }
}
