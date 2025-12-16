using MatchPuzzle.ApplicationLayerLayer.Commands;
using Cysharp.Threading.Tasks;

namespace MatchPuzzle.ApplicationLayerLayer.Queue
{
    /// <summary>
    /// Global command queue interface
    /// Executes commands one by one, waiting for each to complete
    /// </summary>
    public interface ICommandQueue
    {
        /// <summary>
        /// Enqueues a command for execution
        /// </summary>
        void Enqueue(ICommand command);

        /// <summary>
        /// Checks if the queue is currently processing commands
        /// </summary>
        bool IsProcessing { get; }

        /// <summary>
        /// Waits for all commands in the queue to complete
        /// </summary>
        UniTask WaitForCompletion();

        /// <summary>
        /// Clears all pending commands
        /// </summary>
        void Clear();
    }
}
