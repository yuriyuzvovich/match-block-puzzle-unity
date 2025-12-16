using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Bootstrap;

namespace MatchPuzzle.Infrastructure.Services
{
    public sealed class AssetServiceStep : BootstrapStepBase
    {
        public override string Id => "AssetService";

        public override UniTask PreRunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var assetService = new ResourcesAssetService();
            services.Register<IAssetService>(assetService);

            services.TryGet<ILoggerService>(out var logger);
            logger.LogInformation("[Bootstrap] Asset service registered.");
            return UniTask.CompletedTask;
        }

        public override UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken) => UniTask.CompletedTask;
    }
}