using UnityEngine;

namespace MatchPuzzle.Infrastructure.Data
{
    [CreateAssetMenu(fileName = "GridSettings", menuName = "MatchPuzzle/Grid Settings")]
    public class GridSettings : ScriptableObject
    {
        [Header("Grid Configuration")]
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private Vector2 _gridOffset = new Vector2(0, 1f);

        public float CellSize => _cellSize;
        public Vector2 GridOffset => _gridOffset;
    }
}
