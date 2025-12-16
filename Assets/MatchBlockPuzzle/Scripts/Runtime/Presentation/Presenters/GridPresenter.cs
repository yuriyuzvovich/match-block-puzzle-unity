using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;
using UnityEngine;
using Grid = MatchPuzzle.Core.Domain.Grid;

namespace MatchPuzzle.Runtime.Presentation
{
    /// <summary>
    /// Presenter that owns grid presentation logic and drives the grid view.
    /// </summary>
    public class GridPresenter : IGridPresenter
    {
        private readonly IGridView _view;
        private readonly ILoggerService _logger;
        private readonly IPoolService _poolService;
        private readonly GameSettings _gameSettings;
        private readonly BlockAnimationSettings _animationSettings;
        private readonly AssetKeys _assetKeys;
        private readonly IGridDataProvider _gridDataProvider;

        private readonly Dictionary<long, BlockView> _blockViews = new Dictionary<long, BlockView>();

        private int _gridRows;
        private int _gridColumns;

        public GridPresenter(
            IGridView view,
            ILoggerService logger,
            IPoolService poolService,
            GameSettings gameSettings,
            BlockAnimationSettings animationSettings,
            AssetKeys assetKeys,
            IGridDataProvider gridDataProvider)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _poolService = poolService ?? throw new ArgumentNullException(nameof(poolService));
            _gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
            _animationSettings = animationSettings ?? throw new ArgumentNullException(nameof(animationSettings));
            _assetKeys = assetKeys ?? throw new ArgumentNullException(nameof(assetKeys));
            _gridDataProvider = gridDataProvider ?? throw new ArgumentNullException(nameof(gridDataProvider));
        }

        public void Initialize()
        {
            _gridRows = 0;
            _gridColumns = 0;
        }

        public async UniTask<BlockView> CreateBlockViewAsync(Block block)
        {
            if (block == null)
                return null;

            var typeData = _gameSettings.GetBlockTypeData(block.Type);
            if (!typeData)
            {
                _logger.LogError($"No BlockTypeData found for type {block.Type}");
                return null;
            }

            var blockView = await _poolService.GetAsync<BlockView>(_assetKeys.BlockViewPrefabKey, _view.RootTransform);
            if (!blockView)
            {
                _logger.LogError("[GridPresenter] Failed to get BlockView from pool.");
                return null;
            }
            blockView.Initialize(
                block,
                typeData,
                _animationSettings,
                _gridDataProvider
            );
            blockView.name = $"Block_{block.Type}_{block.Position}";

            _blockViews[block.Id] = blockView;
            return blockView;
        }

        public BlockView GetBlockView(Block block)
        {
            if (block == null)
                return null;

            _blockViews.TryGetValue(block.Id, out var blockView);
            return blockView;
        }

        public void RemoveBlockView(Block block)
        {
            if (block == null)
                return;

            if (_blockViews.TryGetValue(block.Id, out var blockView))
            {
                _blockViews.Remove(block.Id);
                if (blockView != null)
                {
                    _poolService.Return(_assetKeys.BlockViewPrefabKey, blockView);
                }
            }
        }

        public void ClearAllBlockViews()
        {
            foreach (var blockView in _blockViews.Values)
            {
                if (blockView)
                {
                    _poolService.Return(_assetKeys.BlockViewPrefabKey, blockView);
                }
            }

            _blockViews.Clear();
            _gridRows = 0;
            _gridColumns = 0;
        }

        public async UniTask CreateAllBlockViewsAsync(Grid grid)
        {
            if (grid == null)
            {
                _logger.LogError("[GridPresenter] Cannot create block views without a grid.");
                return;
            }

            ClearAllBlockViews();

            _gridRows = grid.Rows;
            _gridColumns = grid.Columns;

            var blocks = grid.GetAllBlocks();
            foreach (var block in blocks)
            {
                await CreateBlockViewAsync(block);
            }
        }

        public GridPosition ScreenToGridPosition(Vector2 screenPosition, Camera camera)
        {
            var worldPosition = camera.ScreenToWorldPoint(screenPosition);
            return WorldToGridPosition(worldPosition);
        }

        public GridPosition WorldToGridPosition(Vector3 worldPosition)
        {
            var x = worldPosition.x - _gridDataProvider.GridOffset.x;
            var y = worldPosition.y - _gridDataProvider.GridOffset.y;

            var col = Mathf.RoundToInt(x / _gridDataProvider.CellSize - 0.5f);
            var row = Mathf.RoundToInt(y / _gridDataProvider.CellSize - 0.5f);

            return new GridPosition(row, col);
        }

        public Vector3 GridToWorldPosition(GridPosition gridPosition)
        {
            var x = _gridDataProvider.GridOffset.x + (gridPosition.Column + 0.5f) * _gridDataProvider.CellSize;
            var y = _gridDataProvider.GridOffset.y + (gridPosition.Row + 0.5f) * _gridDataProvider.CellSize;
            return new Vector3(x, y, 0);
        }
    }
}