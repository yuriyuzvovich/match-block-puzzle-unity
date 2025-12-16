using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;

namespace MatchPuzzle.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods to integrate DOTween with UniTask
    /// </summary>
    public static class DOTweenUniTaskExtensions
    {
        /// <summary>
        /// Converts a DOTween Tween to a UniTask
        /// </summary>
        public static async UniTask ToUniTask(this Tween tween, CancellationToken cancellationToken = default)
        {
            if (tween == null || !tween.active)
                return;

            // Use UniTask.Yield to wait for the tween to complete
            while (tween.active && !tween.IsComplete())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }
        }
    }
}
