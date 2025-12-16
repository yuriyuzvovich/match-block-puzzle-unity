using System.Collections.Generic;
using MatchPuzzle.ApplicationLayerLayer.Commands;
using Cysharp.Threading.Tasks;

namespace MatchPuzzle.ApplicationLayerLayer.Queue
{
    /// <summary>
    /// Global command queue implementation
    /// Executes commands sequentially
    /// </summary>
    public class CommandQueue : ICommandQueue
    {
        private readonly Queue<ICommand> _commands = new Queue<ICommand>();
        private bool _isProcessing;
        private UniTaskCompletionSource _completionSource;

        public bool IsProcessing => _isProcessing;

        public void Enqueue(ICommand command)
        {
            if (!command.CanExecute())
            {
                return;
            }

            _commands.Enqueue(command);

            if (!_isProcessing)
            {
                _isProcessing = true;
                _completionSource ??= new UniTaskCompletionSource();
                ProcessQueueAsync().Forget();
            }
        }

        public async UniTask WaitForCompletion()
        {
            if (!_isProcessing && _commands.Count == 0)
                return;

            // This ensures that we wait until all current commands are processed
            _completionSource ??= new UniTaskCompletionSource();
            await _completionSource.Task;
        }

        public void Clear()
        {
            _commands.Clear();

            if (!_isProcessing && _completionSource != null)
            {
                _completionSource.TrySetResult();
                _completionSource = null;
            }
        }

        private async UniTaskVoid ProcessQueueAsync()
        {
            try
            {
                while (_commands.Count > 0)
                {
                    var command = _commands.Dequeue();

                    if (command.CanExecute())
                    {
                        await command.ExecuteAsync();
                    }
                }
            }
            finally
            {
                _isProcessing = false;

                if (_completionSource != null)
                {
                    _completionSource.TrySetResult();
                    _completionSource = null;
                }
            }
        }
    }
}
