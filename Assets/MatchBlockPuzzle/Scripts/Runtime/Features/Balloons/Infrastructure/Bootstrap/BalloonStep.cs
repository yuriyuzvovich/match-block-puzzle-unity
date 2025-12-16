using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure;
using MatchPuzzle.Infrastructure.Bootstrap;
using MatchPuzzle.Infrastructure.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MatchPuzzle.Features.Balloon
{
    public sealed class BalloonStep : BootstrapStepBase, ICleanupStep
    {
        private readonly string _balloonAssetKeysKey;
        private GameObject _balloonContainerPrefab;
        private BalloonAssetKeys _balloonAssetKeys;
        private BalloonSettings _balloonSettings;
        private bool _isEnabled = true;

        public BalloonStep(string balloonAssetKeysKey)
        {
            _balloonAssetKeysKey = balloonAssetKeysKey ?? throw new ArgumentNullException(nameof(balloonAssetKeysKey));
        }

        public override string Id => "Balloon";
        public override IReadOnlyList<string> DependsOn => new[] { "GridPresentation", "Camera", "Pool", "Settings" };

        public override async UniTask PreRunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var assetService = services.Get<IAssetService>();
            _balloonAssetKeys = await assetService.LoadAsync<BalloonAssetKeys>(_balloonAssetKeysKey);
            if (!_balloonAssetKeys)
            {
                throw new InvalidOperationException($"Failed to load BalloonAssetKeys from key: {_balloonAssetKeysKey}");
            }

            services.Register(_balloonAssetKeys);

            _balloonSettings = await assetService.LoadAsync<BalloonSettings>(_balloonAssetKeys.BalloonSettingsKey);
            if (!_balloonSettings)
            {
                throw new InvalidOperationException($"Failed to load BalloonSettings from key: {_balloonAssetKeys.BalloonSettingsKey}");
            }

            _isEnabled = _balloonSettings.IsEnabled;
            if (!_isEnabled)
            {
                return;
            }

            _balloonContainerPrefab = await assetService.LoadAsync<GameObject>(_balloonAssetKeys.BalloonContainerViewPrefabKey);
            if (!_balloonContainerPrefab)
            {
                throw new InvalidOperationException($"Failed to load balloon container prefab from key: {_balloonAssetKeys.BalloonContainerViewPrefabKey}");
            }
        }

        public override async UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<ILoggerService>(out var logger);

            var poolingSettings = services.Get<PoolingSettings>();
            var poolService = services.Get<IPoolService>();

            if (poolingSettings.EnablePoolWarmup && _balloonSettings.BalloonViewWarmupCount > 0)
            {
                await poolService.WarmupAsync(_balloonAssetKeys.BalloonViewPrefabKey, _balloonSettings.BalloonViewWarmupCount);
            }

            if (!_isEnabled)
            {
                logger?.LogInformation("[Bootstrap] Balloon feature disabled via settings; skipping balloon initialization.");
                return;
            }

            var cameraService = services.Get<ICameraService>();

            if (!services.TryGet<IMatchPuzzleRoot>(out var root) || root == null)
            {
                throw new InvalidOperationException("Cannot initialize balloon spawner - root view not initialized.");
            }

            var balloonContainer = Object.Instantiate(_balloonContainerPrefab, root.ThisTransform).transform;
            var balloonSpawner = new BalloonSpawner(poolService, cameraService, balloonContainer, logger);

            balloonSpawner.Initialize(_balloonSettings);
            services.Register<IBalloonSpawner>(balloonSpawner);

            logger?.LogInformation("[Bootstrap] Balloon spawner service initialized.");
        }

        public void Cleanup(ServiceContainer services)
        {
            if (services != null && services.TryGet<IBalloonSpawner>(out var spawner))
            {
                spawner.Cleanup();
            }
        }
    }
}