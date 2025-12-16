using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Runtime.Presentation;
using UnityEngine;
using Grid = MatchPuzzle.Core.Domain.Grid;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Presenter that drives the grid view and manages block view lifecycle.
    /// </summary>
    public interface IGridPresenter
    {
        void Initialize();

        UniTask<BlockView> CreateBlockViewAsync(Block block);

        BlockView GetBlockView(Block block);

        void RemoveBlockView(Block block);

        void ClearAllBlockViews();

        UniTask CreateAllBlockViewsAsync(Grid grid);

        GridPosition ScreenToGridPosition(Vector2 screenPosition, Camera camera);

        GridPosition WorldToGridPosition(Vector3 worldPosition);

        Vector3 GridToWorldPosition(GridPosition gridPosition);
    }
}
