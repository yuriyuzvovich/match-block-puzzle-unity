using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using MatchPuzzle.ApplicationLayer;
using MatchPuzzle.Infrastructure.Data;

namespace MatchPuzzle.ApplicationLayerLayer.Commands
{
    /// <summary>
    /// Command to normalize the grid (gravity + matching)
    /// Runs until no more changes occur
    /// </summary>
    public class NormalizeGridCommand : ICommand
    {
        private readonly GameStateManager _gameState;
        private readonly IGlobalEventBus _eventBus;
        private readonly Func<BlockMove, UniTask> _onBlockFall;
        private readonly Func<Block, UniTask> _onBlockDestroy;
        private readonly Action _onNormalizationComplete;
        private readonly MatchSettings _settings;

        public NormalizeGridCommand(
            GameStateManager gameState,
            IGlobalEventBus eventBus,
            Func<BlockMove, UniTask> onBlockFall,
            Func<Block, UniTask> onBlockDestroy,
            Action onNormalizationComplete,
            MatchSettings settings)
        {
            _gameState = gameState;
            _eventBus = eventBus;
            _onBlockFall = onBlockFall;
            _onBlockDestroy = onBlockDestroy;
            _onNormalizationComplete = onNormalizationComplete;
            _settings = settings;
        }

        public bool CanExecute()
        {
            return _gameState.CurrentGrid != null && !_gameState.IsNormalizing;
        }

        public async UniTask ExecuteAsync()
        {
            _gameState.IsNormalizing = true;

            var grid = _gameState.CurrentGrid;
            var normalizationEngine = _gameState.NormalizationEngine;

            bool hasChanges = true;

            while (hasChanges)
            {
                var result = normalizationEngine.Normalize();
                hasChanges = result.HasChanges;

                if (!hasChanges)
                    break;

                // Handle gravity (blocks falling)
                if (result.Moves.Count > 0)
                {
                    // Collect all fall animation tasks
                    var fallTasks = new List<UniTask>();
                    foreach (var move in result.Moves)
                    {
                        var block = grid.GetBlock(move.To);
                        if (block != null)
                        {
                            block.IsFalling = true;
                            if (_onBlockFall != null)
                            {
                                fallTasks.Add(_onBlockFall(move));
                            }
                        }
                    }

                    // Wait for all fall animations to complete
                    if (fallTasks.Count > 0)
                    {
                        await UniTask.WhenAll(fallTasks);
                    }

                    // Mark blocks as not falling
                    foreach (var move in result.Moves)
                    {
                        var block = grid.GetBlock(move.To);
                        if (block != null)
                        {
                            block.IsFalling = false;
                        }
                    }
                }

                // Handle matches
                if (result.MatchedAreas.Count > 0)
                {
                    var totalBlockCount = result.MatchedAreas.Sum(area => area.Count);

                    // Mark blocks as being destroyed
                    var blocksToDestroy = result.MatchedAreas
                        .SelectMany(area => area)
                        .Select(pos => grid.GetBlock(pos))
                        .Where(block => block != null)
                        .ToList();

                    foreach (var block in blocksToDestroy)
                    {
                        block.IsBeingDestroyed = true;
                    }

                    // Trigger destroy animations
                    var destroyTasks = blocksToDestroy.Select(block => _onBlockDestroy(block));
                    await UniTask.WhenAll(destroyTasks);

                    // Actually destroy blocks
                    normalizationEngine.DestroyBlocks(result.MatchedAreas);

                    // Publish events
                    _eventBus?.Publish(new BlocksMatchedEvent(totalBlockCount));
                    _eventBus?.Publish(new BlocksDestroyedEvent(totalBlockCount));

                    // Wait a bit before next cycle
                    var postMatchDelayMs = GetPostMatchDelayMs();
                    if (postMatchDelayMs > 0)
                    {
                        await UniTask.Delay(postMatchDelayMs);
                    }
                }
            }

            _gameState.IsNormalizing = false;
            _onNormalizationComplete?.Invoke();

            // Check if level is complete
            if (_gameState.IsLevelComplete())
            {
                _eventBus?.Publish(new LevelCompletedEvent(
                    _gameState.CurrentLevelNumber,
                    _gameState.MoveCount));
            }
        }

        private int GetPostMatchDelayMs()
        {
            return _settings ? Math.Max(0, _settings.PostMatchDelayMs) : 100;
        }
    }
}
