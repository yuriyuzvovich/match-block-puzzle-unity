using UnityEngine;

namespace MatchPuzzle.Infrastructure.Data
{
    [CreateAssetMenu(fileName = "MatchSettings", menuName = "MatchPuzzle/Match Settings")]
    public class MatchSettings : ScriptableObject
    {
        [Header("Matching")]
        [SerializeField, Min(2)] private int _minMatchLength = 3;

        [Header("Timing (milliseconds)")]
        [Tooltip("Delay after matched blocks are destroyed before running the next normalization cycle.")]
        [SerializeField, Min(0)] private int _postMatchDelayMs = 100;

        [Tooltip("Delay before auto-loading the next level after normalization finishes and the level is cleared.")]
        [SerializeField, Min(0)] private int _postNormalizationNextLevelDelayMs = 500;

        public int MinMatchLength => Mathf.Max(2, _minMatchLength);
        public int PostMatchDelayMs => Mathf.Max(0, _postMatchDelayMs);
        public int PostNormalizationNextLevelDelayMs => Mathf.Max(0, _postNormalizationNextLevelDelayMs);
    }
}