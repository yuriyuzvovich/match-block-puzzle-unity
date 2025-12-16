using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure;
using MatchPuzzle.Infrastructure.Bootstrap;
using Object = UnityEngine.Object;

namespace MatchPuzzle.Features.UI
{
    public sealed class UIStep : BootstrapStepBase, ICleanupStep
    {
        private readonly string _uiAssetKeysKey;
        private UIAssetKeys _uiAssetKeys;
        private MatchPuzzleUIView _uiViewPrefab;
        private MatchPuzzleUIView _uiViewInstance;

        public UIStep(string uiAssetKeysKey)
        {
            _uiAssetKeysKey = uiAssetKeysKey ?? throw new ArgumentNullException(nameof(uiAssetKeysKey));
        }

        public override string Id => "UI";
        public override IReadOnlyList<string> DependsOn => new[] { "AssetService", "GridPresentation" };

        public override async UniTask PreRunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var assetService = services.Get<IAssetService>();

            _uiAssetKeys = await assetService.LoadAsync<UIAssetKeys>(_uiAssetKeysKey);
            if (!_uiAssetKeys)
            {
                throw new InvalidOperationException($"Failed to load UIAssetKeys from key: {_uiAssetKeysKey}");
            }

            services.Register(_uiAssetKeys);

            _uiViewPrefab = await assetService.LoadAsync<MatchPuzzleUIView>(_uiAssetKeys.MatchPuzzleUIViewPrefabKey);
            if (!_uiViewPrefab)
            {
                throw new InvalidOperationException($"Failed to load UI view prefab from key: {_uiAssetKeys.MatchPuzzleUIViewPrefabKey}");
            }
        }

        public override async UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<ILoggerService>(out var logger);

            _uiViewInstance = Object.Instantiate(_uiViewPrefab);
            var uiView = _uiViewInstance.GetComponent<IMatchPuzzleUIView>();
            if (uiView == null)
            {
                throw new InvalidOperationException("UI view prefab does not have MatchPuzzleUIView component!");
            }

            services.Register<IMatchPuzzleUIView>(uiView);
            logger?.LogInformation("[Bootstrap] UI view loaded.");

            await UniTask.Yield();
        }

        public void Cleanup(ServiceContainer services)
        {
            if (_uiViewInstance != null)
            {
                Object.Destroy(_uiViewInstance);
                _uiViewInstance = null;
            }
        }
    }
}
