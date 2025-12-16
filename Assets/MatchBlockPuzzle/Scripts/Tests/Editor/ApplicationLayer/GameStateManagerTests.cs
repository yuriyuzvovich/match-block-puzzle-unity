using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MatchPuzzle.ApplicationLayer;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using NUnit.Framework;

namespace MatchPuzzle.Tests.Editor.ApplicationLayer
{
    public class GameStateManagerTests
    {
        private FakePersistence _persistence;
        private GameStateManager _state;

        [SetUp]
        public void SetUp()
        {
            _persistence = new FakePersistence();
            _state = new GameStateManager(_persistence);
        }

        [Test]
        public void InitializeLevel_CreatesGridAndBlocks()
        {
            var level = new Level(1, 2, 2);
            level.Blocks.Add(new BlockData(new BlockTypeId("A"), 0, 0));
            level.Blocks.Add(new BlockData(new BlockTypeId("B"), 1, 1));

            _state.InitializeLevel(level);

            Assert.AreEqual(1, _state.CurrentLevelNumber);
            Assert.NotNull(_state.CurrentGrid);
            Assert.AreEqual(2, _state.CurrentGrid.Rows);
            Assert.AreEqual(2, _state.CurrentGrid.Columns);
            Assert.AreEqual(new BlockTypeId("A"), _state.CurrentGrid.GetBlock(new GridPosition(0, 0)).Type);
            Assert.AreEqual(new BlockTypeId("B"), _state.CurrentGrid.GetBlock(new GridPosition(1, 1)).Type);
            Assert.AreEqual(0, _state.MoveCount);
            Assert.IsFalse(_state.IsNormalizing);
        }

        [Test]
        public void RestoreLevelState_RebuildsGridFromProfile()
        {
            var levelState = new LevelStateProfileData
            {
                LevelNumber = 3,
                Rows = 1,
                Columns = 2,
                Blocks = new[]
                {
                    new BlockStateProfileData(10, new BlockTypeId("A"), 0, 0)
                }
            };
            var level = new Level(3, 1, 2);

            _state.RestoreLevelState(levelState, level);

            Assert.AreEqual(3, _state.CurrentLevelNumber);
            Assert.AreEqual(1, _state.CurrentGrid.Rows);
            Assert.AreEqual(2, _state.CurrentGrid.Columns);
            Assert.AreEqual(10, _state.CurrentGrid.GetBlock(new GridPosition(0, 0)).Id);
        }

        [Test]
        public async Task SaveStateAsync_SerializesCurrentGrid()
        {
            var level = new Level(2, 1, 1);
            level.Blocks.Add(new BlockData(new BlockTypeId("A"), 0, 0));
            _state.InitializeLevel(level);

            await _state.SaveStateAsync();

            Assert.NotNull(_persistence.SavedState);
            Assert.AreEqual(2, _persistence.SavedState.CurrentLevelNumber);
            Assert.AreEqual(1, _persistence.SavedState.CurrentLevelState.Blocks.Length);
            var savedBlock = _persistence.SavedState.CurrentLevelState.Blocks.Single();
            Assert.AreEqual(new BlockTypeId("A"), savedBlock.Type);
            Assert.AreEqual(0, savedBlock.Row);
            Assert.AreEqual(0, savedBlock.Column);
        }

        [Test]
        public void IsLevelComplete_ReturnsTrueWhenGridEmpty()
        {
            var level = new Level(1, 1, 1);
            level.Blocks.Add(new BlockData(new BlockTypeId("A"), 0, 0));
            _state.InitializeLevel(level);

            Assert.IsFalse(_state.IsLevelComplete());

            _state.CurrentGrid.Clear();

            Assert.IsTrue(_state.IsLevelComplete());
        }

        [Test]
        public void CreateNormalizationEngine_AttachesToCurrentGrid()
        {
            var level = new Level(1, 1, 1);
            _state.InitializeLevel(level);

            _state.CreateNormalizationEngine(3);

            Assert.NotNull(_state.NormalizationEngine);
        }

        [Test]
        public void MoveCount_CanBeIncrementedAndReset()
        {
            _state.IncrementMoveCount();
            _state.IncrementMoveCount();
            Assert.AreEqual(2, _state.MoveCount);

            _state.ResetMoveCount();
            Assert.AreEqual(0, _state.MoveCount);
        }

        [Test]
        public void HasSavedState_DelegatesToPersistence()
        {
            Assert.IsFalse(_state.HasSavedState());
            _persistence.SaveGameStateAsync(new GameStateProfileData()).Forget();
            Assert.IsTrue(_state.HasSavedState());
        }

        private class FakePersistence : IPersistenceService
        {
            public GameStateProfileData SavedState { get; private set; }
            public bool SavedStateExists => SavedState != null;

            public UniTask SaveGameStateAsync(GameStateProfileData stateProfileData)
            {
                SavedState = stateProfileData;
                return UniTask.CompletedTask;
            }

            public UniTask<GameStateProfileData> LoadGameStateAsync()
            {
                return UniTask.FromResult(SavedState);
            }

            public bool HasSavedState() => SavedStateExists;

            public void ClearSavedState()
            {
                SavedState = null;
            }
        }
    }
}
