using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    public class CameraPresenter : ICameraPresenter
    {
        private const float MIN_THRESHOLD = 0.001f;

        private readonly ICameraView _cameraView;
        private readonly CameraSettings _cameraSettings;
        private readonly IGridDataProvider _gridDataProvider;
        private readonly ILoggerService _logger;

        public CameraPresenter(
            ICameraView cameraView,
            CameraSettings cameraSettings,
            IGridDataProvider gridDataProvider,
            ILoggerService logger
        )
        {
            _cameraView = cameraView;
            _cameraSettings = cameraSettings;
            _gridDataProvider = gridDataProvider;
            _logger = logger;
        }

        public void Initialize()
        {
            _cameraView.Initialize(_cameraSettings);
        }

        public void AdjustCameraForGrid(
            int rows,
            int columns,
            float cellSize,
            float gridOffsetY
        )
        {
            var camera = _cameraView.Camera;
            if (!camera)
            {
                _logger?.LogError("[CameraPresenter] Camera is missing on view");
                return;
            }

            // Clamp viewport rect and anchor values to [0,1]
            var viewportRect01 = ClampRect01(_cameraSettings.GridViewportRect01);
            var anchorY01 = Mathf.Clamp01(_cameraSettings.GroundAnchorViewportY01);

            // Calculate grid dimensions in world units
            var gridWidthWorldUnits = columns * cellSize;
            var gridHeightWorldUnits = rows * cellSize;

            // Calculate grid bounds in world units
            var gridOffsetX = _gridDataProvider.GridOffset.x;
            var gridCenterX = gridOffsetX + (gridWidthWorldUnits / 2f);
            var gridBottomWorldUnits = gridOffsetY;
            var gridTopWorldUnits = gridOffsetY + gridHeightWorldUnits;

            // Ensure rect dimensions are above minimum threshold
            var rectWidth01 = Mathf.Max(MIN_THRESHOLD, viewportRect01.width);
            var rectHeight01 = Mathf.Max(MIN_THRESHOLD, viewportRect01.height);

            // Calculate camera size needed to fit the grid within the viewport rect
            var cameraSizeFromGridHeight = (gridHeightWorldUnits / 2f) / rectHeight01;
            var cameraSizeFromGridWidth = (gridWidthWorldUnits / 2f) / (rectWidth01 * camera.aspect);

            // Calculate size needed to keep the top of the grid visible
            var topDenom01 = Mathf.Max(MIN_THRESHOLD, viewportRect01.yMax - anchorY01);
            
            // Size needed to keep the top of the grid visible
            // orthographicSize >= gridTopWorldUnits / (2 * (yMax - anchorY))
            var sizeFromTop = Mathf.Max(0f, (gridTopWorldUnits / 2f) / topDenom01);

            // Determine the final orthographic size
            var orthographicCameraSize = Mathf.Max(cameraSizeFromGridHeight, cameraSizeFromGridWidth, sizeFromTop);

            if (viewportRect01.yMin > anchorY01)
            {
                // Calculate size needed to keep the bottom of the grid visible
                var bottomDenom = Mathf.Max(MIN_THRESHOLD, viewportRect01.yMin - anchorY01);
                var maxSizeFromBottom = gridBottomWorldUnits / (2f * bottomDenom);

                if (maxSizeFromBottom > 0f && orthographicCameraSize > maxSizeFromBottom)
                {
                    if (maxSizeFromBottom < sizeFromTop)
                    {
                        _logger?.LogWarning("[CameraPresenter] Grid rect + ground anchor are conflicting. Keeping top inside rect; bottom may be below rect.");
                    }
                    else
                    {
                        orthographicCameraSize = maxSizeFromBottom;
                    }
                }
            }

            _cameraView.SetOrthographicSize(orthographicCameraSize);

            // Calculate camera position to center the grid within the viewport rect
            var cameraHeight = orthographicCameraSize * 2f;
            var cameraWidth = cameraHeight * camera.aspect;
            var rectCenterX = viewportRect01.center.x;
            var cameraX = gridCenterX + (rectCenterX - 0.5f) * cameraWidth;
            var cameraY = orthographicCameraSize * (1f - 2f * anchorY01);

            _cameraView.SetPosition(new Vector3(cameraX, cameraY, _cameraView.Camera.transform.position.z));
            _logger?.LogInformation($"[CameraPresenter] Camera adjusted for grid {rows}x{columns} - OrthographicSize: {orthographicCameraSize:F2}");
        }

        private Rect ClampRect01(Rect rect)
        {
            var xMin = Mathf.Clamp01(rect.xMin);
            var yMin = Mathf.Clamp01(rect.yMin);
            var xMax = Mathf.Clamp(rect.xMax, xMin + MIN_THRESHOLD, 1f);
            var yMax = Mathf.Clamp(rect.yMax, yMin + MIN_THRESHOLD, 1f);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
    }
}