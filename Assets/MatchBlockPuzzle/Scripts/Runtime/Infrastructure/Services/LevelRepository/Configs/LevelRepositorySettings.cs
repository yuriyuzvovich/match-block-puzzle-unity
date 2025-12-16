using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services.LevelRepository
{
    [CreateAssetMenu(fileName = "LevelRepositorySettings", menuName = "MatchPuzzle/Level Repository Settings")]
    public class LevelRepositorySettings : ScriptableObject
    {
        [Tooltip("Address to load the LevelConfiguration asset")]
        public string LevelConfigurationKey = "Levels/LevelConfiguration";

        [Tooltip("Cache size for level objects in the repository")]
        public int CacheSize = 3;
    }
}
