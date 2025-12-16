using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Features.Balloon
{
    public class BalloonSpawner : IBalloonSpawner
    {
        private readonly IPoolService _poolService;
        private BalloonSettings _balloonSettings;
        private readonly ICameraService _cameraService;
        private readonly Transform _balloonContainer;
        private readonly ILoggerService _logger;

        private readonly Dictionary<BalloonView, string> _balloonPrefabLookup = new Dictionary<BalloonView, string>();
        private readonly Dictionary<BalloonView, IBalloonPresenter> _presenterLookup = new Dictionary<BalloonView, IBalloonPresenter>();
        private List<BalloonView> _activeBalloons = new List<BalloonView>();
        private bool _isInitialized;
        private bool _isSpawningPaused;
        private bool _isSpawnInProgress;

        public BalloonSpawner(
            IPoolService poolService,
            ICameraService cameraService,
            Transform balloonContainer,
            ILoggerService logger
        )
        {
            _poolService = poolService ?? throw new System.ArgumentNullException(nameof(poolService));
            _cameraService = cameraService ?? throw new System.ArgumentNullException(nameof(cameraService));
            _balloonContainer = balloonContainer ?? throw new System.ArgumentNullException(nameof(balloonContainer));
            _logger = logger;
        }

        public void Initialize(BalloonSettings balloonSettings)
        {
            _balloonSettings = balloonSettings ?? throw new System.ArgumentNullException(nameof(balloonSettings));
            _isInitialized = true;
            _isSpawningPaused = false;
            _logger.LogInformation("[BalloonSpawnerService] Service initialized.");
        }

        public void DoFrameTick()
        {
            if (!_isInitialized || _isSpawningPaused || _poolService == null || _balloonSettings == null)
                return;

            // Remove off-screen balloons
            for (int i = _activeBalloons.Count - 1; i >= 0; i--)
            {
                var balloon = _activeBalloons[i];

                if (_presenterLookup.TryGetValue(balloon, out var presenter))
                {
                    if (presenter.IsOffScreen(
                            _cameraService.MainCamera,
                            _balloonSettings.OffscreenMarginRelativeToViewWidthMin,
                            _balloonSettings.OffscreenMarginRelativeToViewWidthMax
                        ))
                    {
                        _activeBalloons.RemoveAt(i);
                        ReturnBalloon(balloon);
                    }
                }
            }

            // Spawn new balloons if needed
            if (_activeBalloons.Count < _balloonSettings.MaxBalloons && !_isSpawnInProgress)
            {
                SpawnBalloonAsync().Forget();
            }
        }

        public void Cleanup()
        {
            CleanupActiveBalloons();
        }

        public void RefreshSpawnArea()
        {
            if (!_isInitialized)
            {
                _logger?.LogWarning("[BalloonSpawnerService] Cannot refresh - service not initialized.");
                return;
            }

            _logger?.LogInformation("[BalloonSpawnerService] Refreshing spawn area for new level/camera.");

            _isSpawningPaused = true;
            CleanupActiveBalloons(log : false);
            _isSpawningPaused = false;

            SpawnUntilMaxAsync().Forget();
        }

        private async UniTask SpawnBalloonAsync()
        {
            if (_poolService == null || !_balloonSettings)
            {
                _logger.LogWarning("[BalloonSpawnerService] PoolService or BalloonSettings is null. Cannot spawn balloon.");
                return;
            }

            _isSpawnInProgress = true;
            try
            {
                // Random direction: left to right (1) or right to left (-1)
                var direction = Random.value > 0.5f ? 1f : -1f;

                // Random height using normalized range from settings
                var cameraHeight = _cameraService.MainCamera.orthographicSize * 2f;
                var minY = -cameraHeight / 2f + (cameraHeight * _balloonSettings.SpawnHeightMinNormalized);
                var maxY = -cameraHeight / 2f + (cameraHeight * _balloonSettings.SpawnHeightMaxNormalized);
                var randomY = Random.Range(minY, maxY);

                // Spawn position (off screen)
                Vector3 startPosition;
                var cameraWidth = cameraHeight * _cameraService.MainCamera.aspect;
                var xOffset = Random.Range(
                    _balloonSettings.SpawnXOffsetMinRelativeToWidth,
                    _balloonSettings.SpawnXOffsetMaxRelativeToWidth
                ) * cameraWidth;

                if (direction > 0)
                {
                    // Spawn on left side
                    startPosition = new Vector3(-cameraWidth / 2f - xOffset, randomY, _balloonSettings.SpawnZCoordinate);
                }
                else
                {
                    // Spawn on right side
                    startPosition = new Vector3(cameraWidth / 2f + xOffset, randomY, _balloonSettings.SpawnZCoordinate);
                }

                // Convert viewport to world
                startPosition = new Vector3(_cameraService.CameraTransform.position.x, _cameraService.CameraTransform.position.y, 0f) + startPosition;

                var prefabKey = _balloonSettings.GetRandomPrefabKey();

                var balloon = await _poolService.GetAsync<BalloonView>(prefabKey, _balloonContainer);
                if (!balloon)
                {
                    _logger?.LogError($"[BalloonSpawnerService] Failed to spawn balloon for key: {prefabKey}");
                    return;
                }

                // Random size relative to camera orthographic size
                var sizeMultiplier = Random.Range(
                    _balloonSettings.SizeMinRelativeToCamera,
                    _balloonSettings.SizeMaxRelativeToCamera
                );
                var targetScale = _cameraService.MainCamera.orthographicSize * sizeMultiplier;
                var speed = Random.Range(_balloonSettings.BalloonMinSpeed, _balloonSettings.BalloonMaxSpeed);

                IBalloonPresenter presenter = new BalloonPresenter(balloon);
                presenter.Initialize(
                    startPosition,
                    targetScale,
                    speed,
                    direction,
                    _balloonSettings.BalloonSineAmplitude,
                    _balloonSettings.BalloonSineFrequency
                );
                _presenterLookup[balloon] = presenter;

                _activeBalloons.Add(balloon);
                _balloonPrefabLookup[balloon] = prefabKey;
            }
            finally
            {
                _isSpawnInProgress = false;
            }
        }

        private void CleanupActiveBalloons(bool log = true)
        {
            if (_poolService == null || _balloonSettings == null)
                return;

            foreach (var balloon in _activeBalloons)
            {
                ReturnBalloon(balloon);
            }

            _activeBalloons.Clear();
            _balloonPrefabLookup.Clear();

            if (log)
            {
                _logger.LogInformation("[BalloonSpawnerService] Cleanup complete.");
            }
        }

        private async UniTask SpawnUntilMaxAsync()
        {
            if (!_balloonSettings)
                return;

            while (_activeBalloons.Count < _balloonSettings.MaxBalloons)
            {
                var previousCount = _activeBalloons.Count;
                await SpawnBalloonAsync();
                if (_activeBalloons.Count == previousCount)
                {
                    _logger?.LogWarning("[BalloonSpawnerService] Failed to spawn a balloon during warmup; stopping spawn loop.");
                    break;
                }
            }
        }

        private void ReturnBalloon(BalloonView balloon)
        {
            if (!balloon || _poolService == null)
                return;

            if (!_balloonPrefabLookup.TryGetValue(balloon, out var prefabKey) || string.IsNullOrEmpty(prefabKey))
            {
                prefabKey = _balloonSettings?.GetRandomPrefabKey();
                _logger?.LogWarning("[BalloonSpawnerService] Missing prefab key for balloon; using random fallback.");
            }

            // Reset presenter if we have one
            if (_presenterLookup.TryGetValue(balloon, out var presenter))
            {
                presenter.Reset();
                _presenterLookup.Remove(balloon);
            }

            _poolService.Return(prefabKey, balloon);
            _balloonPrefabLookup.Remove(balloon);
        }
    }
}