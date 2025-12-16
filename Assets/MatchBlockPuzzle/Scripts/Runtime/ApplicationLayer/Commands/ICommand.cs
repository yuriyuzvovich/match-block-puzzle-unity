using Cysharp.Threading.Tasks;

namespace MatchPuzzle.ApplicationLayerLayer.Commands
{
    /// <summary>
    /// Base interface for all commands
    /// Commands can be synchronous or asynchronous
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command asynchronously
        /// </summary>
        UniTask ExecuteAsync();

        /// <summary>
        /// Checks if the command can be executed in the current game state
        /// </summary>
        bool CanExecute();
    }
}
