using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using Cysharp.Threading.Tasks;

namespace MatchPuzzle.ApplicationLayer
{
    /// <summary>
    /// Manages the current game state (grid, level, move count, etc.)
    /// </summary>
    public class GameStateManager
    {
        public Grid CurrentGrid { get; private set; }
        public Level CurrentLevel { get; private set; }
        public int CurrentLevelNumber { get; private set; }
        public int MoveCount { get; private set; }
        public bool IsNormalizing { get; set; }
        public NormalizationEngine NormalizationEngine { get; private set; }

        private readonly IPersistenceService _persistenceService;

        public GameStateManager(IPersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
        }

        /// <summary>
        /// Initializes a new level
        /// </summary>
        public void InitializeLevel(Level level)
        {
            CurrentLevel = level;
            CurrentLevelNumber = level.LevelNumber;
            MoveCount = 0;
            IsNormalizing = false;

            // Create grid
            CurrentGrid = new Grid(level.Rows, level.Columns);

            // Place blocks
            foreach (var blockData in level.Blocks)
            {
                var position = blockData.ToGridPosition();
                var block = new Block(blockData.Type, position);
                CurrentGrid.SetBlock(position, block);
            }
        }

        /// <summary>
        /// Restores level state from saved data
        /// </summary>
        public void RestoreLevelState(LevelStateProfileData levelState, Level level)
        {
            CurrentLevel = level; // Store the level object so RestartLevel can work
            CurrentLevelNumber = levelState.LevelNumber;
            MoveCount = 0; // Reset move count for restored level
            IsNormalizing = false;

            // Create grid
            CurrentGrid = new Grid(levelState.Rows, levelState.Columns);

            // Place blocks
            foreach (var blockState in levelState.Blocks)
            {
                var id = blockState.Id;
                var position = new GridPosition(blockState.Row, blockState.Column);
                var block = new Block(id, blockState.Type, position);
                CurrentGrid.SetBlock(position, block);
            }
        }

        /// <summary>
        /// Increments the move counter
        /// </summary>
        public void IncrementMoveCount()
        {
            MoveCount++;
        }

        /// <summary>
        /// Resets the move counter
        /// </summary>
        public void ResetMoveCount()
        {
            MoveCount = 0;
        }

        /// <summary>
        /// Checks if the level is complete (no blocks left)
        /// </summary>
        public bool IsLevelComplete()
        {
            return CurrentGrid != null && CurrentGrid.IsGridEmpty();
        }

        /// <summary>
        /// Creates the NormalizationEngine for the current grid.
        /// Should be called after level initialization.
        /// </summary>
        public void CreateNormalizationEngine(int minMatchLength)
        {
            if (CurrentGrid != null)
            {
                NormalizationEngine = new NormalizationEngine(CurrentGrid, minMatchLength);
            }
        }

        /// <summary>
        /// Saves the current game state asynchronously
        /// </summary>
        public UniTask SaveStateAsync()
        {
            if (CurrentGrid == null || CurrentLevel == null)
                return UniTask.CompletedTask;

            var gameState = new GameStateProfileData
            {
                CurrentLevelNumber = CurrentLevelNumber,
                CurrentLevelState = CreateLevelState()
            };

            return _persistenceService.SaveGameStateAsync(gameState);
        }

        /// <summary>
        /// Creates a LevelState from the current grid
        /// </summary>
        private LevelStateProfileData CreateLevelState()
        {
            var blocks = CurrentGrid.GetAllBlocks();
            var blockStates = new BlockStateProfileData[blocks.Count];

            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                blockStates[i] = new BlockStateProfileData(
                    block.Id,
                    block.Type,
                    block.Position.Row,
                    block.Position.Column
                );
            }

            return new LevelStateProfileData
            {
                LevelNumber = CurrentLevelNumber,
                Rows = CurrentGrid.Rows,
                Columns = CurrentGrid.Columns,
                Blocks = blockStates
            };
        }

        /// <summary>
        /// Loads saved game state asynchronously
        /// </summary>
        public UniTask<GameStateProfileData> LoadStateAsync()
        {
            return _persistenceService.LoadGameStateAsync();
        }

        /// <summary>
        /// Checks if there is a saved state
        /// </summary>
        public bool HasSavedState()
        {
            return _persistenceService.HasSavedState();
        }
    }
}
