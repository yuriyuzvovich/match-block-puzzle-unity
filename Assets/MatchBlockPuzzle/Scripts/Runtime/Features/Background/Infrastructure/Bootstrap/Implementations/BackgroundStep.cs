using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure;
using MatchPuzzle.Infrastructure.Bootstrap;
using MatchPuzzle.Features.Background.Data;
using Object = UnityEngine.Object;

namespace MatchPuzzle.Features.Background
{
    public sealed class BackgroundStep : BootstrapStepBase, ICleanupStep
    {
        private BackgroundView _backgroundViewObject;
        private readonly string _backgroundAssetKeysKey;
        private BackgroundAssetKeys _backgroundAssetKeys;
        private BackgroundSettings _backgroundSettings;
        private BackgroundView _backgroundViewPrefab;

        public override string Id => "Background";
        public override IReadOnlyList<string> DependsOn => new[] { "Camera", "Settings" };

        public BackgroundStep(string backgroundAssetKeysKey)
        {
            _backgroundAssetKeysKey = backgroundAssetKeysKey ?? throw new ArgumentNullException(nameof(backgroundAssetKeysKey));
        }

        public override async UniTask PreRunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var assetService = services.Get<IAssetService>();
            _backgroundAssetKeys = await assetService.LoadAsync<BackgroundAssetKeys>(_backgroundAssetKeysKey);
            if (!_backgroundAssetKeys)
            {
                throw new InvalidOperationException($"Failed to load BackgroundAssetKeys from key: {_backgroundAssetKeysKey}");
            }

            services.Register(_backgroundAssetKeys);

            _backgroundSettings = await assetService.LoadAsync<BackgroundSettings>(_backgroundAssetKeys.BackgroundSettingsKey);
            if (_backgroundSettings == null)
            {
                throw new InvalidOperationException($"Failed to load BackgroundSettings from key: {_backgroundAssetKeys.BackgroundSettingsKey}");
            }

            _backgroundViewPrefab = await assetService.LoadAsync<BackgroundView>(_backgroundAssetKeys.BackgroundViewPrefabKey);
            if (!_backgroundViewPrefab)
            {
                throw new InvalidOperationException($"Failed to load background view prefab from key: {_backgroundAssetKeys.BackgroundViewPrefabKey}");
            }
        }

        public override async UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<ILoggerService>(out var logger);
            var assetService = services.Get<IAssetService>();
            var cameraService = services.Get<ICameraService>();
            // Background view
            _backgroundViewObject = Object.Instantiate(_backgroundViewPrefab);
            var backgroundView = _backgroundViewObject.GetComponent<IBackgroundView>();
            if (backgroundView == null)
            {
                throw new InvalidOperationException("Background view prefab does not have BackgroundView component!");
            }

            IBackgroundPresenter backgroundPresenter = new BackgroundPresenter(backgroundView, _backgroundSettings, cameraService, logger);
            backgroundPresenter.Initialize();
            services.Register<IBackgroundPresenter>(backgroundPresenter);
        }

        public void Cleanup(ServiceContainer services)
        {
            if (_backgroundViewObject)
            {
                Object.Destroy(_backgroundViewObject);
                _backgroundViewObject = null;
            }
        }
    }
}