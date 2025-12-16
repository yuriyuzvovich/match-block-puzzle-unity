using System;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;
using MatchPuzzle.Runtime.Presentation;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    /// <inheritdoc/>
    public class CameraService : ICameraService
    {
        private readonly CameraSettings _cameraSettings;
        private readonly IAssetService _assetService;
        private readonly ILoggerService _logger;
        private readonly IGridDataProvider _gridDataProvider;
        private ICameraView _cameraView;
        private ICameraPresenter _cameraPresenter;

        public Camera MainCamera => _cameraView?.Camera ?? null;
        public Transform CameraTransform => _cameraView?.CameraTransform ?? null;
        public float GroundAnchorViewportY01 => _cameraSettings.GroundAnchorViewportY01;

        public CameraService(
            CameraSettings cameraSettings,
            IGridDataProvider gridDataProvider,
            IAssetService assetService,
            ILoggerService logger
        )
        {
            _cameraSettings = cameraSettings;
            _gridDataProvider = gridDataProvider ?? throw new ArgumentNullException(nameof(gridDataProvider));
            _assetService = assetService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async UniTask InitializeAsync()
        {
            if (MainCamera)
            {
                _logger.LogWarning("[CameraService] Camera already initialized.");
                return;
            }

            if (_gridDataProvider == null)
            {
                _logger.LogError("[CameraService] Cannot initialize camera - grid data provider is null.");
                return;
            }

            var cameraPrefab = await _assetService.LoadAsync<GameObject>("Prefabs/CameraView");
            if (!cameraPrefab)
            {
                _logger.LogError("[CameraService] Failed to load CameraView prefab from Prefabs/CameraView");
                return;
            }

            var cameraObject = UnityEngine.Object.Instantiate(cameraPrefab);

            _cameraView = cameraObject.GetComponent<ICameraView>();
            if (_cameraView == null)
            {
                _logger.LogError("[CameraService] CameraView prefab does not have a component implementing ICameraView");
                UnityEngine.Object.Destroy(cameraObject);
                return;
            }

            _cameraPresenter = new CameraPresenter(
                _cameraView,
                _cameraSettings,
                _gridDataProvider,
                _logger
            );
            _cameraPresenter.Initialize();

            _logger.LogInformation("[CameraService] Main camera created and initialized.");
        }

        /// <inheritdoc/>
        public void AdjustCameraForGrid(int rows, int columns, float cellSize)
        {
            if (_cameraView == null || !MainCamera)
            {
                _logger.LogError("[CameraService] Cannot adjust camera - camera not initialized.");
                return;
            }

            if (_gridDataProvider == null)
            {
                _logger.LogError("[CameraService] Cannot adjust camera - grid data provider is missing.");
                return;
            }

            var gridOffsetY = _gridDataProvider.GridOffset.y;

            _cameraPresenter.AdjustCameraForGrid(
                rows,
                columns,
                cellSize,
                gridOffsetY
            );

            _logger.LogInformation($"[CameraService] Camera adjusted for grid {rows}x{columns} - OrthographicSize: {MainCamera.orthographicSize:F2}");
        }

        /// <inheritdoc/>
        public Vector3 ScreenToWorldPoint(Vector2 screenPosition)
        {
            if (!MainCamera)
            {
                _logger.LogError("[CameraService] Cannot convert screen to world - camera not initialized.");
                return Vector3.zero;
            }

            return MainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, MainCamera.nearClipPlane));
        }

        /// <inheritdoc/>
        public float GetWorldYAtViewportY01(float viewportY01)
        {
            if (!MainCamera)
            {
                _logger.LogError("[CameraService] Cannot convert viewport to world - camera not initialized.");
                return 0f;
            }

            var clampedViewportY = Mathf.Clamp01(viewportY01);
            var cameraY = MainCamera.transform.position.y;
            var halfHeight = MainCamera.orthographicSize;

            // Viewport 0 => bottom (cameraY - halfHeight), 1 => top (cameraY + halfHeight)
            return cameraY + halfHeight * (2f * clampedViewportY - 1f);
        }
    }
}