using MatchPuzzle.Infrastructure.Data;
using MatchPuzzle.Infrastructure.Services;
using UnityEngine;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Interface for camera view component.
    /// </summary>
    public interface ICameraView
    {
        Camera Camera { get; }
        GameObject CameraGameObject { get; }
        Transform CameraTransform { get; }

        /// <summary>
        /// Initializes the camera view with the provided settings and grid data.
        /// </summary>
        void Initialize(CameraSettings cameraSettings);

        // Simple view API: presenters set orthographic size and camera position.
        void SetOrthographicSize(float size);
        void SetPosition(Vector3 position);
    }
}
