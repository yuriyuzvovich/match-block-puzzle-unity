using System;
using UnityEngine;
using MatchPuzzle.Infrastructure.Data;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Input data from a swipe gesture
    /// </summary>
    public struct SwipeData
    {
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        public Vector2 Delta => EndPosition - StartPosition;
        public float Distance => Delta.magnitude;
    }

    /// <summary>
    /// Service for handling input (abstracts Unity Input System)
    /// </summary>
    public interface IInputService
    {
        event Action<SwipeData> OnSwipe;
        event Action<Vector2> OnTap;

        /// <summary>
        /// Current input configuration used by the service.
        /// </summary>
        PlayerInputSettings Settings { get; }

        void Enable();
        void Disable();

        Vector2 GetPointerPosition();

        void DoFrameTick();
    }
}
