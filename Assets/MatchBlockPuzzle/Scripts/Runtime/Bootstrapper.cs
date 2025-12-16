using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;
using MatchPuzzle.Features.Background;
using MatchPuzzle.Features.Balloon;
using MatchPuzzle.Features.UI;
using MatchPuzzle.Infrastructure.Bootstrap;
using DG.Tweening;
using MatchPuzzle.Infrastructure.Services;
using UnityEngine;

namespace MatchPuzzle.Infrastructure
{
    /// <summary>
    /// Main bootstrapper - the only component in the GameScene.
    /// Drives the ordered initialization chain for all modules and services.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Asset Keys")]
        [SerializeField] private string _assetKeysKey = "Settings/AssetKeys";
        [SerializeField] private string _balloonAssetKeysKey = "Settings/BalloonAssetKeys";
        [SerializeField] private string _backgroundAssetKeysKey = "Settings/BackgroundAssetKeys";
        [SerializeField] private string _uiAssetKeysKey = "Settings/UIAssetKeys";

        private ServiceContainer _services;
        private BootstrapChain _chain;
        private bool _isInitialized;

        private async void Start()
        {
            await InitializeAsync();
        }

        private async UniTask InitializeAsync()
        {
            if (_isInitialized)
            {
                GetLogger()?.LogWarning("[Bootstrapper] Already initialized. Skipping...", this);
                return;
            }

            _services = new ServiceContainer();
            _chain = BuildChain();
            PrewarmTweening();

            await _chain.RunAsync(_services, this.GetCancellationTokenOnDestroy());

            _isInitialized = true;
            GetLogger()?.LogInformation("[Bootstrapper] Initialization complete!", this);
        }

        private BootstrapChain BuildChain()
        {
            return new BootstrapChain()
                .AddStep(new AssetServiceStep())
                .AddStep(new AssetKeysStep(_assetKeysKey))
                .AddStep(new LoggingStep())
                .AddStep(new SettingsLoadingStep())
                .AddStep(new LevelRepositoryStep())
                .AddStep(new CoreServicesStep())
                .AddStep(new CameraStep())
                .AddStep(new PoolStep())
                .AddStep(new MatchPuzzleAppStep())
                .AddStep(new GridPresentationSetupStep())
                .AddStep(new UIStep(_uiAssetKeysKey))
                .AddStep(new BackgroundStep(_backgroundAssetKeysKey))
                .AddStep(new BalloonStep(_balloonAssetKeysKey))
                .AddStep(new PoolWarmupStep())
                .AddStep(new MatchPuzzleGameControllerStep())
                .AddStep(new StartGameStep());
        }

        private void PrewarmTweening()
        {
            // Prevent DOTween from initializing on the first block animation.
            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);

            // Pre-allocate enough tweeners for the grid so we avoid runtime resizes.
            DOTween.SetTweensCapacity(256, 64);
        }

        private void OnDestroy()
        {
            if (_services == null)
            {
                return;
            }

            var logger = GetLogger();
            logger?.LogInformation("[Bootstrapper] Cleaning up...", this);

            _chain?.Cleanup(_services);
            logger?.LogInformation("[Bootstrapper] Cleanup complete.", this);
            _services.Clear();
        }

        private ILoggerService GetLogger()
        {
            return _services != null && _services.TryGet<ILoggerService>(out var logger) ? logger : null;
        }
    }
}
