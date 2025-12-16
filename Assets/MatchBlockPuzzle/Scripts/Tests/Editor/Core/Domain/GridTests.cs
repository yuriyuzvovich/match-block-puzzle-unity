using MatchPuzzle.Core.Domain;
using NUnit.Framework;

namespace MatchPuzzle.Tests.Editor.Core.Domain
{
    public class GridTests
    {
        [SetUp]
        public void ResetIds()
        {
            BlockIdGeneratorForTest.Reset();
        }

        private static Block CreateBlock(string type, int row, int column, long id = 0)
        {
            return new Block(id == 0 ? BlockIdGeneratorForTest.Next() : id, new BlockTypeId(type), new GridPosition(row, column));
        }

        [Test]
        public void IsValidPosition_RespectsBounds()
        {
            var grid = new Grid(2, 3);

            Assert.IsTrue(grid.IsValidPosition(new GridPosition(0, 0)));
            Assert.IsTrue(grid.IsValidPosition(new GridPosition(1, 2)));
            Assert.IsFalse(grid.IsValidPosition(new GridPosition(-1, 0)));
            Assert.IsFalse(grid.IsValidPosition(new GridPosition(2, 1)));
            Assert.IsFalse(grid.IsValidPosition(new GridPosition(1, 3)));
        }

        [Test]
        public void SetBlock_PlacesBlockAndUpdatesPosition()
        {
            var grid = new Grid(2, 2);
            var block = CreateBlock("Gem", 0, 0);

            grid.SetBlock(new GridPosition(1, 1), block);

            Assert.AreEqual(block, grid.GetBlock(new GridPosition(1, 1)));
            Assert.AreEqual(new GridPosition(1, 1), block.Position);
        }

        [Test]
        public void SwapBlocks_ExchangesBlocksAndPositions()
        {
            var grid = new Grid(2, 2);
            var left = CreateBlock("Gem", 0, 0, 1);
            var right = CreateBlock("Coin", 0, 1, 2);

            grid.SetBlock(left.Position, left);
            grid.SetBlock(right.Position, right);

            grid.SwapBlocks(left.Position, right.Position);

            Assert.AreEqual(right, grid.GetBlock(new GridPosition(0, 0)));
            Assert.AreEqual(left, grid.GetBlock(new GridPosition(0, 1)));
            Assert.AreEqual(new GridPosition(0, 0), right.Position);
            Assert.AreEqual(new GridPosition(0, 1), left.Position);
        }

        [Test]
        public void SwapBlocks_AllowsMovingIntoEmptyCell()
        {
            var grid = new Grid(3, 1);
            var block = CreateBlock("Gem", 2, 0, 1);

            grid.SetBlock(block.Position, block);

            grid.SwapBlocks(new GridPosition(2, 0), new GridPosition(1, 0));

            Assert.IsNull(grid.GetBlock(new GridPosition(2, 0)));
            Assert.AreEqual(block, grid.GetBlock(new GridPosition(1, 0)));
            Assert.AreEqual(new GridPosition(1, 0), block.Position);
        }

        [Test]
        public void GetNeighbors_ReturnsOnlyValidPositions()
        {
            var grid = new Grid(2, 2);
            var neighbors = grid.GetNeighbors(new GridPosition(0, 0));

            CollectionAssert.AreEquivalent(
                new[] { new GridPosition(1, 0), new GridPosition(0, 1) },
                neighbors);
        }

        [Test]
        public void Clear_RemovesAllBlocks()
        {
            var grid = new Grid(1, 2);
            var block = CreateBlock("Gem", 0, 0);
            grid.SetBlock(block.Position, block);

            grid.Clear();

            Assert.IsTrue(grid.IsGridEmpty());
            Assert.IsTrue(grid.IsEmpty(new GridPosition(0, 0)));
        }

        [Test]
        public void IsGridEmpty_DetectsWhenBlocksExist()
        {
            var grid = new Grid(1, 1);
            Assert.IsTrue(grid.IsGridEmpty());

            grid.SetBlock(new GridPosition(0, 0), CreateBlock("Gem", 0, 0));

            Assert.IsFalse(grid.IsGridEmpty());
        }
    }

    internal static class BlockIdGeneratorForTest
    {
        private static long _next;

        public static void Reset() => _next = 0;
        public static long Next() => ++_next;
    }
}
