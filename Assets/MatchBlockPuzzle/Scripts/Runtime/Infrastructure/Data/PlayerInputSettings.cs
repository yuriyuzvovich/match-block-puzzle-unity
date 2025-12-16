using UnityEngine;

namespace MatchPuzzle.Infrastructure.Data
{
    /// <summary>
    /// ScriptableObject containing configurable input settings.
    /// </summary>
    [CreateAssetMenu(fileName = "InputSettings", menuName = "MatchPuzzle/Input Settings")]
    public class PlayerInputSettings : ScriptableObject
    {
        [Header("Swipe")]
        [Tooltip("Minimum distance (in screen pixels) required for a drag to be treated as a swipe.")]
        [Min(0f)][SerializeField] private float _minSwipeDistance = 12f;

        [Tooltip("If true, drags shorter than MinSwipeDistance will trigger a tap event.")]
        [SerializeField] private bool _triggerTapOnShortSwipe = true;

        public bool TriggerTapOnShortSwipe => _triggerTapOnShortSwipe;
        public float MinSwipeDistance => _minSwipeDistance;
    }
}
