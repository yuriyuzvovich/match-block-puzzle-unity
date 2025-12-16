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
    public class SwitchAndRestartCommandTests
    {
        [Test]
        public async Task SwitchLevelCommand_InitializesLevelAndPublishesEvent()
        {
            var state = new GameStateManager(new NoopPersistence());
            var level = new Level(5, 1, 1);
            var bus = new FakeEventBus();
            bool invoked = false;
            var command = new SwitchLevelCommand(state, level, bus, () => invoked = true);

            Assert.IsTrue(command.CanExecute());
            await command.ExecuteAsync();

            Assert.AreEqual(5, state.CurrentLevelNumber);
            Assert.NotNull(state.CurrentGrid);
            Assert.IsTrue(invoked);
            Assert.That(bus.Events, Has.Exactly(1).TypeOf<LevelStartedEvent>());
        }

        [Test]
        public async Task RestartLevelCommand_ReinitializesCurrentLevelAndPublishesEvent()
        {
            var state = new GameStateManager(new NoopPersistence());
            var level = new Level(1, 1, 1);
            state.InitializeLevel(level);
            state.IncrementMoveCount();

            var bus = new FakeEventBus();
            bool restarted = false;
            var command = new RestartLevelCommand(state, bus, () => restarted = true);

            Assert.IsTrue(command.CanExecute());
            await command.ExecuteAsync();

            Assert.AreEqual(0, state.MoveCount);
            Assert.IsTrue(restarted);
            Assert.That(bus.Events, Has.Exactly(1).TypeOf<LevelStartedEvent>());
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
