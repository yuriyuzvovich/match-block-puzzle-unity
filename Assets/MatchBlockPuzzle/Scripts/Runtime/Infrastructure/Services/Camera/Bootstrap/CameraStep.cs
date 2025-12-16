using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Bootstrap;
using MatchPuzzle.Infrastructure.Data;

namespace MatchPuzzle.Infrastructure.Services
{
    public sealed class CameraStep : BootstrapStepBase
    {
        public override string Id => "Camera";
        public override IReadOnlyList<string> DependsOn => new[] { "CoreServices", "Settings" };

        public override async UniTask PreRunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var assetKeys = services.Get<AssetKeys>();
            var cameraSettings = await services.Get<IAssetService>().LoadAsync<CameraSettings>(assetKeys.CameraSettingsKey);
            if (!cameraSettings)
            {
                throw new InvalidOperationException($"Failed to load CameraSettings from key: {assetKeys.CameraSettingsKey}");
            }

            services.Register(cameraSettings);
        }

        public override UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var logger = services.TryGet<ILoggerService>(out var l) ? l : null;
            var cameraSettings = services.Get<CameraSettings>();
            var gridDataProvider = services.Get<IGridDataProvider>();

            if (gridDataProvider == null)
            {
                throw new InvalidOperationException("Cannot initialize camera service - grid data provider not loaded.");
            }

            ICameraService cameraService = new CameraService(
                cameraSettings,
                gridDataProvider,
                services.Get<IAssetService>(),
                logger
            );

            cameraService.InitializeAsync();
            services.Register<ICameraService>(cameraService);

            logger.LogInformation("[Bootstrap] Camera service initialized.");
            return UniTask.CompletedTask;
        }
    }
}
