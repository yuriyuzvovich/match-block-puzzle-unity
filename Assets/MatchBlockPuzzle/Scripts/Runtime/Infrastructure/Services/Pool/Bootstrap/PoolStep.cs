using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Bootstrap;

namespace MatchPuzzle.Infrastructure.Services
{
    public sealed class PoolStep : BootstrapStepBase, ICleanupStep
    {
        public override string Id => "Pool";
        public override IReadOnlyList<string> DependsOn => new[] { "Logging", "AssetService" };

        public override UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<ILoggerService>(out var logger);

            var poolService = new PoolService(services.Get<IAssetService>(), logger);
            services.Register<IPoolService>(poolService);

            logger.LogInformation("[Bootstrap] Pool service initialized.");
            return UniTask.CompletedTask;
        }

        public void Cleanup(ServiceContainer services)
        {
            if (services != null && services.TryGet<IPoolService>(out var poolService))
            {
                poolService.ClearAll();
            }
        }
    }
}