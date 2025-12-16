using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using System;
using MatchPuzzle.ApplicationLayer;

namespace MatchPuzzle.ApplicationLayerLayer.Commands
{
    /// <summary>
    /// Command to swap two blocks (when target cell is occupied)
    /// </summary>
    public class SwipeCommand : ICommand
    {
        private readonly GameStateManager _gameState;
        private readonly GridPosition _blockPosition;
        private readonly Direction _direction;
        private readonly IGlobalEventBus _eventBus;
        private readonly Func<Block, GridPosition, UniTask> _onBlockMoved;
        private readonly Func<UniTask> _onNormalizationNeeded;
        private readonly ILoggerService _logger;

        public SwipeCommand(
            GameStateManager gameState,
            GridPosition blockPosition,
            Direction direction,
            IGlobalEventBus eventBus,
            Func<Block, GridPosition, UniTask> onBlockMoved,
            Func<UniTask> onNormalizationNeeded,
            ILoggerService logger = null)
        {
            _gameState = gameState;
            _blockPosition = blockPosition;
            _direction = direction;
            _eventBus = eventBus;
            _onBlockMoved = onBlockMoved;
            _onNormalizationNeeded = onNormalizationNeeded;
            _logger = logger;
        }

        public bool CanExecute()
        {
            // Can't execute if normalizing
            if (_gameState.IsNormalizing)
            {
                _logger?.LogDebug($"[SwipeCommand] Cannot execute: grid is normalizing");
                return false;
            }

            var grid = _gameState.CurrentGrid;
            if (grid == null)
            {
                _logger?.LogWarning($"[SwipeCommand] Cannot execute: grid is null");
                return false;
            }

            // Check source block exists and can interact
            var block = grid.GetBlock(_blockPosition);
            if (block == null)
            {
                _logger?.LogDebug($"[SwipeCommand] Cannot execute: no block at {_blockPosition}");
                return false;
            }

            if (!block.CanInteract)
            {
                _logger?.LogDebug($"[SwipeCommand] Cannot execute: block at {_blockPosition} cannot interact");
                return false;
            }

            // Get target position
            var targetPosition = grid.GetNeighbor(_blockPosition, _direction);

            // Check target is in bounds
            if (!grid.IsValidPosition(targetPosition))
            {
                _logger?.LogDebug($"[SwipeCommand] Cannot execute: target {targetPosition} out of bounds (direction: {_direction})");
                return false;
            }

            // Check target is OCCUPIED (not empty)
            if (grid.IsEmpty(targetPosition))
            {
                _logger?.LogDebug($"[SwipeCommand] Cannot execute: target {targetPosition} is empty");
                return false;
            }

            // Check target block can interact
            var targetBlock = grid.GetBlock(targetPosition);
            if (targetBlock == null)
            {
                _logger?.LogDebug($"[SwipeCommand] Cannot execute: target block is null at {targetPosition}");
                return false;
            }

            if (!targetBlock.CanInteract)
            {
                _logger?.LogDebug($"[SwipeCommand] Cannot execute: target block at {targetPosition} cannot interact");
                return false;
            }

            // Upward swaps are allowed (no direction restriction)
            return true;
        }

        public async UniTask ExecuteAsync()
        {
            var grid = _gameState.CurrentGrid;
            var sourceBlock = grid.GetBlock(_blockPosition);

            if (sourceBlock == null)
                return;

            var targetPosition = grid.GetNeighbor(_blockPosition, _direction);
            var targetBlock = grid.GetBlock(targetPosition);

            if (targetBlock == null)
                return;

            // Store old positions for animation
            var sourceOldPosition = sourceBlock.Position;
            var targetOldPosition = targetBlock.Position;

            // Perform the swap
            grid.SwapBlocks(_blockPosition, targetPosition);

            // Increment move count
            _gameState.IncrementMoveCount();

            // Publish event
            _eventBus?.Publish(new MoveExecutedEvent(_blockPosition, _direction));

            // Wait for both swap animations to complete
            if (_onBlockMoved != null)
            {
                await UniTask.WhenAll(
                    _onBlockMoved(sourceBlock, sourceOldPosition),
                    _onBlockMoved(targetBlock, targetOldPosition)
                );
            }

            // Trigger normalization
            if (_onNormalizationNeeded != null)
            {
                await _onNormalizationNeeded();
            }
        }
    }
}
