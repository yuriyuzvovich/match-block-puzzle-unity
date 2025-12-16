using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;

namespace MatchPuzzle.Infrastructure.Bootstrap
{
    public sealed class StartGameStep : BootstrapStepBase
    {
        public override string Id => "StartGame";
        public override IReadOnlyList<string> DependsOn => new[] { "Controller" };

        public override async UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<ILoggerService>(out var logger);
            services.TryGet<GameSettings>(out var gameSettings);

            if (!services.TryGet<IMatchPuzzleGameController>(out var gameController))
            {
                throw new InvalidOperationException("Cannot start game - controller not initialized.");
            }

            if (gameSettings.IsAutostartEnabled)
            {
                await gameController.StartGameAsync();
                logger.LogInformation("[Bootstrap] Game started.");
            }
            else
            {
                logger.LogWarning("[Bootstrap] Autostart is disabled - skipping game start.");
            }
        }
    }
}