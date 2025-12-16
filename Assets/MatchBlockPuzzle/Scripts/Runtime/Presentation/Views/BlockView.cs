using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MatchPuzzle.Core.Domain;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;
using MatchPuzzle.Infrastructure.Extensions;
using UnityEngine;

namespace MatchPuzzle.Runtime.Presentation
{
    /// <summary>
    /// Visual representation of a block
    /// </summary>
    public class BlockView : MonoBehaviour, MatchPuzzle.Core.Interfaces.IPoolObject
    {
        public Block Block { get; private set; }

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;

        private BlockAnimationSettings _animationSettings;
        private IGridDataProvider _gridDataProvider;

        private static readonly int DestroyAnimationName = Animator.StringToHash("Destroy");

        private void Awake()
        {
            if (!_spriteRenderer) throw new MissingComponentException($"SpriteRenderer is not assigned in {nameof(BlockView)}");
            if (!_animator) throw new MissingComponentException($"Animator is not assigned in {nameof(BlockView)}");
        }

        public void Initialize(
            Block block,
            BlockTypeData typeData,
            BlockAnimationSettings animationSettings,
            IGridDataProvider gridDataProvider
        )
        {
            Block = block;
            _animationSettings = animationSettings ?? throw new ArgumentNullException(nameof(animationSettings));
            _gridDataProvider = gridDataProvider ?? throw new ArgumentNullException(nameof(gridDataProvider));

            _spriteRenderer.sprite = typeData.IdleSprite;

            if (typeData.AnimatorController)
            {
                _animator.runtimeAnimatorController = typeData.AnimatorController;
            }

            // Scale the view to fit the grid's cell size while preserving aspect ratio
            try
            {
                var cellSize = _gridDataProvider?.CellSize ?? 0f;
                var sprite = _spriteRenderer.sprite;
                if (sprite != null && cellSize > 0f)
                {
                    var spriteSize = sprite.bounds.size;
                    var maxDimension = Mathf.Max(spriteSize.x, spriteSize.y);
                    if (maxDimension > 0f)
                    {
                        transform.localScale = Vector3.one * cellSize;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"BlockView: failed to scale to cell size - {ex.Message}", this);
            }

            UpdateSortingOrder();
            UpdatePosition(false);
            _animator.speed = UnityEngine.Random.Range(_animationSettings.AnimationSpeedMin, _animationSettings.AnimationSpeedMax);
        }

        private void UpdateSortingOrder()
        {
            if (Block == null)
                return;

            // Sorting: row * 1000 + column
            // This ensures left-to-right, bottom-to-top sorting
            _spriteRenderer.sortingOrder = Block.Position.Row * 1000 + Block.Position.Column;
        }

        /// <summary>
        /// Updates the visual position based on block's grid position
        /// </summary>
        private void UpdatePosition(bool animate)
        {
            if (Block == null)
                return;

            var targetPosition = GetWorldPosition(Block.Position);

            if (animate)
            {
                transform
                    .DOMove(targetPosition, _animationSettings.BlockMoveDurationSec)
                    .SetEase(_animationSettings.BlockMoveEase);
            }
            else
            {
                transform.position = targetPosition;
            }
        }

        /// <summary>
        /// Animates the block moving to a new position
        /// </summary>
        public async UniTask AnimateMoveAsync(GridPosition from, GridPosition to)
        {
            // Kill any existing tweens on this transform
            transform.DOKill();

            var fromWorld = GetWorldPosition(from);
            var toWorld = GetWorldPosition(to);

            transform.position = fromWorld;

            // Refresh sorting order shortly after movement begins so crossing blocks render correctly
            var earlySortingUpdateTask = UpdateSortingOrderWithDelayAsync();

            var tween = transform
                .DOMove(toWorld, _animationSettings.BlockMoveDurationSec)
                .SetEase(_animationSettings.BlockMoveEase);

            await UniTask.WhenAll(tween.ToUniTask(), earlySortingUpdateTask);

            UpdateSortingOrder();
        }

        /// <summary>
        /// Animates the block falling
        /// </summary>
        public async UniTask AnimateFallAsync(GridPosition to)
        {
            // Kill any existing tweens on this transform
            transform.DOKill();

            var toWorld = GetWorldPosition(to);

            var tween = transform
                .DOMove(toWorld, _animationSettings.BlockFallDurationSec)
                .SetEase(_animationSettings.BlockFallEase);

            await tween.ToUniTask();

            UpdateSortingOrder();
        }

        /// <summary>
        /// Animates the block destruction.
        /// Note: Do NOT destroy the object - it will be returned to the pool by the caller.
        /// </summary>
        public async UniTask AnimateDestroyAsync()
        {
            // Play destroy animation
            _animator.speed = 1f / _animationSettings.BlockDestroyDurationSec;
            _animator.Play(DestroyAnimationName, 0, 0f);

            // Wait for animation duration
            await UniTask.Delay((int) (_animationSettings.BlockDestroyDurationSec * 1000));
        }

        /// <summary>
        /// Resets the block view state when returning to pool.
        /// Called by the pool service before deactivating.
        /// </summary>
        public void ResetState()
        {
            Block = null;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
            _spriteRenderer.sprite = null;
            _spriteRenderer.sortingOrder = 0;
            _animationSettings = null;

            // Kill any active tweens on this object
            transform.DOKill();
        }

        /// <summary>
        /// Converts grid position to world position
        /// Grid bounds' bottom-left corner is at GridOffset.
        /// Bottom-left cell (0,0) center is at (GridOffset.x + CellSize/2, GridOffset.y + CellSize/2)
        /// </summary>
        private Vector3 GetWorldPosition(GridPosition gridPosition)
        {
            var x = _gridDataProvider.GridOffset.x + (gridPosition.Column + 0.5f) * _gridDataProvider.CellSize;
            var y = _gridDataProvider.GridOffset.y + (gridPosition.Row + 0.5f) * _gridDataProvider.CellSize;
            return new Vector3(x, y, 0);
        }

        private UniTask UpdateSortingOrderWithDelayAsync()
        {
            // If settings or animation settings aren't loaded, do an immediate update
            if (_animationSettings == null)
            {
                UpdateSortingOrder();
                return UniTask.CompletedTask;
            }

            // Prevent the delay from exceeding the move duration to avoid stalling the command queue
            var delaySeconds = Mathf.Clamp(_animationSettings.SortingOrderUpdateDelaySec, 0f, _animationSettings.BlockMoveDurationSec);

            if (delaySeconds <= 0f)
            {
                UpdateSortingOrder();
                return UniTask.CompletedTask;
            }

            return UpdateSortingOrderAfterDelayAsync(delaySeconds);
        }

        private async UniTask UpdateSortingOrderAfterDelayAsync(float delaySeconds)
        {
            var delayMs = Mathf.RoundToInt(delaySeconds * 1000f);
            await UniTask.Delay(delayMs);
            UpdateSortingOrder();
        }
    }
}