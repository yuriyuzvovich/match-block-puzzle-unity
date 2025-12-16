using UnityEngine;

namespace MatchPuzzle.Infrastructure.Data
{
    [CreateAssetMenu(fileName = "AssetKeys", menuName = "MatchPuzzle/Asset Keys")]
    public class AssetKeys : ScriptableObject
    {
        [Header("Settings")]
        [Tooltip("Address to load the GameSettings asset")]
        [SerializeField] private string _gameSettingsKey = "Settings/GameSettings";

        [Tooltip("Address to load the CameraSettings asset")]
        [SerializeField] private string _cameraSettingsKey = "Settings/CameraSettings";

        [Tooltip("Address to load the LoggingSettings asset")]
        [SerializeField] private string _loggingSettingsKey = "Settings/LoggingSettings";

        [Tooltip("Address to load the PersistenceSettings asset")]
        [SerializeField] private string _persistenceSettingsKey = "Settings/PersistenceSettings";

        [Tooltip("Address to load the BlockAnimationSettings asset")]
        [SerializeField] private string _blockAnimationSettingsKey = "Settings/BlockAnimationSettings";

        [Tooltip("Address to load the PoolingSettings asset")]
        [SerializeField] private string _poolingSettingsKey = "Settings/PoolingSettings";

        [Tooltip("Address to load the LevelRepositorySettings asset")]
        [SerializeField] private string _levelRepositorySettingsKey = "Settings/LevelRepositorySettings";

        [Tooltip("Address to load the MatchSettings asset")]
        [SerializeField] private string _matchSettingsKey = "Settings/MatchSettings";

        [Tooltip("Address to load the GridSettings asset")]
        [SerializeField] private string _gridSettingsKey = "Settings/GridSettings";

        [Tooltip("Address to load the InputSettings asset")]
        [SerializeField] private string _inputSettingsKey = "Settings/InputSettings";

        [Header("Prefabs - Views")]
        [Tooltip("Address to load the BlockView prefab")]
        [SerializeField] private string _blockViewPrefabKey = "Prefabs/BlockView";
        [Tooltip("Address to load the GridView prefab")]
        [SerializeField] private string _gridViewPrefabKey = "Prefabs/GridView";

        [Tooltip("Address to load the MatchPuzzleRoot prefab")]
        [SerializeField] private string _matchPuzzleRootPrefabKey = "Prefabs/MatchPuzzleRoot";

        public string GameSettingsKey => _gameSettingsKey;
        public string CameraSettingsKey => _cameraSettingsKey;
        public string LoggingSettingsKey => _loggingSettingsKey;
        public string PersistenceSettingsKey => _persistenceSettingsKey;
        public string BlockAnimationSettingsKey => _blockAnimationSettingsKey;
        public string PoolingSettingsKey => _poolingSettingsKey;
        public string LevelRepositorySettingsKey => _levelRepositorySettingsKey;
        public string MatchSettingsKey => _matchSettingsKey;
        public string GridSettingsKey => _gridSettingsKey;
        public string InputSettingsKey => _inputSettingsKey;
        public string BlockViewPrefabKey => _blockViewPrefabKey;
        public string GridViewPrefabKey => _gridViewPrefabKey;
        public string MatchPuzzleRootPrefabKey => _matchPuzzleRootPrefabKey;
    }
}
