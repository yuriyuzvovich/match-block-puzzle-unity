using MatchPuzzle.Core.Domain;

namespace MatchPuzzle.Core.Interfaces
{
    public abstract class GameEvent { }

    public class LevelStartedEvent : GameEvent
    {
        public int LevelNumber { get; }

        public LevelStartedEvent(int levelNumber)
        {
            LevelNumber = levelNumber;
        }
    }

    public class LevelCompletedEvent : GameEvent
    {
        public int LevelNumber { get; }
        public int MoveCount { get; }

        public LevelCompletedEvent(int levelNumber, int moveCount)
        {
            LevelNumber = levelNumber;
            MoveCount = moveCount;
        }
    }

    public class MoveExecutedEvent : GameEvent
    {
        public GridPosition BlockPosition { get; }
        public Direction Direction { get; }

        public MoveExecutedEvent(GridPosition blockPosition, Direction direction)
        {
            BlockPosition = blockPosition;
            Direction = direction;
        }
    }

    public class BlocksMatchedEvent : GameEvent
    {
        public int BlockCount { get; }

        public BlocksMatchedEvent(int blockCount)
        {
            BlockCount = blockCount;
        }
    }

    public class BlocksDestroyedEvent : GameEvent
    {
        public int BlockCount { get; }

        public BlocksDestroyedEvent(int blockCount)
        {
            BlockCount = blockCount;
        }
    }

    public class InvalidMoveAttemptEvent : GameEvent
    {
        public GridPosition BlockPosition { get; }
        public Direction Direction { get; }
        public string Reason { get; }

        public InvalidMoveAttemptEvent(GridPosition blockPosition, Direction direction, string reason)
        {
            BlockPosition = blockPosition;
            Direction = direction;
            Reason = reason;
        }
    }
}
