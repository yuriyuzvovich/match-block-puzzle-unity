using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MatchPuzzle.ApplicationLayer;
using MatchPuzzle.ApplicationLayerLayer;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;
using MatchPuzzle.Infrastructure.Services.LevelRepository;
using NUnit.Framework;
using UnityEngine;

namespace MatchPuzzle.Tests.Editor.ApplicationLayer
{
    public class MatchPuzzleAppServiceTests
    {
        private FakePersistence _persistence;
        private FakeEventBus _eventBus;
        private FakeGridDataProvider _gridDataProvider;
        private FakeLogger _logger;
        private FakeLevelRepository _levelRepository;
        private MatchPuzzleAppService _app;

        [SetUp]
        public void SetUp()
        {
            _persistence = new FakePersistence();
            _eventBus = new FakeEventBus();
            _gridDataProvider = new FakeGridDataProvider();
            _logger = new FakeLogger();
            _levelRepository = new FakeLevelRepository();
            _app = new MatchPuzzleAppService(
                _persistence,
                _eventBus,
                _gridDataProvider,
                _logger,
                null,
                _levelRepository
            );
        }

        [Test]
        public async Task StartGameAsync_LoadsLevelAndCreatesNormalizationEngine()
        {
            _levelRepository.SetLevelCount(2);
            _levelRepository.SetLevelFactory(num => CreateLevel(num, 2, 2, new[] { new BlockData(new BlockTypeId("A"), 0, 0) }));

            await _app.InitializeAsync();
            await _app.StartGameAsync();

            Assert.NotNull(_app.GameState.CurrentGrid);
            Assert.AreEqual(1, _app.GameState.CurrentLevelNumber);
            Assert.NotNull(_app.GameState.NormalizationEngine);
            Assert.IsTrue(_persistence.SavedStateExists);
            Assert.That(_eventBus.Events, Has.Some.TypeOf<LevelStartedEvent>());
        }

        [Test]
        public void MoveBlock_WhenNoBlock_PublishesInvalidMove()
        {
            var level = CreateLevel(1, 1, 1, blocks: null);
            _app.GameState.InitializeLevel(level);

            _app.MoveBlock(new GridPosition(0, 0), Direction.Left);

            Assert.That(_eventBus.Events, Has.Exactly(1).TypeOf<InvalidMoveAttemptEvent>());
            var evt = (InvalidMoveAttemptEvent)_eventBus.Events[0];
            Assert.AreEqual(new GridPosition(0, 0), evt.BlockPosition);
        }

        [Test]
        public void MoveBlock_WhenBlockCannotInteract_PublishesInvalidMove()
        {
            var level = CreateLevel(1, 1, 1, new[] { new BlockData(new BlockTypeId("A"), 0, 0) });
            _app.GameState.InitializeLevel(level);
            _app.GameState.CurrentGrid.GetBlock(new GridPosition(0, 0)).IsFalling = true;

            _app.MoveBlock(new GridPosition(0, 0), Direction.Right);

            Assert.That(_eventBus.Events, Has.Exactly(1).TypeOf<InvalidMoveAttemptEvent>());
            Assert.That(_eventBus.Events[0], Is.TypeOf<InvalidMoveAttemptEvent>());
            Assert.That(((InvalidMoveAttemptEvent)_eventBus.Events[0]).Reason, Does.Contain("cannot interact"));
        }

