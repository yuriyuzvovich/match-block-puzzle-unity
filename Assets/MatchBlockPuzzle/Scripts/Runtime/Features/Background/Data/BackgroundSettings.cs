using UnityEngine;

namespace MatchPuzzle.Features.Background
{
    /// <summary>
    /// ScriptableObject containing background configuration settings.
    /// Loaded via AssetKeys through IAssetService.
    /// </summary>
    [CreateAssetMenu(fileName = "BackgroundSettings", menuName = "MatchPuzzle/Background Settings")]
    public class BackgroundSettings : ScriptableObject
    {
        [Header("Alignment")]
        [Tooltip("Which side of the background should stick to the border of camera view")]
        [SerializeField] private BackgroundAlign _align = BackgroundAlign.Center;

        public BackgroundAlign Align => _align;
    }
}