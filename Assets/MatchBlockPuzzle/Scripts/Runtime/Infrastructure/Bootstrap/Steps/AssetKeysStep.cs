using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Infrastructure.Data;

namespace MatchPuzzle.Infrastructure.Bootstrap
{
    public sealed class AssetKeysStep : BootstrapStepBase
    {
        private readonly string _assetKeysKey;

        public AssetKeysStep(string assetKeysKey)
        {
            _assetKeysKey = assetKeysKey ?? throw new ArgumentNullException(nameof(assetKeysKey));
        }

        public override string Id => "AssetKeys";
        public override IReadOnlyList<string> DependsOn => new[] { "AssetService" };

        public override async UniTask PreRunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            var assetService = services.Get<IAssetService>();
            var assetKeys = await assetService.LoadAsync<AssetKeys>(_assetKeysKey);
            if (!assetKeys)
            {
                throw new InvalidOperationException($"Failed to load AssetKeys from key: {_assetKeysKey}. Cannot continue initialization.");
            }

            services.Register(assetKeys);
        }

        public override UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            services.TryGet<ILoggerService>(out var logger);
            logger.LogInformation("[Bootstrap] Asset keys loaded.");
            return UniTask.CompletedTask;
        }
    }
}
