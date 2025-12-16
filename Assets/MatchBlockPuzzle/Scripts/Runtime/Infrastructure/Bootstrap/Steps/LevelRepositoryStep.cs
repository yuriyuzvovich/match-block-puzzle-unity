using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;
using MatchPuzzle.Infrastructure.Services.LevelRepository;

namespace MatchPuzzle.Infrastructure.Bootstrap
{
    public class LevelRepositoryStep : BootstrapStepBase
    {
        public override string Id => "LevelRepository";

        public override UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var assetService = services.Get<IAssetService>();
            services.TryGet<ILoggerService>(out var logger);
            var assetKeys = services.Get<AssetKeys>();
            var levelRepository = new LevelRepository(assetService, logger, assetKeys);

            services.Register<ILevelRepository>(levelRepository);
            return UniTask.CompletedTask;
        }
    }
}