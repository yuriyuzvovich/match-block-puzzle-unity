using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Bootstrap;
using MatchPuzzle.Infrastructure.Data;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    public sealed class LoggingStep : BootstrapStepBase
    {
        public override string Id => "Logging";
        public override IReadOnlyList<string> DependsOn => new[] { "AssetKeys" };

        public override async UniTask PreRunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var assetService = services.Get<IAssetService>();
            var assetKeys = services.Get<AssetKeys>();
            var loggingSettings = await assetService.LoadAsync<LoggingSettings>(assetKeys.LoggingSettingsKey);
            services.Register(loggingSettings);
        }

        public override UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<LoggingSettings>(out var loggingSettings);

            var minimumLevel = loggingSettings ? loggingSettings.MinimumLevel : LogLevel.Information;
            var isEnabled = ShouldEnableLogging(loggingSettings);

            var logger = new UnityLoggerService(minimumLevel, Debug.unityLogger, isEnabled);
            services.Register<ILoggerService>(logger);

            logger.LogInformation("[Bootstrap] Logging configured.");
            return UniTask.CompletedTask;
        }

        private static bool ShouldEnableLogging(LoggingSettings loggingSettings)
        {
            if (!loggingSettings)
            {
                return Debug.isDebugBuild || Application.isEditor;
            }

            if (loggingSettings.ForceEnable)
            {
                return true;
            }

            if (Application.isEditor)
            {
                return loggingSettings.EnableInEditor;
            }

            if (Debug.isDebugBuild)
            {
                return loggingSettings.EnableInDevelopmentBuild;
            }

            return loggingSettings.EnableInReleaseBuild;
        }
    }
}
