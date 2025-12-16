using UnityEngine;

using MatchPuzzle.Core.Interfaces;

namespace MatchPuzzle.Features.Balloon
{
    public interface IBalloonView : IPoolObject
    {
        Transform Transform { get; }
        void SetScale(float scale);
        void SetPosition(Vector3 position);
        event System.Action<float> UpdateTick;
    }
}
