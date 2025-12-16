using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MatchPuzzle.ApplicationLayer;
using MatchPuzzle.ApplicationLayerLayer.Commands;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using NUnit.Framework;

namespace MatchPuzzle.Tests.Editor.ApplicationLayer
{
    public class MoveCommandTests
    {
        private GameStateManager _state;
        private FakeEventBus _eventBus;
        private List<(Block block, GridPosition oldPos)> _moved;
        private int _normalizations;

        [SetUp]
        public void SetUp()
        {
            _state = new GameStateManager(new NoopPersistence());
            _eventBus = new FakeEventBus();
            _moved = new List<(Block, GridPosition)>();
            _normalizations = 0;
        }

        [Test]
        public async Task ExecuteAsync_MovesBlockToEmptyCellAndPublishesEvents()
        {
            InitializeGridWithBlock(rows: 1, cols: 2, blockPos: new GridPosition(0, 0));
            var command = new MoveCommand(
                _state,
                new GridPosition(0, 0),
                Direction.Right,
                _eventBus,
                (b, oldPos) => RecordMove(b, oldPos),
                () => { _normalizations++; return UniTask.CompletedTask; }
            );

            Assert.IsTrue(command.CanExecute());
            await command.ExecuteAsync();

            var movedBlock = _state.CurrentGrid.GetBlock(new GridPosition(0, 1));
            Assert.NotNull(movedBlock);
            Assert.AreEqual(new GridPosition(0, 1), movedBlock.Position);
            Assert.AreEqual(1, _state.MoveCount);
            Assert.That(_eventBus.Events, Has.Exactly(1).TypeOf<MoveExecutedEvent>());
            Assert.AreEqual(1, _moved.Count);
            Assert.AreEqual(new GridPosition(0, 0), _moved[0].oldPos);
            Assert.AreEqual(1, _normalizations);
        }

        [Test]
        public void CanExecute_ReturnsFalseForUpwardMoveIntoEmpty()
        {
            InitializeGridWithBlock(rows: 2, cols: 1, blockPos: new GridPosition(0, 0));
            var command = new MoveCommand(
                _state,
                new GridPosition(0, 0),
                Direction.Up,
                _eventBus,
                null,
                null
            );

            Assert.IsFalse(command.CanExecute());
        }

        [Test]
        public void CanExecute_ReturnsFalseWhenTargetOccupied()
        {
            InitializeGridWithTwoBlocks();
            var command = new MoveCommand(
                _state,
                new GridPosition(0, 0),
                Direction.Right,
                _eventBus,
                null,
                null
            );

            Assert.IsFalse(command.CanExecute());
        }

        [Test]
        public void CanExecute_ReturnsFalseWhenGridNull()
        {
            var command = new MoveCommand(
                _state,
                new GridPosition(0, 0),
                Direction.Right,
                _eventBus,
                null,
                null
            );

            Assert.IsFalse(command.CanExecute());
        }

        [Test]
        public void CanExecute_ReturnsFalseWhenTargetOutOfBounds()
        {
            InitializeGridWithBlock(rows: 1, cols: 1, blockPos: new GridPosition(0, 0));
            var command = new MoveCommand(
                _state,
                new GridPosition(0, 0),
                Direction.Right,
                _eventBus,
                null,
                null
            );

            Assert.IsFalse(command.CanExecute());
        }

        [Test]
        public void CanExecute_ReturnsFalseWhenSourceBlockNotInteractable()
        {
            InitializeGridWithBlock(rows: 1, cols: 2, blockPos: new GridPosition(0, 0));
            _state.CurrentGrid.GetBlock(new GridPosition(0, 0)).IsBeingDestroyed = true;

            var command = new MoveCommand(
                _state,
                new GridPosition(0, 0),
                Direction.Right,
                _eventBus,
                null,
                null
            );

            Assert.IsFalse(command.CanExecute());
        }

        [Test]
        public async Task ExecuteAsync_DoesNothingWhenCanExecuteFalse()
        {
            InitializeGridWithTwoBlocks(); // target occupied makes CanExecute false
            var command = new MoveCommand(
                _state,
                new GridPosition(0, 0),
                Direction.Right,
                _eventBus,
                (b, pos) => RecordMove(b, pos),
                () => UniTask.CompletedTask
            );

            Assert.IsFalse(command.CanExecute());
            await command.ExecuteAsync();

            Assert.AreEqual(0, _state.MoveCount);
            Assert.IsEmpty(_eventBus.Events);
            Assert.IsEmpty(_moved);
            // positions unchanged
            Assert.AreEqual(new GridPosition(0, 0), _state.CurrentGrid.GetBlock(new GridPosition(0, 0)).Position);
            Assert.AreEqual(new GridPosition(0, 1), _state.CurrentGrid.GetBlock(new GridPosition(0, 1)).Position);
        }

        private void InitializeGridWithBlock(int rows, int cols, GridPosition blockPos)
        {
            var level = new Level(1, rows, cols);
            level.Blocks.Add(new BlockData(new BlockTypeId("A"), blockPos.Row, blockPos.Column));
            _state.InitializeLevel(level);
        }

        private void InitializeGridWithTwoBlocks()
        {
            var level = new Level(1, 1, 2);
            level.Blocks.Add(new BlockData(new BlockTypeId("A"), 0, 0));
            level.Blocks.Add(new BlockData(new BlockTypeId("B"), 0, 1));
            _state.InitializeLevel(level);
        }

        private UniTask RecordMove(Block block, GridPosition oldPos)
        {
            _moved.Add((block, oldPos));
            return UniTask.CompletedTask;
        }

        private class FakeEventBus : IGlobalEventBus
        {
            public List<object> Events { get; } = new List<object>();
            public void Subscribe<TEvent>(System.Action<TEvent> handler) where TEvent : class { }
            public void Unsubscribe<TEvent>(System.Action<TEvent> handler) where TEvent : class { }
            public void Publish<TEvent>(TEvent eventData) where TEvent : class => Events.Add(eventData);
            public void Clear() => Events.Clear();
        }

        private class NoopPersistence : IPersistenceService
        {
            public UniTask SaveGameStateAsync(GameStateProfileData stateProfileData) => UniTask.CompletedTask;
            public UniTask<GameStateProfileData> LoadGameStateAsync() => UniTask.FromResult<GameStateProfileData>(null);
            public bool HasSavedState() => false;
            public void ClearSavedState() { }
        }
    }
}
