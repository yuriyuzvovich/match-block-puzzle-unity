using System;
using MatchPuzzle.Core.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MatchPuzzle.Features.Background
{
    public class BackgroundPresenter : IBackgroundPresenter
    {
        private readonly IBackgroundView _view;
        private readonly BackgroundSettings _settings;
        private readonly ICameraService _cameraService;
        private readonly ILoggerService _logger;

        public BackgroundPresenter(
            IBackgroundView view,
            BackgroundSettings settings,
            ICameraService cameraService,
            ILoggerService logger
        )
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Initialize()
        {
            if (!_view.SpriteRenderer)
            {
                _logger.LogError("[BackgroundPresenter] SpriteRenderer is not assigned on the view.", _view as Object);
                return;
            }

            _view.SetActive(false);
            AdjustBackgroundSize();
        }

        public void AdjustBackgroundSize()
        {
            var sprite = _view.SpriteRenderer.sprite;
            if (!sprite)
            {
                _logger.LogWarning("[BackgroundPresenter] No sprite assigned to background renderer.", _view as Object);
                return;
            }

            var mainCamera = _cameraService.MainCamera;
            if (!mainCamera)
            {
                _logger.LogError("[BackgroundPresenter] Main camera is missing.", _view as Object);
                return;
            }

            var cameraHeight = mainCamera.orthographicSize * 2f;
            var cameraWidth = cameraHeight * mainCamera.aspect;

            // Calculate scale to cover the entire camera view
            var spriteSize = sprite.bounds.size;
            var scaleX = cameraWidth / spriteSize.x;
            var scaleY = cameraHeight / spriteSize.y;
            var scale = Mathf.Max(scaleX, scaleY);

            _view.SetScale(new Vector3(scale, scale, 1f));

            var cameraPosition = mainCamera.transform.position;
            var backgroundCoords = new Vector3(cameraPosition.x, cameraPosition.y, _view.RootTransform.position.z);

            if (_settings.Align != BackgroundAlign.Center)
            {
                var scaledSpriteSize = spriteSize * scale;

                var halfCameraWidth = cameraWidth * 0.5f;
                var halfCameraHeight = cameraHeight * 0.5f;
                var halfSpriteWidth = scaledSpriteSize.x * 0.5f;
                var halfSpriteHeight = scaledSpriteSize.y * 0.5f;

                switch (_settings.Align)
                {
                    case BackgroundAlign.Top:
                        backgroundCoords.y = cameraPosition.y + halfCameraHeight - halfSpriteHeight;
                        break;
                    case BackgroundAlign.Bottom:
                        backgroundCoords.y = cameraPosition.y - halfCameraHeight + halfSpriteHeight;
                        break;
                    case BackgroundAlign.Right:
                        backgroundCoords.x = cameraPosition.x + halfCameraWidth - halfSpriteWidth;
                        break;
                    case BackgroundAlign.Left:
                        backgroundCoords.x = cameraPosition.x - halfCameraWidth + halfSpriteWidth;
                        break;
                }
            }

            _view.SetPosition(backgroundCoords);

            if (_view.GroundLine)
            {
                var anchorViewportY = _cameraService.GroundAnchorViewportY01;
                var anchorWorldY = _cameraService.GetWorldYAtViewportY01(anchorViewportY);
                var groundWorldY = _view.GroundLine.position.y;
                var deltaY = anchorWorldY - groundWorldY;

                if (!Mathf.Approximately(deltaY, 0f))
                {
                    _view.Move(new Vector3(0f, deltaY, 0f));
                }
            }

            _view.SetActive(true);
        }
    }
}