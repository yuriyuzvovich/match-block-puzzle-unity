using UnityEngine;

namespace MatchPuzzle.Features.Background.Data
{
    [CreateAssetMenu(fileName = "BackgroundAssetKeys", menuName = "MatchPuzzle/Background/Asset Keys")]
    public class BackgroundAssetKeys : ScriptableObject
    {
        [Tooltip("Address to load the BackgroundSettings asset")]
        [SerializeField] private string _backgroundSettingsKey = "Settings/BackgroundSettings";

        [Tooltip("Address to load the BackgroundView prefab")]
        [SerializeField] private string _backgroundViewPrefabKey = "Prefabs/BackgroundView";

        public string BackgroundSettingsKey => _backgroundSettingsKey;
        public string BackgroundViewPrefabKey => _backgroundViewPrefabKey;
    }
}