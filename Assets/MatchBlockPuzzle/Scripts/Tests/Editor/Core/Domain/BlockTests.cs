using System.Reflection;
using MatchPuzzle.Core.Domain;
using NUnit.Framework;

namespace MatchPuzzle.Tests.Editor.Core.Domain
{
    public class BlockTests
    {
        private static readonly FieldInfo NextIdField = typeof(Block)
            .Assembly
            .GetType("MatchPuzzle.Core.Domain.BlockIdGenerator")
            ?.GetField("_nextId", BindingFlags.Static | BindingFlags.NonPublic);

        [SetUp]
        public void ResetIdGenerator()
        {
            Assert.NotNull(NextIdField, "Failed to locate BlockIdGenerator._nextId via reflection");
            // Keep ids deterministic for assertions
            NextIdField?.SetValue(null, 0L);
        }

        [Test]
        public void Constructor_AssignsIdAndDefaults()
        {
            var block = new Block(new BlockTypeId("Gem"), new GridPosition(0, 0));

            Assert.Greater(block.Id, 0);
            Assert.AreEqual(new BlockTypeId("Gem"), block.Type);
            Assert.AreEqual(new GridPosition(0, 0), block.Position);
            Assert.IsFalse(block.IsFalling);
            Assert.IsFalse(block.IsBeingDestroyed);
            Assert.IsTrue(block.CanInteract);
        }

        [Test]
        public void Constructor_WithExplicitId_AdvancesGenerator()
        {
            var seeded = new Block(10, new BlockTypeId("Gem"), new GridPosition(1, 1));
            var next = new Block(new BlockTypeId("Gem"), new GridPosition(2, 2));

            Assert.AreEqual(10, seeded.Id);
            Assert.Greater(next.Id, seeded.Id);
        }

        [Test]
        public void CanInteract_ReflectsFallingOrDestroying()
        {
            var block = new Block(new BlockTypeId("Gem"), new GridPosition(0, 0))
            {
                IsFalling = true
            };

            Assert.IsFalse(block.CanInteract);

            block.IsFalling = false;
            block.IsBeingDestroyed = true;

            Assert.IsFalse(block.CanInteract);
        }

        [Test]
        public void ToString_IncludesTypeAndPosition()
        {
            var block = new Block(5, new BlockTypeId("Coin"), new GridPosition(2, 3));

            Assert.AreEqual("Block[Coin] at (2, 3)", block.ToString());
        }
    }
}
