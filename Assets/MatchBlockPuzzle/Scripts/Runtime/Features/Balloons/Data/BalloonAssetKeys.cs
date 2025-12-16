using UnityEngine;

namespace MatchPuzzle.Features.Balloon
{
    /// <summary>
    /// ScriptableObject containing asset addresses used by the balloon feature.
    /// </summary>
    [CreateAssetMenu(fileName = "BalloonAssetKeys", menuName = "MatchPuzzle/Balloon/Asset Keys")]
    public class BalloonAssetKeys : ScriptableObject
    {
        [Header("Settings")]
        [Tooltip("Address to load the BalloonSettings asset")]
        public string BalloonSettingsKey = "Settings/BalloonSettings";

        [Header("Prefabs")]
        [Tooltip("Address to load the BalloonView prefab")]
        public string BalloonViewPrefabKey = "Prefabs/BalloonView";

        [Tooltip("Address to load the BalloonContainerView prefab")]
        public string BalloonContainerViewPrefabKey = "Prefabs/BalloonContainerView";
    }
}
