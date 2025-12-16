using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using System;
using MatchPuzzle.ApplicationLayer;

namespace MatchPuzzle.ApplicationLayerLayer.Commands
{
    /// <summary>
    /// Command to move a block to an empty cell
    /// </summary>
    public class MoveCommand : ICommand
    {
        private readonly GameStateManager _gameState;
        private readonly GridPosition _blockPosition;
        private readonly Direction _direction;
        private readonly IGlobalEventBus _eventBus;
        private readonly Func<Block, GridPosition, UniTask> _onBlockMoved;
        private readonly Func<UniTask> _onNormalizationNeeded;
        private readonly ILoggerService _logger;

        public MoveCommand(
            GameStateManager gameState,
            GridPosition blockPosition,
            Direction direction,
            IGlobalEventBus eventBus,
            Func<Block, GridPosition, UniTask> onBlockMoved,
            Func<UniTask> onNormalizationNeeded,
            ILoggerService logger = null
        )
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
                _logger?.LogDebug($"[MoveCommand] Cannot execute: grid is normalizing");
                return false;
            }

            var grid = _gameState.CurrentGrid;
            if (grid == null)
            {
                _logger?.LogWarning($"[MoveCommand] Cannot execute: grid is null");
                return false;
            }

            // Check source block exists and can interact
            var block = grid.GetBlock(_blockPosition);
            if (block == null)
            {
                _logger?.LogDebug($"[MoveCommand] Cannot execute: no block at {_blockPosition}");
                return false;
            }

            if (!block.CanInteract)
            {
                _logger?.LogDebug($"[MoveCommand] Cannot execute: block at {_blockPosition} cannot interact");
                return false;
            }

            // Get target position
            var targetPosition = grid.GetNeighbor(_blockPosition, _direction);

            // Check target is in bounds
            if (!grid.IsValidPosition(targetPosition))
            {
                _logger?.LogDebug($"[MoveCommand] Cannot execute: target {targetPosition} out of bounds (direction: {_direction})");
                return false;
            }

            // Check target is EMPTY
            if (!grid.IsEmpty(targetPosition))
            {
                _logger?.LogDebug($"[MoveCommand] Cannot execute: target {targetPosition} is occupied");
                return false;
            }

            // CRITICAL: Reject upward moves into empty space
            if (_direction == Direction.Up)
            {
                _logger?.LogDebug($"[MoveCommand] Cannot execute: upward moves into empty space are not allowed (position: {_blockPosition})");
                return false;
            }

            return true;
        }

        public async UniTask ExecuteAsync()
        {
            var grid = _gameState.CurrentGrid;
            var block = grid.GetBlock(_blockPosition);

            if (block == null)
                return;

            var targetPosition = grid.GetNeighbor(_blockPosition, _direction);

            // Validate target is empty
            if (!grid.IsEmpty(targetPosition))
                return;

            // Store old position for animation
            var oldPosition = block.Position;

            // Move block to target position and clear source
            grid.SetBlock(targetPosition, block);
            grid.SetBlock(_blockPosition, null);

            // Increment move count
            _gameState.IncrementMoveCount();

            // Publish event
            _eventBus?.Publish(new MoveExecutedEvent(_blockPosition, _direction));

            // Wait for move animation to complete
            if (_onBlockMoved != null)
            {
                await _onBlockMoved(block, oldPosition);
            }

            // Trigger normalization
            if (_onNormalizationNeeded != null)
            {
                await _onNormalizationNeeded();
            }
        }
    }
}