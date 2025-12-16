using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.ApplicationLayerLayer;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;
using MatchPuzzle.Infrastructure.Services;
using MatchPuzzle.Infrastructure.Services.LevelRepository;

namespace MatchPuzzle.Infrastructure.Bootstrap
{
    public sealed class MatchPuzzleAppStep : BootstrapStepBase, ICleanupStep
    {
        public override string Id => "AppFacade";
        public override IReadOnlyList<string> DependsOn => new[] { "CoreServices", "Settings" };

        public override async UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<ILoggerService>(out var logger);
            var appService = new MatchPuzzleAppService(
                services.Get<IPersistenceService>(),
                services.Get<IGlobalEventBus>(),
                services.Get<IGridDataProvider>(),
                logger,
                services.Get<MatchSettings>(),
                services.Get<ILevelRepository>()
            );

            services.Register<IMatchPuzzleAppService>(appService);
            await appService.InitializeAsync();

            logger.LogInformation("[Bootstrap] Game facade initialized.");
        }

        public void Cleanup(ServiceContainer services)
        {
            if (services != null && services.TryGet<IMatchPuzzleAppService>(out var facade))
            {
                facade.Dispose();
            }
        }
    }
}