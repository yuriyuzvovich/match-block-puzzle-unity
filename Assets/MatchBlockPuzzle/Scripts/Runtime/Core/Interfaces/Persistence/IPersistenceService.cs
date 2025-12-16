using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Domain;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Service for persisting game state between sessions
    /// </summary>
    public interface IPersistenceService
    {
        /// <summary>
        /// Saves the current game state asynchronously
        /// </summary>
        UniTask SaveGameStateAsync(GameStateProfileData stateProfileData);

        /// <summary>
        /// Loads the saved game state asynchronously
        /// </summary>
        UniTask<GameStateProfileData> LoadGameStateAsync();

        /// <summary>
        /// Checks if saved game state exists
        /// </summary>
        bool HasSavedState();

        /// <summary>
        /// Clears all saved data
        /// </summary>
        void ClearSavedState();
    }
}
