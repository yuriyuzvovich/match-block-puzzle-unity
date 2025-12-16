using Cysharp.Threading.Tasks;
using MatchPuzzle.ApplicationLayerLayer.Commands;
using MatchPuzzle.ApplicationLayerLayer.Queue;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using System;
using MatchPuzzle.ApplicationLayer;
using MatchPuzzle.Infrastructure.Data;
using MatchPuzzle.Infrastructure.Services.LevelRepository;

namespace MatchPuzzle.ApplicationLayerLayer
{
    public class MatchPuzzleAppService : IMatchPuzzleAppService
    {
        public GameStateManager GameState { get; private set; }
        public float CellSize => _gridDataProvider.CellSize;
        public IGlobalEventBus EventBus { get; private set; }

        public event Action LevelSwitched;
        public event Action LevelRestarted;
        public event Action NormalizationCompleted;
        public event Func<Block, GridPosition, UniTask> BlockMoveInvoked;
        public event Func<BlockMove, UniTask> BlockFallInvoked;
        public event Func<Block, UniTask> BlockDestroyInvoked;

        private readonly ICommandQueue _commandQueue;
        private readonly ILoggerService _logger;
        private readonly IGridDataProvider _gridDataProvider;
        private readonly ILevelRepository _levelRepository;
        private readonly MatchSettings _matchSettings;
        private int _maxLevelCount;

        public MatchPuzzleAppService(
            IPersistenceService persistenceService,
            IGlobalEventBus eventBus,
            IGridDataProvider gridDataProvider,
            ILoggerService logger,
            MatchSettings matchSettings,
            ILevelRepository levelRepository
        )
        {
            _logger = logger;
            EventBus = eventBus;
            _gridDataProvider = gridDataProvider ?? throw new ArgumentNullException(nameof(gridDataProvider));
            _commandQueue = new CommandQueue();
            _matchSettings = matchSettings;
            _levelRepository = levelRepository;

            GameState = new GameStateManager(persistenceService);
        }

        public async UniTask InitializeAsync()
        {
            await _levelRepository.InitializeAsync();
            _maxLevelCount = _levelRepository.LevelCount;
        }

        public async UniTask StartGameAsync()
        {
            // Check if there's a saved state
            if (GameState.HasSavedState())
            {
                var savedState = await GameState.LoadStateAsync();

                // Restore level state
                if (savedState.CurrentLevelState != null)
                {
                    // Get the level object so RestartLevel can work properly
                    var level = await GetLevelAsync(savedState.CurrentLevelNumber);
                    if (level == null)
                    {
                        _logger.LogError($"Level {savedState.CurrentLevelNumber} not found!");
                        // Fallback to level 1
                        await LoadLevelAsync(1);
                        return;
                    }

                    GameState.RestoreLevelState(savedState.CurrentLevelState, level);
                    CreateNormalizationEngine();
                    LevelSwitched?.Invoke();
                }
                else
                {
                    // No level state, load the saved level number
                    await LoadLevelAsync(savedState.CurrentLevelNumber);
                }
            }
            else
            {
                // Start from level 1
                await LoadLevelAsync(1);
            }
        }

        public async UniTask LoadLevelAsync(int levelNumber)
        {
            var level = await GetLevelAsync(levelNumber);

            if (level == null)
            {
                _logger.LogError($"Level {levelNumber} not found!");
                return;
            }

            var command = new SwitchLevelCommand(
                GameState,
                level,
                EventBus,
                DoOnLevelSwitched
            );

            _commandQueue.Enqueue(command);
            await _commandQueue.WaitForCompletion();

            // Save after loading
            await SaveGame();
        }

        private void DoOnLevelSwitched()
        {
            CreateNormalizationEngine();
            LevelSwitched?.Invoke();
        }

        private void CreateNormalizationEngine()
        {
            var minMatchLength = _matchSettings ? Math.Max(2, _matchSettings.MinMatchLength) : 3;
            GameState.CreateNormalizationEngine(minMatchLength);
        }

        public void RestartLevel()
        {
            var command = new RestartLevelCommand(
                GameState,
                EventBus,
                RiseLevelRestarted
            );

            _commandQueue.Enqueue(command);
        }

        private void RiseLevelRestarted()
        {
            CreateNormalizationEngine();
            LevelRestarted?.Invoke();
        }

        public async UniTask LoadNextLevelAsync()
        {
            var nextLevelNumber = GetNextLevelNumber(GameState.CurrentLevelNumber);
            await LoadLevelAsync(nextLevelNumber);
        }

