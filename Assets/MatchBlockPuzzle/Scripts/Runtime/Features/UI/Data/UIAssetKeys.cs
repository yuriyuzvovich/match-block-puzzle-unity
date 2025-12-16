using UnityEngine;

namespace MatchPuzzle.Features.UI
{
    [CreateAssetMenu(fileName = "UIAssetKeys", menuName = "MatchPuzzle/UI/Asset Keys")]
    public class UIAssetKeys : ScriptableObject
    {
        [Tooltip("Address to load the MatchPuzzleUIView prefab")]
        public string MatchPuzzleUIViewPrefabKey = "Prefabs/MatchPuzzleUIView";
    }
}
