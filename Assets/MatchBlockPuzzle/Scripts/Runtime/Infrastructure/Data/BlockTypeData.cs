using MatchPuzzle.Core.Domain;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Data
{
    /// <summary>
    /// ScriptableObject that defines visual and behavioral properties of a block type
    /// </summary>
    [CreateAssetMenu(fileName = "BlockTypeData", menuName = "MatchPuzzle/Block Type Data")]
    public class BlockTypeData : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique ID used in code and saved data (e.g. fire, water).")]
        [SerializeField] private string _id = "fire";

        [Tooltip("Designer-friendly name shown in tools.")]
        [SerializeField] private string _displayName = "Fire";

        [Tooltip("Short label for editor grid buttons.")]
        [SerializeField] private string _shortLabel = "F";

        [Tooltip("Color shown in editor tools.")]
        [SerializeField] private Color _editorColor = Color.gray;

        [Header("Visual")]
        [SerializeField] private Sprite _idleSprite;
        [SerializeField] private RuntimeAnimatorController _animatorController;

        [Header("Sorting")]
        [SerializeField] private int _baseSortingOrder = 0;
        public BlockTypeId TypeId => BlockTypeId.From(Id);

        private void OnValidate()
        {
            // Normalize ID to trimmed lowercase to avoid duplicate variants.
            _id = string.IsNullOrWhiteSpace(_id) ? string.Empty : _id.Trim().ToLowerInvariant();
        }

        public string Id => _id;
        public string DisplayName => _displayName;
        public string ShortLabel => _shortLabel;
        public Color EditorColor => _editorColor;
        public Sprite IdleSprite => _idleSprite;
        public RuntimeAnimatorController AnimatorController => _animatorController;
        public int BaseSortingOrder => _baseSortingOrder;
    }
}