using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace MatchPuzzle.Infrastructure.Bootstrap
{
    /// <summary>
    /// Contract for a bootstrap step that supports three-phase execution.
    /// </summary>
    public interface IBootstrapStep
    {
        string Id { get; }
        IReadOnlyList<string> DependsOn { get; }

        UniTask PreRunAsync(ServiceContainer services, CancellationToken cancellationToken);
        UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken);
        UniTask PostRunAsync(ServiceContainer services, CancellationToken cancellationToken);
    }
}
