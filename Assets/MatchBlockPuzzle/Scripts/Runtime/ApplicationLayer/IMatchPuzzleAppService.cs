using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using System;
using MatchPuzzle.ApplicationLayer;

namespace MatchPuzzle.ApplicationLayerLayer
{
    /// <summary>
    /// Main facade for the Match Puzzle game
    /// Entry point for all game operations
    /// </summary>
    public interface IMatchPuzzleAppService
    {
        /// <summary>
        /// Current game state
        /// </summary>
        GameStateManager GameState { get; }
        
        /// <summary>
        /// Size of each cell in the grid (world units)
        /// </summary>
        float CellSize { get; }

        /// <summary>
        /// Global event bus
        /// </summary>
        IGlobalEventBus EventBus { get; }

        // Lifecycle callbacks
        event Action LevelSwitched;
        event Action LevelRestarted;
        event Action NormalizationCompleted;

        // Animation callbacks
        event Func<Block, GridPosition, UniTask> BlockMoveInvoked; // Block, old position
        event Func<BlockMove, UniTask> BlockFallInvoked; // Gravity move
        event Func<Block, UniTask> BlockDestroyInvoked; // Destruction animation

        /// <summary>
        /// Initializes the facade and all services
        /// </summary>
        UniTask InitializeAsync();

        /// <summary>
        /// Starts a new game (loads first level or continues from saved state)
        /// </summary>
        UniTask StartGameAsync();

        /// <summary>
        /// Loads a specific level
        /// </summary>
        UniTask LoadLevelAsync(int levelNumber);

        /// <summary>
        /// Restarts the current level
        /// </summary>
        void RestartLevel();

        /// <summary>
        /// Loads the next level
        /// </summary>
        UniTask LoadNextLevelAsync();

        /// <summary>
        /// Moves a block in a direction
        /// </summary>
        void MoveBlock(GridPosition position, Direction direction);

        /// <summary>
        /// Saves the current game state
        /// </summary>
        UniTask SaveGame();

        /// <summary>
        /// Disposes the facade and all services
        /// </summary>
        void Dispose();
    }
}
