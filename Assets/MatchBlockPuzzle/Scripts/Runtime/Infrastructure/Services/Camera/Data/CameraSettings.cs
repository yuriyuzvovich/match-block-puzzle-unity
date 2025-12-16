using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "MatchPuzzle/Camera Settings")]
    public class CameraSettings : ScriptableObject
    {
        [Header("Appearance")]
        [Tooltip("Camera background color")]
        [SerializeField] private Color _backgroundColor = new Color(0.1f, 0.1f, 0.15f);

        [Header("Camera Position")]
        [Tooltip("Initial camera position")]
        [SerializeField] private Vector3 _initialPosition = new Vector3(0, 0, -10f);

        [Header("Rendering")]
        [Tooltip("Camera depth (lower values render first)")]
        [SerializeField] private int _depth = -1;

        [Tooltip("Camera clear flags")]
        [SerializeField] private CameraClearFlags _clearFlags = CameraClearFlags.SolidColor;

        [Tooltip("Culling mask for what layers the camera sees")]
        [SerializeField] private LayerMask _cullingMask = -1; // All layers

        [Header("Advanced")]
        [Tooltip("Near clipping plane")]
        [SerializeField] private float _nearClipPlane = 0.3f;

        [Tooltip("Far clipping plane")]
        [SerializeField] private float _farClipPlane = 1000f;

        [Tooltip("Viewport Y (0 = bottom, 1 = top) where GroundWorldY should appear")]
        [Range(0f, 1f)] [SerializeField] private  float _groundAnchorViewportY01 = 0.18f;

        [Tooltip("Normalized viewport rect (0-1) where the grid must fit")]
        [SerializeField] private Rect _gridViewportRect01 = new Rect(0.05f, 0.25f, 0.9f, 0.6f);

        [Header("Camera Tag")]
        [SerializeField] private string _cameraTag = "MainCamera";

        public float FarClipPlane => _farClipPlane;
        public Rect GridViewportRect01 => _gridViewportRect01;
        public string CameraTag => _cameraTag;
        public float GroundAnchorViewportY01 => _groundAnchorViewportY01;
        public float NearClipPlane => _nearClipPlane;
        public Color BackgroundColor => _backgroundColor;
        public Vector3 InitialPosition => _initialPosition; 
        public int Depth => _depth;
        public CameraClearFlags ClearFlags => _clearFlags;
        public LayerMask CullingMask => _cullingMask;
    }
}
