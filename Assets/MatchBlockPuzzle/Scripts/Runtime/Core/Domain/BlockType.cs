using System;

namespace MatchPuzzle.Core.Domain
{
    /// <summary>
    /// Lightweight identifier for a block type. Designers can add new IDs via ScriptableObjects without code changes.
    /// </summary>
    [Serializable]
    public struct BlockTypeId : IEquatable<BlockTypeId>
    {
        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Internal string value. Empty/null represents "None".
        /// </summary>
        public string Value;

        public bool IsNone => string.IsNullOrEmpty(Normalize(Value));

        public static BlockTypeId None => default;

        public BlockTypeId(string value)
        {
            Value = Normalize(value);
        }

        public static BlockTypeId From(string value) => new BlockTypeId(value);

        public override string ToString() => IsNone ? "None" : Value;

        public override bool Equals(object obj) => obj is BlockTypeId other && Equals(other);

        public bool Equals(BlockTypeId other) => Comparer.Equals(Normalize(Value), Normalize(other.Value));

        public override int GetHashCode() => Comparer.GetHashCode(Normalize(Value));

        public static bool operator ==(BlockTypeId left, BlockTypeId right) => left.Equals(right);

        public static bool operator !=(BlockTypeId left, BlockTypeId right) => !(left == right);

        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
