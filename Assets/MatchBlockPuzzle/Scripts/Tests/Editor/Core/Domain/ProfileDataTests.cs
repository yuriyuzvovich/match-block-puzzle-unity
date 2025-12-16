using MatchPuzzle.Core.Domain;
using NUnit.Framework;

namespace MatchPuzzle.Tests.Editor.Core.Domain
{
    public class ProfileDataTests
    {
        [Test]
        public void BlockData_StoresValuesAndConvertsToGridPosition()
        {
            var data = new BlockData(new BlockTypeId("Gem"), 2, 3);

            Assert.AreEqual(new BlockTypeId("Gem"), data.Type);
            Assert.AreEqual(2, data.Row);
            Assert.AreEqual(3, data.Column);
            Assert.AreEqual(new GridPosition(2, 3), data.ToGridPosition());
        }

        [Test]
        public void Level_InitializesWithEmptyBlockList()
        {
            var defaultLevel = new Level();
            var configured = new Level(5, 7, 8);

            Assert.IsNotNull(defaultLevel.Blocks);
            Assert.AreEqual(0, defaultLevel.Blocks.Count);
            Assert.AreEqual(5, configured.LevelNumber);
            Assert.AreEqual(7, configured.Rows);
            Assert.AreEqual(8, configured.Columns);
            Assert.IsNotNull(configured.Blocks);
        }

        [Test]
        public void LevelStateProfileData_ExposesMutableProperties()
        {
            var state = new LevelStateProfileData
            {
                LevelNumber = 4,
                Rows = 5,
                Columns = 6,
                Blocks = new[]
                {
                    new BlockStateProfileData(1, new BlockTypeId("A"), 0, 0),
                    new BlockStateProfileData(2, new BlockTypeId("B"), 1, 1)
                }
            };

            Assert.AreEqual(4, state.LevelNumber);
            Assert.AreEqual(5, state.Rows);
            Assert.AreEqual(6, state.Columns);
            Assert.AreEqual(2, state.Blocks.Length);
            Assert.AreEqual(new BlockTypeId("B"), state.Blocks[1].Type);
        }

        [Test]
        public void GameStateProfileData_HoldsCurrentLevelInformation()
        {
            var levelState = new LevelStateProfileData { LevelNumber = 3 };
            var state = new GameStateProfileData
            {
                CurrentLevelNumber = 2,
                CurrentLevelState = levelState
            };

            Assert.AreEqual(2, state.CurrentLevelNumber);
            Assert.AreSame(levelState, state.CurrentLevelState);
        }

        [Test]
        public void BlockStateProfileData_PreservesAssignedValues()
        {
            var blockState = new BlockStateProfileData(10, new BlockTypeId("Gem"), 1, 2);

            Assert.AreEqual(10, blockState.Id);
            Assert.AreEqual(new BlockTypeId("Gem"), blockState.Type);
            Assert.AreEqual(1, blockState.Row);
            Assert.AreEqual(2, blockState.Column);
        }
    }
}
