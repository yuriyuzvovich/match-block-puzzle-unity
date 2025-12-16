using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MatchPuzzle.ApplicationLayer;
using MatchPuzzle.ApplicationLayerLayer.Commands;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using NUnit.Framework;

namespace MatchPuzzle.Tests.Editor.ApplicationLayer
{
    public class SwipeCommandTests
    {
        private GameStateManager _state;
        private FakeEventBus _eventBus;
        private List<(Block block, GridPosition oldPos)> _moves;

        [SetUp]
        public void SetUp()
        {
            _state = new GameStateManager(new NoopPersistence());
            _eventBus = new FakeEventBus();
            _moves = new List<(Block, GridPosition)>();
        }

        [Test]
        public async System.Threading.Tasks.Task ExecuteAsync_SwapsBlocksAndPublishesEvent()
        {
            InitializeGridWithTwoBlocks();
            var command = new SwipeCommand(
                _state,
                new GridPosition(0, 0),
                Direction.Right,
                _eventBus,
                (b, oldPos) => RecordMove(b, oldPos),
                () => UniTask.CompletedTask
            );

            Assert.IsTrue(command.CanExecute());
            await command.ExecuteAsync();

            Assert.AreEqual(1, _state.MoveCount);
            Assert.That(_eventBus.Events, Has.Exactly(1).TypeOf<MoveExecutedEvent>());
            Assert.AreEqual(new GridPosition(0, 1), _state.CurrentGrid.GetBlock(new GridPosition(0, 1)).Position);
            Assert.AreEqual(new GridPosition(0, 0), _state.CurrentGrid.GetBlock(new GridPosition(0, 0)).Position);
            Assert.AreEqual(2, _moves.Count);
        }

        [Test]
        public void CanExecute_ReturnsFalseWhenTargetEmpty()
        {
            InitializeGridWithSingleBlock();
            var command = new SwipeCommand(
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
        public void CanExecute_ReturnsFalseWhenTargetBlockNotInteractable()
        {
            InitializeGridWithTwoBlocks();
            _state.CurrentGrid.GetBlock(new GridPosition(0, 1)).IsBeingDestroyed = true;

            var command = new SwipeCommand(
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
            InitializeGridWithTwoBlocks();
            var command = new SwipeCommand(
                _state,
                new GridPosition(0, 0),
                Direction.Left,
                _eventBus,
                null,
                null
            );

            Assert.IsFalse(command.CanExecute());
        }

        [Test]
        public void CanExecute_ReturnsFalseWhenGridNull()
        {
            var command = new SwipeCommand(
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
        public async System.Threading.Tasks.Task ExecuteAsync_DoesNothingWhenCanExecuteFalse()
        {
            InitializeGridWithSingleBlock(); // target empty => CanExecute false
            var command = new SwipeCommand(
                _state,
                new GridPosition(0, 0),
                Direction.Right,
                _eventBus,
                (b, oldPos) => RecordMove(b, oldPos),
                () => UniTask.CompletedTask
            );

            Assert.IsFalse(command.CanExecute());
            await command.ExecuteAsync();

            Assert.AreEqual(0, _state.MoveCount);
            Assert.IsEmpty(_eventBus.Events);
            Assert.IsEmpty(_moves);
            Assert.AreEqual(new GridPosition(0, 0), _state.CurrentGrid.GetBlock(new GridPosition(0, 0)).Position);
        }

        private void InitializeGridWithTwoBlocks()
        {
            var level = new Level(1, 1, 2);
            level.Blocks.Add(new BlockData(new BlockTypeId("A"), 0, 0));
            level.Blocks.Add(new BlockData(new BlockTypeId("B"), 0, 1));
            _state.InitializeLevel(level);
        }

        private void InitializeGridWithSingleBlock()
        {
            var level = new Level(1, 1, 2);
            level.Blocks.Add(new BlockData(new BlockTypeId("A"), 0, 0));
            _state.InitializeLevel(level);
        }

        private UniTask RecordMove(Block block, GridPosition oldPos)
        {
            _moves.Add((block, oldPos));
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
