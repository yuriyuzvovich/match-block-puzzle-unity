using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    public class CameraView : MonoBehaviour, ICameraView
    {
        private CameraSettings _cameraSettings;

        [SerializeField] private Camera _camera;

        private GameObject _cachedGameObject;
        private Transform _cachedTransform;

        public Camera Camera => _camera;
        public GameObject CameraGameObject => _cachedGameObject ??= gameObject;
        public Transform CameraTransform => _cachedTransform ??= transform;

        private void Awake()
        {
            if (!_camera) throw new System.Exception("[CameraView] Camera component is not assigned.");
        }

        public void SetOrthographicSize(float size) => _camera.orthographicSize = size;
        public void SetPosition(Vector3 position) => transform.position = position;

        /// <inheritdoc/>
        public void Initialize(CameraSettings cameraSettings)
        {
            _cameraSettings = cameraSettings;

            if (!_camera)
            {
                _camera = GetComponent<Camera>();
            }

            // Apply camera settings
            if (_cameraSettings)
            {
                _camera.orthographic = true;
                transform.position = _cameraSettings.InitialPosition;
                _camera.backgroundColor = _cameraSettings.BackgroundColor;
                _camera.clearFlags = _cameraSettings.ClearFlags;
                _camera.cullingMask = _cameraSettings.CullingMask;
                _camera.depth = _cameraSettings.Depth;
                _camera.nearClipPlane = _cameraSettings.NearClipPlane;
                _camera.farClipPlane = _cameraSettings.FarClipPlane;
                gameObject.tag = _cameraSettings.CameraTag;
            }
        }
    }
}