        [Test]
        public async Task StartGameAsync_RestoresSavedState_WhenPersistenceHasData()
        {
            var savedLevel = new LevelStateProfileData
            {
                LevelNumber = 2,
                Rows = 1,
                Columns = 1,
                Blocks = new[] { new BlockStateProfileData(1, new BlockTypeId("A"), 0, 0) }
            };
            _persistence.Saved = new GameStateProfileData
            {
                CurrentLevelNumber = 2,
                CurrentLevelState = savedLevel
            };
            _levelRepository.SetLevelCount(2);
            _levelRepository.SetLevelFactory(num => CreateLevel(num, 1, 1, new[] { new BlockData(new BlockTypeId("A"), 0, 0) }));
            bool levelSwitched = false;
            _app.LevelSwitched += () => levelSwitched = true;

            await _app.InitializeAsync();
            await _app.StartGameAsync();

            Assert.AreEqual(2, _app.GameState.CurrentLevelNumber);
            Assert.NotNull(_app.GameState.CurrentGrid);
            Assert.NotNull(_app.GameState.NormalizationEngine);
            Assert.IsTrue(levelSwitched);
        }

        [Test]
        public async Task LoadNextLevelAsync_WrapsAroundLevelCount()
        {
            _levelRepository.SetLevelCount(2);
            _levelRepository.SetLevelFactory(num => CreateLevel(num, 1, 1, blocks: null));
            await _app.InitializeAsync();
            await _app.LoadLevelAsync(2);

            await _app.LoadNextLevelAsync();

            Assert.AreEqual(1, _app.GameState.CurrentLevelNumber);
        }

        [Test]
        public void Dispose_ClearsEventBusAndLevelCache()
        {
            _levelRepository.SetLevelCount(1);
            _app.Dispose();

            Assert.IsTrue(_eventBus.Cleared);
            Assert.IsTrue(_levelRepository.CacheCleared);
        }

        private static Level CreateLevel(int number, int rows, int cols, IEnumerable<BlockData> blocks)
        {
            var level = new Level(number, rows, cols);
            if (blocks != null)
            {
                level.Blocks.AddRange(blocks);
            }

            return level;
        }

        private class FakePersistence : IPersistenceService
        {
            public GameStateProfileData Saved { get; set; }
            public bool SavedStateExists => Saved != null;

            public UniTask SaveGameStateAsync(GameStateProfileData stateProfileData)
            {
                Saved = stateProfileData;
                return UniTask.CompletedTask;
            }

            public UniTask<GameStateProfileData> LoadGameStateAsync()
            {
                return UniTask.FromResult(Saved);
            }

            public bool HasSavedState() => SavedStateExists;

            public void ClearSavedState() => Saved = null;
        }

        private class FakeEventBus : IGlobalEventBus
        {
            public List<object> Events { get; } = new List<object>();
            public bool Cleared { get; private set; }

            public void Subscribe<TEvent>(System.Action<TEvent> handler) where TEvent : class { }
            public void Unsubscribe<TEvent>(System.Action<TEvent> handler) where TEvent : class { }
            public void Publish<TEvent>(TEvent eventData) where TEvent : class => Events.Add(eventData);
            public void Clear()
            {
                Cleared = true;
                Events.Clear();
            }
        }

        private class FakeGridDataProvider : IGridDataProvider
        {
            public float CellSize => 1f;
            public Vector2 GridOffset => Vector2.zero;
        }

        private class FakeLogger : ILoggerService
        {
            public LogLevel MinimumLevel { get; set; }
            public bool IsEnabled { get; private set; } = true;
            public List<string> Messages { get; } = new List<string>();

            public void SetEnabled(bool enabled) => IsEnabled = enabled;

            public void Log(LogLevel level, string message, Object context = null, System.Exception exception = null)
            {
                Messages.Add($"[{level}] {message}");
            }
        }

        private class FakeLevelRepository : ILevelRepository
        {
            private System.Func<int, Level> _factory = _ => null;

            public int LevelCount { get; private set; }
            public bool CacheCleared { get; private set; }

            public void SetLevelCount(int count) => LevelCount = count;
            public void SetLevelFactory(System.Func<int, Level> factory) => _factory = factory;

            public UniTask InitializeAsync() => UniTask.CompletedTask;

            public UniTask<Level> LoadLevelAsync(int levelNumber) => UniTask.FromResult(_factory(levelNumber));

            public void ClearCache() { CacheCleared = true; }
        }
    }
}
