using MatchPuzzle.Core.Domain;
using NUnit.Framework;

namespace MatchPuzzle.Tests.Editor.Core.Domain
{
    public class BlockTypeIdTests
    {
        [Test]
        public void Equals_IgnoresCaseAndWhitespace()
        {
            var withWhitespace = new BlockTypeId("  Bomb ");
            var differentCase = BlockTypeId.From("bomb");

            Assert.AreEqual(withWhitespace, differentCase);
            Assert.AreEqual(withWhitespace.GetHashCode(), differentCase.GetHashCode());
            Assert.AreEqual("Bomb", withWhitespace.ToString());
        }

        [Test]
        public void None_WhenValueIsEmptyOrWhitespace()
        {
            var empty = new BlockTypeId(string.Empty);
            var spaced = BlockTypeId.From("   ");

            Assert.IsTrue(empty.IsNone);
            Assert.IsTrue(spaced.IsNone);
            Assert.AreEqual(BlockTypeId.None, empty);
            Assert.AreEqual("None", spaced.ToString());
        }
    }

    public class GridPositionTests
    {
        [Test]
        public void DirectionHelpers_MoveInExpectedDirections()
        {
            var position = new GridPosition(2, 3);

            Assert.AreEqual(new GridPosition(3, 3), position.Up());
            Assert.AreEqual(new GridPosition(1, 3), position.Down());
            Assert.AreEqual(new GridPosition(2, 2), position.Left());
            Assert.AreEqual(new GridPosition(2, 4), position.Right());
        }

        [Test]
        public void EqualityAndHashCode_UseRowAndColumn()
        {
            var a = new GridPosition(1, 2);
            var b = new GridPosition(1, 2);
            var c = new GridPosition(2, 1);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a == c);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), c.GetHashCode());
        }

        [Test]
        public void ToString_ShowsCoordinates()
        {
            Assert.AreEqual("(1, 2)", new GridPosition(1, 2).ToString());
        }
    }
}
