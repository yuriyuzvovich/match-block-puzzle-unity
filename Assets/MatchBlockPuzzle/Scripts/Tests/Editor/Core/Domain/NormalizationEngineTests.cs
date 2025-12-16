using System.Collections.Generic;
using MatchPuzzle.Core.Domain;
using NUnit.Framework;

namespace MatchPuzzle.Tests.Editor.Core.Domain
{
    public class NormalizationEngineTests
    {
        [Test]
        public void Normalize_AppliesGravityUntilSettled()
        {
            var grid = new Grid(3, 1);
            var block = new Block(1, new BlockTypeId("A"), new GridPosition(2, 0));
            grid.SetBlock(block.Position, block);

            var engine = new NormalizationEngine(grid, 3);

            var result = engine.Normalize();

            Assert.AreEqual(new GridPosition(0, 0), block.Position);
            Assert.AreEqual(2, result.Moves.Count);
            Assert.AreEqual(new GridPosition(2, 0), result.Moves[0].From);
            Assert.AreEqual(new GridPosition(1, 0), result.Moves[0].To);
            Assert.AreEqual(new GridPosition(1, 0), result.Moves[1].From);
            Assert.AreEqual(new GridPosition(0, 0), result.Moves[1].To);
            Assert.IsEmpty(result.MatchedAreas);
            Assert.IsTrue(result.HasChanges);
        }

        [Test]
        public void Normalize_FindsMatchesAfterGravity()
        {
            var grid = new Grid(2, 3);
            // Top row blocks will fall to create a horizontal match on the bottom row
            grid.SetBlock(new GridPosition(1, 0), new Block(1, new BlockTypeId("X"), new GridPosition(1, 0)));
            grid.SetBlock(new GridPosition(1, 1), new Block(2, new BlockTypeId("X"), new GridPosition(1, 1)));
            grid.SetBlock(new GridPosition(1, 2), new Block(3, new BlockTypeId("X"), new GridPosition(1, 2)));

            var engine = new NormalizationEngine(grid, 3);

            var result = engine.Normalize();

            var expectedMatch = new HashSet<GridPosition>
            {
                new GridPosition(0, 0),
                new GridPosition(0, 1),
                new GridPosition(0, 2),
            };

            Assert.AreEqual(new GridPosition(0, 0), grid.GetBlock(new GridPosition(0, 0)).Position);
            Assert.AreEqual(new GridPosition(0, 1), grid.GetBlock(new GridPosition(0, 1)).Position);
            Assert.AreEqual(new GridPosition(0, 2), grid.GetBlock(new GridPosition(0, 2)).Position);
            Assert.That(result.MatchedAreas, Has.Exactly(1).Matches<HashSet<GridPosition>>(area => area.SetEquals(expectedMatch)));
            Assert.IsTrue(result.HasChanges);
        }

        [Test]
        public void DestroyBlocks_RemovesProvidedPositions()
        {
            var grid = new Grid(2, 2);
            var a = new Block(1, new BlockTypeId("A"), new GridPosition(0, 0));
            var b = new Block(2, new BlockTypeId("B"), new GridPosition(0, 1));
            grid.SetBlock(a.Position, a);
            grid.SetBlock(b.Position, b);

            var engine = new NormalizationEngine(grid, 3);
            var areas = new List<HashSet<GridPosition>>
            {
                new HashSet<GridPosition> { new GridPosition(0, 0), new GridPosition(0, 1) }
            };

            engine.DestroyBlocks(areas);

            Assert.IsTrue(grid.IsEmpty(new GridPosition(0, 0)));
            Assert.IsTrue(grid.IsEmpty(new GridPosition(0, 1)));
            Assert.IsTrue(grid.IsGridEmpty());
        }

        [Test]
        public void NormalizationResult_ReportsChangesOnlyWhenPresent()
        {
            var result = new NormalizationResult();
            Assert.IsFalse(result.HasChanges);

            result.Moves.Add(new BlockMove(new GridPosition(0, 0), new GridPosition(1, 0)));
            Assert.IsTrue(result.HasChanges);
        }
    }
}
