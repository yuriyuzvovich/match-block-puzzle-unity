using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.ApplicationLayerLayer.EventBus;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;
using MatchPuzzle.Infrastructure.Services;

namespace MatchPuzzle.Infrastructure.Bootstrap
{
    public sealed class CoreServicesStep : BootstrapStepBase
    {
        private const string GAME_STATE_KEY = "MatchPuzzle_GameState";

        private PersistenceSettings _persistenceSettings;
        public override string Id => "CoreServices";
        public override IReadOnlyList<string> DependsOn => new[] { "Settings" };

        public override async UniTask PreRunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var assetKeys = services.Get<AssetKeys>();
            _persistenceSettings = await services.Get<IAssetService>().LoadAsync<PersistenceSettings>(assetKeys.PersistenceSettingsKey);
            if (!_persistenceSettings)
            {
                services.TryGet<ILoggerService>(out var logger);
                logger.LogInformation("[Bootstrap] PersistenceSettings not found. Using defaults.");
            }

            services.Register(_persistenceSettings);
        }

        public override UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var persistenceService = new PersistenceBuilder()
                .WithGameStateKey(GAME_STATE_KEY)
                .WithStorage(new PlayerPrefsStorage())
                .WithParser(_persistenceSettings.ResolveParser())
                .WithLogger(services.Get<ILoggerService>())
                .Build();

            services.Register<IPersistenceService>(persistenceService);

            var eventBus = new GlobalEventBus();
            services.Register<IGlobalEventBus>(eventBus);

            var unityEvents = new UnityEventsService();
            services.Register<IUnityEventsService>(unityEvents);

            var inputSettings = services.Get<PlayerInputSettings>();
            var inputService = CreatePlatformInputService(inputSettings);
            services.Register<IInputService>(inputService);

            services.TryGet<ILoggerService>(out var logger);
            logger.LogInformation("[Bootstrap] Core services initialized.");
            return UniTask.CompletedTask;
        }

        private IInputService CreatePlatformInputService(PlayerInputSettings inputSettings)
        {
#if UNITY_EDITOR
            // Use hybrid service in Editor to support both Game view (mouse) and Device Simulator (touch)
            return new EditorHybridInputService(inputSettings);
#else
            return UnityEngine.Application.isMobilePlatform
                ? new MobileUnityPackageInputService(inputSettings)
                : new DesktopUnityPackageInputService(inputSettings);
#endif
        }
    }
}