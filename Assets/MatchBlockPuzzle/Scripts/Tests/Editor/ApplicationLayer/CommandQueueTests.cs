using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MatchPuzzle.ApplicationLayerLayer.Commands;
using MatchPuzzle.ApplicationLayerLayer.Queue;
using NUnit.Framework;

namespace MatchPuzzle.Tests.Editor.ApplicationLayer
{
    public class CommandQueueTests
    {
        [Test]
        public async Task Enqueue_ExecutesCommandsSequentially()
        {
            var executedOrder = new List<string>();
            var queue = new CommandQueue();

            queue.Enqueue(new FakeCommand("first", executedOrder, delayMs: 10));
            queue.Enqueue(new FakeCommand("second", executedOrder));

            await queue.WaitForCompletion();

            CollectionAssert.AreEqual(new[] { "first", "second" }, executedOrder);
            Assert.IsFalse(queue.IsProcessing);
        }

        [Test]
        public async Task Enqueue_SkipsCommandsThatCannotExecute()
        {
            var executedOrder = new List<string>();
            var queue = new CommandQueue();

            queue.Enqueue(new FakeCommand("skip", executedOrder, canExecute: false));
            queue.Enqueue(new FakeCommand("run", executedOrder));

            await queue.WaitForCompletion();

            CollectionAssert.AreEqual(new[] { "run" }, executedOrder);
        }

        [Test]
        public async Task Clear_WhileProcessing_RemovesPendingCommandsAndCompletes()
        {
            var executed = new List<string>();
            var queue = new CommandQueue();
            var gate = new UniTaskCompletionSource();

            queue.Enqueue(new BlockingCommand("first", executed, gate));
            queue.Enqueue(new FakeCommand("second", executed));

            // Allow the first command to start
            await UniTask.Yield();

            // Clear pending commands before unblocking the first
            queue.Clear();
            gate.TrySetResult();

            await queue.WaitForCompletion();

            CollectionAssert.AreEqual(new[] { "first" }, executed);
            Assert.IsFalse(queue.IsProcessing);
        }

        [Test]
        public async Task WaitForCompletion_ReturnsImmediatelyWhenIdle()
        {
            var queue = new CommandQueue();

            await queue.WaitForCompletion();

            Assert.IsFalse(queue.IsProcessing);
        }

        [Test]
        public async Task Enqueue_AfterClear_StillProcesses()
        {
            var executed = new List<string>();
            var queue = new CommandQueue();
            queue.Clear();

            queue.Enqueue(new FakeCommand("run", executed));
            await queue.WaitForCompletion();

            CollectionAssert.AreEqual(new[] { "run" }, executed);
        }

        private class FakeCommand : ICommand
        {
            private readonly string _id;
            private readonly List<string> _executed;
            private readonly bool _canExecute;
            private readonly int _delayMs;

            public FakeCommand(string id, List<string> executed, bool canExecute = true, int delayMs = 0)
            {
                _id = id;
                _executed = executed;
                _canExecute = canExecute;
                _delayMs = delayMs;
            }

            public bool CanExecute() => _canExecute;

            public async UniTask ExecuteAsync()
            {
                if (_delayMs > 0)
                {
                    await UniTask.Delay(_delayMs);
                }

                _executed.Add(_id);
            }
        }

        private class BlockingCommand : ICommand
        {
            private readonly string _id;
            private readonly List<string> _executed;
            private readonly UniTaskCompletionSource _gate;

            public BlockingCommand(string id, List<string> executed, UniTaskCompletionSource gate)
            {
                _id = id;
                _executed = executed;
                _gate = gate;
            }

            public bool CanExecute() => true;

            public async UniTask ExecuteAsync()
            {
                await _gate.Task;
                _executed.Add(_id);
            }
        }
    }
}
