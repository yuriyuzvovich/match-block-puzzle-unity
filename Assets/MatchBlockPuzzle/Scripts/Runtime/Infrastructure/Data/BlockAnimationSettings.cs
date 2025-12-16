using DG.Tweening;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Data
{
    [CreateAssetMenu(fileName = "BlockAnimationSettings", menuName = "MatchPuzzle/Block Animation Settings")]
    public class BlockAnimationSettings : ScriptableObject
    {
        [Header("Animation Timings")]
        [SerializeField] private float _blockMoveDurationSec = 0.3f;
        [SerializeField] private float _blockFallDurationSec = 0.2f;
        [SerializeField] private float _blockDestroyDurationSec = 0.5f;
        [SerializeField] private float _animationSpeedMin = 0.8f;
        [SerializeField] private float _animationSpeedMax = 1.2f;

        [Header("Rendering")]
        [Tooltip("Delay (seconds) after a move starts before refreshing sprite sorting order")]
        [SerializeField] private float _sortingOrderUpdateDelaySec = 0.05f;

        [Header("Easing")]
        [SerializeField] private Ease _blockFallEase = Ease.InQuad;
        [SerializeField] private Ease _blockMoveEase = Ease.OutQuad;

        public float SortingOrderUpdateDelaySec => _sortingOrderUpdateDelaySec;
        public float BlockMoveDurationSec => _blockMoveDurationSec;
        public float BlockFallDurationSec => _blockFallDurationSec;
        public float BlockDestroyDurationSec => _blockDestroyDurationSec;
        public float AnimationSpeedMin => _animationSpeedMin;
        public float AnimationSpeedMax => _animationSpeedMax;
        public Ease BlockFallEase => _blockFallEase;
        public Ease BlockMoveEase => _blockMoveEase;
    }
}