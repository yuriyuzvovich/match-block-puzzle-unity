using System;
using Cysharp.Threading.Tasks;
using MatchPuzzle.ApplicationLayerLayer;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Features.Background;
using MatchPuzzle.Features.Balloon;
using MatchPuzzle.Features.UI;
using UnityEngine;

namespace MatchPuzzle.Infrastructure
{
    public class MatchPuzzleGameController : IMatchPuzzleGameController
    {
        private readonly IMatchPuzzleAppService _appService;
        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;
        private readonly IBalloonSpawner _balloonSpawner;
        private readonly IBackgroundPresenter _backgroundPresenter;
        private readonly IGridPresenter _gridPresenter;
        private readonly IMatchPuzzleUIView _uiView;
        private IMatchPuzzleUIPresenter _uiPresenter;
        private readonly IUnityEventsService _unityEventsService;

        private bool _isDisposed;

        public MatchPuzzleGameController(
            IMatchPuzzleAppService appService,
            IInputService inputService,
            ICameraService cameraService,
            IGridPresenter gridPresenter,
            IBackgroundPresenter backgroundPresenter,
            IMatchPuzzleUIView uiView,
            IBalloonSpawner balloonSpawner,
            IUnityEventsService unityEventsService
        )
        {
            _appService = appService ?? throw new ArgumentNullException(nameof(appService));
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
            _gridPresenter = gridPresenter ?? throw new ArgumentNullException(nameof(gridPresenter));
            _backgroundPresenter = backgroundPresenter ?? NullBackgroundPresenter.Instance;
            _uiView = uiView ?? throw new ArgumentNullException(nameof(uiView));
            _balloonSpawner = balloonSpawner ?? throw new ArgumentNullException(nameof(balloonSpawner));
            _unityEventsService = unityEventsService ?? throw new ArgumentNullException(nameof(unityEventsService));
        }

        public async UniTask InitializeAsync()
        {
            SubscribeToAppEvents();
            SubscribeToInputEvents();
            _unityEventsService.OnUpdate += HandleFrameTick;
            SetupUI();

            await UniTask.Yield();

            _inputService.Enable();
        }

        public async UniTask StartGameAsync()
        {
            await _appService.StartGameAsync();
        }

        private void SubscribeToAppEvents()
        {
            _appService.LevelSwitched += HandleLevelStart;
            _appService.LevelRestarted += HandleLevelStart;
            _appService.BlockMoveInvoked += HandleBlockMoved;
            _appService.BlockFallInvoked += HandleBlockFall;
            _appService.BlockDestroyInvoked += HandleBlockDestroy;
        }

        private void UnsubscribeFromAppEvents()
        {
            _appService.LevelSwitched -= HandleLevelStart;
            _appService.LevelRestarted -= HandleLevelStart;
            _appService.BlockMoveInvoked -= HandleBlockMoved;
            _appService.BlockFallInvoked -= HandleBlockFall;
            _appService.BlockDestroyInvoked -= HandleBlockDestroy;
        }

        private void SubscribeToInputEvents()
        {
            _inputService.OnSwipe += HandleSwipe;
            _inputService.OnTap += HandleTap;
        }

        private void UnsubscribeFromInputEvents()
        {
            _inputService.OnSwipe -= HandleSwipe;
            _inputService.OnTap -= HandleTap;
        }

        private void SetupUI()
        {
            if (_uiView != null)
            {
                _uiPresenter = new MatchPuzzleUIPresenter(_uiView);
                _uiPresenter.Initialize();
                _uiPresenter.RestartButtonClicked += HandleRestartButton;
                _uiPresenter.NextButtonClicked += HandleNextButton;
            }
        }

        private void CleanupUI()
        {
            if (_uiPresenter != null)
            {
                _uiPresenter.RestartButtonClicked -= HandleRestartButton;
                _uiPresenter.NextButtonClicked -= HandleNextButton;
                _uiPresenter.Dispose();
                _uiPresenter = null;
            }
        }

