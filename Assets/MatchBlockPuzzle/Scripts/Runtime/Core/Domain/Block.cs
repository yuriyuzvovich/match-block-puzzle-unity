namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Represents a block in the game grid.
    /// Domain entity - pure logic, no Unity dependencies.
    /// </summary>
    public class Block
    {
        public long Id { get; private set; }
        public BlockTypeId Type { get; private set; }
        public GridPosition Position { get; set; }

        /// <summary>
        /// Indicates if the block is currently falling
        /// </summary>
        public bool IsFalling { get; set; }

        /// <summary>
        /// Indicates if the block is being destroyed
        /// </summary>
        public bool IsBeingDestroyed { get; set; }

        /// <summary>
        /// Indicates if the block can be interacted with (not falling, not being destroyed)
        /// </summary>
        public bool CanInteract => !IsFalling && !IsBeingDestroyed;

        public Block(
            BlockTypeId type,
            GridPosition position
        ) : this(
            BlockIdGenerator.NextId(),
            type,
            position
        )
        {
        }

        /// <summary>
        /// Constructor for deserialization (with specific ID)
        /// </summary>
        public Block(long id, BlockTypeId type, GridPosition position)
        {
            Id = id;
            BlockIdGenerator.EnsureAtLeast(id);
            Type = type;
            Position = position;
            IsFalling = false;
            IsBeingDestroyed = false;
        }

        public override string ToString()
        {
            return $"Block[{Type}] at {Position}";
        }
    }
}