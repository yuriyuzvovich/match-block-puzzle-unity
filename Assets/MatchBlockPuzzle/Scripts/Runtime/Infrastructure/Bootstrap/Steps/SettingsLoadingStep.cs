using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;
using MatchPuzzle.Infrastructure.Services;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Bootstrap
{
    public sealed class SettingsLoadingStep : BootstrapStepBase
    {
        public override string Id => "Settings";
        public override IReadOnlyList<string> DependsOn => new[] { "Logging", "AssetKeys" };

        public override async UniTask PreRunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var assetService = services.Get<IAssetService>();
            var assetKeys = services.Get<AssetKeys>();
            services.TryGet<ILoggerService>(out var logger);

            var gameSettings = await assetService.LoadAsync<GameSettings>(assetKeys.GameSettingsKey);
            if (!gameSettings)
            {
                throw new InvalidOperationException($"Failed to load GameSettings from key: {assetKeys.GameSettingsKey}");
            }
            services.Register(gameSettings);

            var gridSettings = await assetService.LoadAsync<GridSettings>(assetKeys.GridSettingsKey);
            if (!gridSettings)
            {
                throw new InvalidOperationException("GridSettings is missing.");
            }

            var gridDataProvider = new SettingsGridDataProvider(gridSettings);
            services.Register<IGridDataProvider>(gridDataProvider);

            var playerInputSettings = await assetService.LoadAsync<PlayerInputSettings>(assetKeys.InputSettingsKey);
            if (!playerInputSettings)
            {
                logger.LogWarning($"[Bootstrap] InputSettings not found at key: {assetKeys.InputSettingsKey}. Falling back to default values.");
            }
            services.Register(playerInputSettings);

            var blockAnimationSettings = await assetService.LoadAsync<BlockAnimationSettings>(assetKeys.BlockAnimationSettingsKey);
            if (!blockAnimationSettings)
            {
                logger.LogWarning($"[Bootstrap] BlockAnimationSettings not found at key: {assetKeys.BlockAnimationSettingsKey}. Falling back to defaults.");
            }
            services.Register(blockAnimationSettings);

            var matchSettings = await assetService.LoadAsync<MatchSettings>(assetKeys.MatchSettingsKey);
            if (!matchSettings)
            {
                logger?.LogWarning($"[Bootstrap] MatchSettings not found at key: {assetKeys.MatchSettingsKey}. Falling back to defaults.");
                matchSettings = ScriptableObject.CreateInstance<MatchSettings>();
            }
            services.Register(matchSettings);

            var poolingSettings = await assetService.LoadAsync<PoolingSettings>(assetKeys.PoolingSettingsKey);
            if (!poolingSettings)
            {
                logger.LogWarning($"[Bootstrap] PoolingSettings not found at key: {assetKeys.PoolingSettingsKey}. Falling back to defaults.");
            }
            services.Register(poolingSettings);
        }

        public override UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<ILoggerService>(out var logger);
            logger.LogInformation("[Bootstrap] Settings loaded.");
            return UniTask.CompletedTask;
        }
    }
}
