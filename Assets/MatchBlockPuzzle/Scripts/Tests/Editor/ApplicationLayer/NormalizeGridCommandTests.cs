using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MatchPuzzle.ApplicationLayer;
using MatchPuzzle.ApplicationLayerLayer.Commands;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;
using NUnit.Framework;
using UnityEngine;

namespace MatchPuzzle.Tests.Editor.ApplicationLayer
{
    public class NormalizeGridCommandTests
    {
        [Test]
        public async System.Threading.Tasks.Task ExecuteAsync_AppliesGravityAndInvokesFallCallbacks()
        {
            var state = CreateStateWithGrid(rows: 2, cols: 1, new[] { new BlockData(new BlockTypeId("A"), 1, 0) });
            state.CreateNormalizationEngine(3);
            var bus = new FakeEventBus();
            var falls = new List<BlockMove>();
            var command = new NormalizeGridCommand(
                state,
                bus,
                move => { falls.Add(move); return UniTask.CompletedTask; },
                block => UniTask.CompletedTask,
                () => { },
                CreateMatchSettings(delayMs: 0)
            );

            Assert.IsTrue(command.CanExecute());
            await command.ExecuteAsync();

            var block = state.CurrentGrid.GetBlock(new GridPosition(0, 0));
            Assert.NotNull(block);
            Assert.AreEqual(new GridPosition(0, 0), block.Position);
            Assert.AreEqual(1, falls.Count);
            Assert.IsFalse(state.IsNormalizing);
            Assert.IsEmpty(bus.Events);
        }

        [Test]
        public async System.Threading.Tasks.Task ExecuteAsync_DestroysMatchesAndPublishesEvents()
        {
            var state = CreateStateWithGrid(rows: 1, cols: 3, new[]
            {
                new BlockData(new BlockTypeId("A"), 0, 0),
                new BlockData(new BlockTypeId("A"), 0, 1),
                new BlockData(new BlockTypeId("A"), 0, 2),
            });
            state.CreateNormalizationEngine(3);
            var bus = new FakeEventBus();
            var destroyed = new List<Block>();
            var command = new NormalizeGridCommand(
                state,
                bus,
                move => UniTask.CompletedTask,
                block => { destroyed.Add(block); return UniTask.CompletedTask; },
                () => { },
                CreateMatchSettings(delayMs: 0)
            );

            await command.ExecuteAsync();

            Assert.IsTrue(state.CurrentGrid.IsGridEmpty());
            Assert.That(bus.Events, Has.Some.TypeOf<BlocksMatchedEvent>());
            Assert.That(bus.Events, Has.Some.TypeOf<BlocksDestroyedEvent>());
            Assert.AreEqual(3, destroyed.Count);
        }

        [Test]
        public void CanExecute_ReturnsFalseWhenNormalizingOrGridNull()
        {
            var state = new GameStateManager(new NoopPersistence());
            var commandNoGrid = new NormalizeGridCommand(state, new FakeEventBus(), null, null, null, null);
            Assert.IsFalse(commandNoGrid.CanExecute());

            var state2 = CreateStateWithGrid(1, 1, null);
            state2.CreateNormalizationEngine(3);
            state2.IsNormalizing = true;
            var commandBusy = new NormalizeGridCommand(state2, new FakeEventBus(), null, null, null, null);
            Assert.IsFalse(commandBusy.CanExecute());
        }

        [Test]
        public async System.Threading.Tasks.Task ExecuteAsync_PublishesLevelCompletedWhenGridEmpty()
        {
            var state = CreateStateWithGrid(rows: 1, cols: 1, blocks: null);
            state.CreateNormalizationEngine(3);
            var bus = new FakeEventBus();
            bool completed = false;
            var command = new NormalizeGridCommand(
                state,
                bus,
                null,
                null,
                () => completed = true,
                CreateMatchSettings(delayMs: 0)
            );

            await command.ExecuteAsync();

            Assert.IsTrue(completed);
            Assert.That(bus.Events, Has.Some.TypeOf<LevelCompletedEvent>());
        }

        private static GameStateManager CreateStateWithGrid(int rows, int cols, IEnumerable<BlockData> blocks)
        {
            var state = new GameStateManager(new NoopPersistence());
            var level = new Level(1, rows, cols);
            if (blocks != null)
            {
                level.Blocks.AddRange(blocks);
            }
            state.InitializeLevel(level);
            return state;
        }

        private static MatchSettings CreateMatchSettings(int delayMs)
        {
            var settings = ScriptableObject.CreateInstance<MatchSettings>();
            var type = typeof(MatchSettings);
            type.GetField("_postMatchDelayMs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(settings, delayMs);
            type.GetField("_postNormalizationNextLevelDelayMs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(settings, 0);
            type.GetField("_minMatchLength", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(settings, 3);
            return settings;
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
