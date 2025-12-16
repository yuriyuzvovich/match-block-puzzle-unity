using System.Threading;

namespace MatchPuzzle.Core.Domain
{
    internal static class BlockIdGenerator
    {
        private static long _nextId;
        public static long NextId() => Interlocked.Increment(ref _nextId);

        /// <summary>
        /// Ensures that the next generated ID will be at least the specified value
        /// </summary>
        public static void EnsureAtLeast(long id)
        {
            long current;
            // Loop until we successfully update _nextId or determine no update is needed
            do
            {
                current = Interlocked.Read(ref _nextId); // Read current value atomically 
                // If current is already >= id, no need to update
                if (id <= current)
                    return;
            }
            // Try to set _nextId to id if it is still current
            // If another thread changed it, retry
            while (Interlocked.CompareExchange(ref _nextId, id, current) != current);
        }
    }
}