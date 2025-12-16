using System.Collections.Generic;
using System.Linq;
using MatchPuzzle.Core.Domain;
using NUnit.Framework;

namespace MatchPuzzle.Tests.Editor.Core.Domain
{
    public class MatchFinderTests
    {
        private long _idCounter;

        [SetUp]
        public void ResetCounter()
        {
            _idCounter = 0;
        }

        [Test]
        public void FindMatchAreas_FindsHorizontalAndVerticalLines()
        {
            var grid = new Grid(3, 4);
            Place(grid, "A", (0, 0), (0, 1), (0, 2)); // horizontal
            Place(grid, "B", (0, 3), (1, 3), (2, 3)); // vertical

            var finder = new MatchFinder(grid, 3);

            var matches = finder.FindMatchAreas();

            Assert.That(matches, Has.Exactly(1).Matches<HashSet<GridPosition>>(set =>
                set.SetEquals(new[] { new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2) })));

            Assert.That(matches, Has.Exactly(1).Matches<HashSet<GridPosition>>(set =>
                set.SetEquals(new[] { new GridPosition(0, 3), new GridPosition(1, 3), new GridPosition(2, 3) })));
        }

        [Test]
        public void FindMatchAreas_GroupsConnectedMatchesOfSameType()
        {
            var grid = new Grid(3, 3);
            // Cross shape of the same type
            Place(grid, "B", (0, 1), (1, 1), (2, 1));
            Place(grid, "B", (1, 0), (1, 1), (1, 2));

            var finder = new MatchFinder(grid, 3);

            var matches = finder.FindMatchAreas();

            var expectedArea = new[]
            {
                new GridPosition(0, 1),
                new GridPosition(1, 1),
                new GridPosition(2, 1),
                new GridPosition(1, 0),
                new GridPosition(1, 2)
            };

            Assert.AreEqual(1, matches.Count);
            Assert.IsTrue(matches[0].SetEquals(expectedArea));
        }

        private void Place(Grid grid, string type, params (int row, int col)[] coords)
        {
            foreach (var (row, col) in coords.Distinct())
            {
                var position = new GridPosition(row, col);
                grid.SetBlock(position, new Block(++_idCounter, new BlockTypeId(type), position));
            }
        }
    }
}
