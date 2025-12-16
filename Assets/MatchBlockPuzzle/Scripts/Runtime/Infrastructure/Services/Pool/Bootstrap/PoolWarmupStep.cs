using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Bootstrap;
using MatchPuzzle.Infrastructure.Data;

namespace MatchPuzzle.Infrastructure.Services
{
    public sealed class PoolWarmupStep : BootstrapStepBase
    {
        public override string Id => "PoolWarmup";
        public override IReadOnlyList<string> DependsOn => new[] { "Pool", "Settings" };

        public override async UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<ILoggerService>(out var logger);
            var poolingSettings = services.Get<PoolingSettings>();

            if (!poolingSettings || !poolingSettings.EnablePoolWarmup)
            {
                logger?.LogInformation("[Bootstrap] Pool warmup disabled.");
                return;
            }

            var poolService = services.Get<IPoolService>();
            var assetKeys = services.Get<AssetKeys>();

            if (poolingSettings.BlockViewWarmupCount > 0)
            {
                await poolService.WarmupAsync(assetKeys.BlockViewPrefabKey, poolingSettings.BlockViewWarmupCount);
            }

            logger?.LogInformation("[Bootstrap] Pool warmup complete.");
        }
    }
}