        private async void HandleLevelStart()
        {
            // Create block views
            await _gridPresenter.CreateAllBlockViewsAsync(_appService.GameState.CurrentGrid);

            // Adjust camera for grid
            _cameraService.AdjustCameraForGrid(
                _appService.GameState.CurrentGrid.Rows,
                _appService.GameState.CurrentGrid.Columns,
                _appService.CellSize
            );

            // Adjust background to match new camera size
            _backgroundPresenter?.AdjustBackgroundSize();

            // Refresh balloons to match new camera/grid bounds
            _balloonSpawner.RefreshSpawnArea();
        }

        private async UniTask HandleBlockMoved(Block block, GridPosition oldPosition)
        {
            var blockView = _gridPresenter.GetBlockView(block);
            if (blockView)
            {
                await blockView.AnimateMoveAsync(oldPosition, block.Position);
            }
        }

        private async UniTask HandleBlockFall(BlockMove move)
        {
            var block = _appService.GameState.CurrentGrid.GetBlock(move.To);
            if (block == null)
                return;

            var blockView = _gridPresenter.GetBlockView(block);
            if (blockView)
            {
                await blockView.AnimateFallAsync(move.To);
            }
        }

        private async UniTask HandleBlockDestroy(Block block)
        {
            var blockView = _gridPresenter.GetBlockView(block);
            if (blockView)
            {
                await blockView.AnimateDestroyAsync();
                _gridPresenter.RemoveBlockView(block);
            }
        }

        private void HandleSwipe(SwipeData swipeData)
        {
            var grid = _appService.GameState.CurrentGrid;
            if (grid == null)
                return;

            // Convert start position to grid position
            var gridPosition = _gridPresenter.ScreenToGridPosition(swipeData.StartPosition, _cameraService.MainCamera);

            // VALIDATION 1: Check if position is within grid bounds
            if (!grid.IsValidPosition(gridPosition))
            {
                ShowInvalidSwipeFeedback(swipeData.StartPosition, "Position out of bounds");
                return;
            }

            // VALIDATION 2: Check if there's a block at the source position
            var block = grid.GetBlock(gridPosition);
            if (block == null)
            {
                ShowInvalidSwipeFeedback(swipeData.StartPosition, "No block at position");
                return;
            }

            // VALIDATION 3: Check if block can interact
            if (!block.CanInteract)
            {
                ShowInvalidSwipeFeedback(swipeData.StartPosition, "Block cannot interact");
                return;
            }

            // Determine direction
            var direction = GetSwipeDirection(swipeData);

            // Execute move
            _appService.MoveBlock(gridPosition, direction);
        }

        private void HandleTap(Vector2 screenPosition)
        {
        }

        private Direction GetSwipeDirection(SwipeData swipeData)
        {
            var delta = swipeData.Delta;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                // Horizontal swipe
                return delta.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                // Vertical swipe
                return delta.y > 0 ? Direction.Up : Direction.Down;
            }
        }

        private void HandleRestartButton()
        {
            _appService.RestartLevel();
        }

        private void HandleNextButton()
        {
            _appService.LoadNextLevelAsync().Forget();
        }

        private void ShowInvalidSwipeFeedback(Vector2 screenPosition, string reason)
        {
            // TODO: Add visual/audio feedback
            // - Shake animation at touch position
            // - Play "denied" sound effect
            // - Show brief X indicator

            // For now, the InvalidMoveAttemptEvent published by facade can be used
            // by other systems to provide feedback
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _inputService?.Disable();
            _unityEventsService.OnUpdate -= HandleFrameTick;

            UnsubscribeFromAppEvents();
            UnsubscribeFromInputEvents();
            CleanupUI();
        }

        private void HandleFrameTick(float deltaTime)
        {
            _inputService?.DoFrameTick();
            _balloonSpawner?.DoFrameTick();
        }
    }
}
