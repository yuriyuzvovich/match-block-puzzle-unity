using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Service for managing the main game camera.
    /// </summary>
    public interface ICameraService
    {
        Camera MainCamera { get; }
        Transform CameraTransform { get; }

        /// <summary>
        /// Gets the configured viewport Y position where the ground should appear (0 = bottom, 1 = top).
        /// </summary>
        float GroundAnchorViewportY01 { get; }

        /// <summary>
        /// Initializes and creates the camera using the provided settings.
        /// </summary>
        UniTask InitializeAsync();

        /// <summary>
        /// Adjusts camera orthographic size based on grid dimensions.
        /// </summary>
        /// <param name="rows">Number of rows in the grid.</param>
        /// <param name="columns">Number of columns in the grid.</param>
        /// <param name="cellSize">Size of each cell in world units.</param>
        void AdjustCameraForGrid(int rows, int columns, float cellSize);

        /// <summary>
        /// Converts screen position to world position.
        /// </summary>
        Vector3 ScreenToWorldPoint(Vector2 screenPosition);

        /// <summary>
        /// Converts a viewport Y (0-1) to world Y based on the current camera position/size.
        /// </summary>
        float GetWorldYAtViewportY01(float viewportY01);
    }
}
