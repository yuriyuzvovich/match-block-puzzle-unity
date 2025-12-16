using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;
using MatchPuzzle.Runtime.Presentation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MatchPuzzle.Infrastructure.Bootstrap
{
    public sealed class GridPresentationSetupStep : BootstrapStepBase, ICleanupStep
    {
        private GameObject _rootObject;
        private GridView _gridViewObject;

        public override string Id => "GridPresentation";
        public override IReadOnlyList<string> DependsOn => new[] { "Camera", "Pool", "Settings" };

        public override async UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<ILoggerService>(out var logger);
            var gameSettings = services.Get<GameSettings>();
            var gridDataProvider = services.Get<IGridDataProvider>();
            var assetKeys = services.Get<AssetKeys>();

            if (!gameSettings)
            {
                throw new InvalidOperationException("Cannot load screen view - GameSettings not loaded.");
            }

            if (gridDataProvider == null)
            {
                throw new InvalidOperationException("Cannot load screen view - grid data provider not initialized.");
            }

            var assetService = services.Get<IAssetService>();
            var poolService = services.Get<IPoolService>();

            try
            {
                // Screen view
                var rootPrefab = await assetService.LoadAsync<GameObject>(assetKeys.MatchPuzzleRootPrefabKey);
                if (!rootPrefab)
                {
                    throw new InvalidOperationException($"Failed to load screen view prefab from key: {assetKeys.MatchPuzzleRootPrefabKey}");
                }

                _rootObject = Object.Instantiate(rootPrefab);
                MatchPuzzleRoot root = _rootObject.GetComponent<MatchPuzzleRoot>();
                if (!root)
                {
                    throw new InvalidOperationException("Screen view prefab does not have MatchPuzzleRoot component!");
                }

                services.Register<IMatchPuzzleRoot>(root);

                // Grid view
                var gridViewPrefab = await assetService.LoadAsync<GridView>(assetKeys.GridViewPrefabKey);
                if (!gridViewPrefab)
                {
                    throw new InvalidOperationException($"Failed to load grid view prefab from key: {assetKeys.GridViewPrefabKey}");
                }

                _gridViewObject = Object.Instantiate(gridViewPrefab);
                var gridView = _gridViewObject.GetComponent<IGridView>();
                if (gridView == null)
                {
                    throw new InvalidOperationException("Grid view prefab does not have GridView component!");
                }

                var gridPresenter = new GridPresenter(
                    gridView,
                    logger,
                    poolService,
                    gameSettings,
                    services.Get<BlockAnimationSettings>(),
                    assetKeys,
                    gridDataProvider
                );

                gridPresenter.Initialize();
                services.Register<IGridPresenter>(gridPresenter);

                root.Show();

                await UniTask.Yield();
            }
            catch
            {
                Cleanup(services);
                throw;
            }
        }

        public void Cleanup(ServiceContainer services)
        {
            if (services != null && services.TryGet<IGridPresenter>(out var gridPresenter))
            {
                gridPresenter.ClearAllBlockViews();
            }

            if (services != null && services.TryGet<IMatchPuzzleRoot>(out var root) && root != null)
            {
                root.Cleanup();
                Object.Destroy(root.ThisGameObject);
            }

            if (_gridViewObject)
            {
                Object.Destroy(_gridViewObject);
                _gridViewObject = null;
            }

            if (_rootObject)
            {
                Object.Destroy(_rootObject);
                _rootObject = null;
            }
        }
    }
}
