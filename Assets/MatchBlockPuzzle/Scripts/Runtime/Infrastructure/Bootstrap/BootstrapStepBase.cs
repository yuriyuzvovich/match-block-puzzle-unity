using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MatchPuzzle.Infrastructure.Bootstrap
{
    public abstract class BootstrapStepBase : IBootstrapStep
    {
        public abstract string Id { get; }
        public virtual IReadOnlyList<string> DependsOn => Array.Empty<string>();

        public virtual UniTask PreRunAsync(ServiceContainer services, CancellationToken cancellationToken) => UniTask.CompletedTask;
        public virtual UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken) => UniTask.CompletedTask;
        public virtual UniTask PostRunAsync(ServiceContainer services, CancellationToken cancellationToken) => UniTask.CompletedTask;
    }
}
