using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.ApplicationLayerLayer;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Features.Background;
using MatchPuzzle.Features.Balloon;
using MatchPuzzle.Features.UI;

namespace MatchPuzzle.Infrastructure.Bootstrap
{
    public sealed class MatchPuzzleGameControllerStep : BootstrapStepBase, ICleanupStep
    {
        public override string Id => "Controller";
        public override IReadOnlyList<string> DependsOn => new[] { "AppFacade", "GridPresentation", "UI", "Background", "Balloon", "Camera", "Pool" };

        public override async UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<ILoggerService>(out var logger);

            if (!services.TryGet<IGridPresenter>(out var gridPresenter) ||
                !services.TryGet<IBackgroundPresenter>(out var backgroundPresenter) ||
                !services.TryGet<IMatchPuzzleUIView>(out var uiView))
            {
                throw new InvalidOperationException("Cannot initialize game controller - presentation or UI not ready.");
            }

            var controller = new MatchPuzzleGameController(
                services.Get<IMatchPuzzleAppService>(),
                services.Get<IInputService>(),
                services.Get<ICameraService>(),
                gridPresenter,
                backgroundPresenter,
                uiView,
                services.Get<IBalloonSpawner>(),
                services.Get<IUnityEventsService>()
            );

            services.Register<IMatchPuzzleGameController>(controller);
            await controller.InitializeAsync();

            logger.LogInformation("[Bootstrap] Game controller initialized.");
        }

        public void Cleanup(ServiceContainer services)
        {
            if (services != null && services.TryGet<MatchPuzzleGameController>(out var controller))
            {
                controller.Dispose();
            }
        }
    }
}
