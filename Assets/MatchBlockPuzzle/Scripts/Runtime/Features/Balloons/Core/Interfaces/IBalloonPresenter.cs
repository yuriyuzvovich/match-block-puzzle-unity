using UnityEngine;

namespace MatchPuzzle.Features.Balloon
{
    public interface IBalloonPresenter
    {
        void Initialize(
            Vector3 startPosition,
            float scale,
            float speed,
            float direction,
            float sineAmplitude,
            float sineFrequency
        );

        bool IsOffScreen(Camera camera, float leftMargin, float rightMargin);
        void Reset();
    }
}