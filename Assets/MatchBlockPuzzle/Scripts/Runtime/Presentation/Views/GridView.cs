using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Runtime.Presentation
{
    /// <summary>
    /// Dumb grid view; all logic lives in GridPresenter.
    /// </summary>
    public class GridView : MonoBehaviour, IGridView
    {
        [SerializeField] private Transform _blocksRoot;

        public Transform RootTransform => _blocksRoot;

        private void Awake()
        {
            if (!_blocksRoot) throw new System.Exception($"Blocks Root is not assigned in {nameof(GridView)} on GameObject '{gameObject.name}'");
        }
    }
}