using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    /// <summary>
    /// ScriptableObject containing object pooling related settings.
    /// Loaded via AssetKeys through IAssetService.
    /// </summary>
    [CreateAssetMenu(fileName = "PoolingSettings", menuName = "MatchPuzzle/Pooling Settings")]
    public class PoolingSettings : ScriptableObject
    {
        [Header("Pool Warmup")]
        [Tooltip("Whether to warmup object pools at startup")]
        [SerializeField] private bool _enablePoolWarmup = true;

        [Tooltip("How many BlockView objects to warmup in the pool at startup")]
        [SerializeField] private int _blockViewWarmupCount = 50;

        public bool EnablePoolWarmup => _enablePoolWarmup;
        public int BlockViewWarmupCount => _blockViewWarmupCount;
    }
}