        public void MoveBlock(GridPosition position, Direction direction)
        {
            var grid = GameState.CurrentGrid;
            if (grid == null)
            {
                _logger?.LogWarning("[MatchPuzzleFacade] Cannot move block: grid is null");
                return;
            }

            // VALIDATION: Source position must be valid and contain an interactable block
            if (!grid.IsValidPosition(position))
            {
                _logger?.LogWarning($"[MatchPuzzleFacade] Source position {position} out of bounds");
                EventBus?.Publish(new InvalidMoveAttemptEvent(position, direction, "Source position out of bounds"));
                return;
            }

            var block = grid.GetBlock(position);
            if (block == null)
            {
                _logger?.LogDebug($"[MatchPuzzleFacade] No block at position {position}");
                EventBus?.Publish(new InvalidMoveAttemptEvent(position, direction, "No block at source position"));
                return;
            }

            if (!block.CanInteract)
            {
                _logger?.LogDebug($"[MatchPuzzleFacade] Block at {position} cannot interact");
                EventBus?.Publish(new InvalidMoveAttemptEvent(position, direction, "Block cannot interact"));
                return;
            }

            // Calculate target position
            var targetPosition = grid.GetNeighbor(position, direction);

            // Validate target position is in bounds
            if (!grid.IsValidPosition(targetPosition))
            {
                _logger?.LogDebug($"[MatchPuzzleFacade] Target position {targetPosition} out of bounds");
                EventBus?.Publish(new InvalidMoveAttemptEvent(position, direction, "Target position out of bounds"));
                return;
            }

            ICommand command;

            // Decide which command to create based on target cell state
            if (grid.IsEmpty(targetPosition))
            {
                // Target is empty - create MoveCommand
                command = new MoveCommand(
                    GameState,
                    position,
                    direction,
                    EventBus,
                    DoOnBlockMoved,
                    NormalizeGridAsync,
                    _logger
                );
            }
            else
            {
                // Target is occupied - create SwipeCommand
                command = new SwipeCommand(
                    GameState,
                    position,
                    direction,
                    EventBus,
                    async (block, oldPos) => {
                        if (BlockMoveInvoked != null)
                        {
                            await BlockMoveInvoked(block, oldPos);
                        }
                    },
                    NormalizeGridAsync,
                    _logger
                );
            }

            _commandQueue.Enqueue(command);
        }

        private async UniTask DoOnBlockMoved(Block targetBlock, GridPosition oldPos)
        {
            if (BlockMoveInvoked != null)
            {
                await BlockMoveInvoked(targetBlock, oldPos);
            }
        }

        public UniTask SaveGame()
        {
            return GameState.SaveStateAsync();
        }

        public void Dispose()
        {
            _commandQueue.Clear();
            EventBus?.Clear();
            _levelRepository.ClearCache();
        }

        private UniTask NormalizeGridAsync()
        {
            var command = new NormalizeGridCommand(
                GameState,
                EventBus,
                async (move) => {
                    if (BlockFallInvoked != null)
                    {
                        await BlockFallInvoked(move);
                    }
                },
                async (block) => {
                    if (BlockDestroyInvoked != null)
                    {
                        await BlockDestroyInvoked(block);
                    }
                },
                () => {
                    NormalizationCompleted?.Invoke();

                    // Check if level is complete
                    if (GameState.IsLevelComplete())
                    {
                        // Auto-load next level after a delay
                        AutoLoadNextLevelAsync().Forget();
                    }
                    else
                    {
                        // Save state after normalization
                        SaveGame().Forget();
                    }
                },
                _matchSettings
            );

            _commandQueue.Enqueue(command);

            // Return completed task - the command queue will handle execution
            return UniTask.CompletedTask;
        }

        private async UniTaskVoid AutoLoadNextLevelAsync()
        {
            // Wait for destruction animation to finish
            var delayMs = _matchSettings
                ? Math.Max(0, _matchSettings.PostNormalizationNextLevelDelayMs)
                : 500;

            if (delayMs > 0)
            {
                await UniTask.Delay(delayMs);
            }

            await LoadNextLevelAsync();
        }

        private async UniTask<Level> GetLevelAsync(int levelNumber)
        {
            if (_maxLevelCount <= 0)
            {
                _logger?.LogError("No levels configured.");
                return null;
            }

            // Normalize level number (loop around)
            var normalizedLevelNumber = ((levelNumber - 1) % _maxLevelCount) + 1;

            return await _levelRepository.LoadLevelAsync(normalizedLevelNumber);
        }

        private int GetNextLevelNumber(int currentLevelNumber)
        {
            return currentLevelNumber + 1;
        }
    }
}
